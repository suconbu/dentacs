using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Memezo = Suconbu.Scripting.Memezo;
using System.ComponentModel;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Interop;
using System.Reactive.Linq;

namespace Suconbu.Dentacs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public ReactiveProperty<string> Expression { get; private set; }
        public ReadOnlyReactiveProperty<string> Result { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsResultEnabled { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsTopmost { get; private set; }
        public ReadOnlyReactiveProperty<string> CaptionText { get; private set; }

        Calculator calculator = Calculator.GetInstance();

        static IntPtr WndProc(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message == 0x00A3/*WM_NCLBUTTONDBLCLK*/)
            {
                var window = HwndSource.FromHwnd(hWnd).RootVisual as MainWindow;
                if (window != null)
                {
                    window.OnMouseDoubleClickNC(new NcMouseEventArgs(wParam, lParam, MouseButton.Left));
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Expression = ReactiveProperty.FromObject(this.calculator, x => x.Expression);
            this.Result = this.calculator.ObserveProperty(x => x.Result).ToReadOnlyReactiveProperty();
            this.IsResultEnabled = this.calculator.ObserveProperty(x => x.IsResultEnabled).ToReadOnlyReactiveProperty();
            this.IsTopmost = this.ObserveProperty(x => x.Topmost).ToReadOnlyReactiveProperty();
            this.CaptionText = this.IsTopmost.Select(x => (string)this.FindResource(x ? "CaptionTopmost" : "Caption")).ToReadOnlyReactiveProperty();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hs = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hs.AddHook(new HwndSourceHook(WndProc));
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            this.InputTextBox.Focus();
        }

        protected virtual void OnMouseDoubleClickNC(NcMouseEventArgs e)
        {
            if (e.HitPosition == NcMouseEventArgs.Position.HTCAPTION)
            {
                this.Topmost = !this.Topmost;
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.Topmost)));
            }
        }

        void CopyTextToClipboard(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            // コピーしたことがわかるようチカっとさせる
            textBox.Focus();
            textBox.SelectAll();
            Clipboard.SetText(textBox.Text);
            Task.Delay(100).ContinueWith(x => { this.Dispatcher.Invoke(() => { textBox.SelectionLength = 0; }); });
        }

        private void InputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;

            var lineIndex = this.InputTextBox.GetLineIndexFromCharacterIndex(this.InputTextBox.CaretIndex);
            this.Expression.Value = this.InputTextBox.GetLineText(lineIndex);
        }

        private void DecButton_Click(object sender, RoutedEventArgs e)
        {
            this.CopyTextToClipboard(this.DecResult);
        }

        private void HexButton_Click(object sender, RoutedEventArgs e)
        {
            this.CopyTextToClipboard(this.HexResult);
        }

        private void BinButton_Click(object sender, RoutedEventArgs e)
        {
            this.CopyTextToClipboard(this.BinResult);
        }
    }

    public class NcMouseEventArgs : EventArgs
    {
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest
        public enum Position
        {
            HTERROR = -2,       // On the screen background or on a dividing line between windows (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).
            HTTRANSPARENT = -1, // In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
            HTNOWHERE = 0,      // On the screen background or on a dividing line between windows.
            HTCLIENT = 1,       // In a client area.
            HTCAPTION = 2,      // In a title bar.
            HTSYSMENU = 3,      // In a window menu or in a Close button in a child window.
            HTSIZE = 4,         // In a size box (same as HTGROWBOX).
            HTGROWBOX = 4,      // In a size box (same as HTSIZE).
            HTMENU = 5,         // In a menu.
            HTHSCROLL = 6,      // In a horizontal scroll bar.
            HTREDUCE = 8,       // In a Minimize button.
            HTMINBUTTON = 8,    // In a Minimize button.
            HTZOOM = 9,         // In a Maximize button. 
            HTMAXBUTTON = 9,    // In a Maximize button.
            HTLEFT = 10,        // In the left border of a resizable window (the user can click the mouse to resize the window horizontally).
            HTRIGHT = 11,       // In the right border of a resizable window (the user can click the mouse to resize the window horizontally).
            HTTOP = 12,         // In the upper-horizontal border of a window.
            HTTOPLEFT = 13,     // In the upper-left corner of a window border.
            HTTOPRIGHT = 14,    // In the upper-right corner of a window border.
            HTVSCROLL = 7,      // In the vertical scroll bar.
            HTBOTTOM = 15,      // In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).
            HTBOTTOMLEFT = 16,  // In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            HTBOTTOMRIGHT = 17, // In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            HTBORDER = 18,      // In the border of a window that does not have a sizing border.
            HTCLOSE = 20,       // In a Close button.
            HTHELP = 21,        // In a Help button.
        }
        public NcMouseEventArgs(IntPtr wParam, IntPtr lParam, MouseButton button)
        {
            this.HitPosition = (NcMouseEventArgs.Position)wParam;
            this.Location = new Point((lParam.ToInt32() & 0xFFFF0000 >> 16), lParam.ToInt32() & 0xFFFF);
            this.Button = button;
            this.Timestamp = Environment.TickCount;
        }
        public Position HitPosition { get; private set; }
        public Point Location { get; private set; }
        public double X { get { return this.Location.X; } }
        public double Y { get { return this.Location.Y; } }
        public MouseButton Button { get; private set; }
        public int Timestamp { get; set; }
    }
}
