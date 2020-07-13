using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Reactive.Bindings;

namespace Suconbu.Dentacs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public enum ErrorState { None, NoMessage, WithMessage }

        public ReactiveProperty<string> RxResult { get; private set; }
        public ReactiveProperty<string> RxError { get; private set; }
        public ReadOnlyReactivePropertySlim<ErrorState> RxErrorState { get; private set; }
        public ReactiveProperty<double> RxZoom { get; private set; }
        public ReactiveProperty<int> RxZoomIndex { get; private set; }
        public ReadOnlyReactivePropertySlim<string> RxTitleText { get; private set; }
        public ReactiveProperty<bool> RxIsFullScreen { get; private set; }
        public ReactiveProperty<string> RxCurrentText { get; private set; }
        public ReactiveProperty<int> RxSelectionLength { get; private set; }

        readonly Calculator calculator = new Calculator();
        int lastLineIndex = -1;
        readonly double[] zoomTable = new []{ 0.5, 1.0, 1.5, 2.0, 3.0 };
        int zoomIndexBackup = 1;
        double maxWidthBackup = System.Windows.SystemParameters.WorkArea.Width;

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

            this.RxResult = new ReactiveProperty<string>();
            this.RxError = new ReactiveProperty<string>();
            this.RxErrorState = this.RxError.Select(x =>
                x == null ? ErrorState.None :
                x == string.Empty ? ErrorState.NoMessage :
                ErrorState.WithMessage)
                .ToReadOnlyReactivePropertySlim();
            this.RxZoom = new ReactiveProperty<double>();
            this.RxZoomIndex = new ReactiveProperty<int>();
            this.RxZoom = this.RxZoomIndex.Select(x => this.zoomTable[x]).ToReactiveProperty();
            this.RxTitleText = this.RxZoom.Select(_ => this.MakeTitleText()).ToReadOnlyReactivePropertySlim();
            this.RxIsFullScreen = new ReactiveProperty<bool>(false);
            this.RxIsFullScreen.Subscribe(x => this.SetFullScreen(x));
            this.RxCurrentText = new ReactiveProperty<string>();
            this.RxSelectionLength = new ReactiveProperty<int>();
        }

        string MakeTitleText()
        {
            return "dentacs" +
                (this.RxZoom.Value != 1.0 ? $" {this.RxZoom.Value * 100:#}%" : "");
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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                this.ChangeZoom(0 < e.Delta ? +1 : -1);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.PageUp) this.ChangeZoom(+1);
                if (e.Key == Key.PageDown) this.ChangeZoom(-1);
            }
            else if (e.Key == Key.F11)
            {
                // Toggle full-screen
                this.RxIsFullScreen.Value = !this.RxIsFullScreen.Value;
            }
        }

        void ChangeZoom(int offset)
        {
            this.RxZoomIndex.Value = Math.Clamp(this.RxZoomIndex.Value + offset, 0, this.zoomTable.Length - 1);
        }

        void SetFullScreen(bool enable)
        {
            if (enable)
            {
                this.zoomIndexBackup = this.RxZoomIndex.Value;
                this.RxZoomIndex.Value = this.zoomTable.Length - 1;
                this.maxWidthBackup = this.MaxWidth;
                this.MaxWidth = Double.PositiveInfinity;
                this.SizeToContent = SizeToContent.Manual;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                this.MaxWidth = this.maxWidthBackup;
                this.RxZoomIndex.Value = this.zoomIndexBackup;
            }
        }

        protected virtual void OnMouseDoubleClickNC(NcMouseEventArgs e)
        {
            if (e.HitPosition == NcMouseEventArgs.Position.HTCAPTION)
            {
                this.RxIsFullScreen.Value = true;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lastLineIndex = -1;
        }

        private void InputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;

            var caretIndex = this.InputTextBox.CaretIndex;
            var lineIndex = this.InputTextBox.GetLineIndexFromCharacterIndex(caretIndex);
            var lines = this.InputTextBox.Text.Split(Environment.NewLine);
            // GetLineText does not work in fullscreen mode.
            var currentLine = lines[lineIndex]; //this.InputTextBox.GetLineText(lineIndex);
            var selectedText = this.InputTextBox.SelectedText;

            if (lineIndex != this.lastLineIndex || this.RxSelectionLength.Value != selectedText.Length)
            {
                this.calculator.Reset();
                for (int i = 0; i <= lineIndex; i++)
                {
                    this.calculator.Calculate(lines[i]);
                }
                if (0 < selectedText.Length)
                {
                    this.calculator.Calculate(selectedText);
                }
                if (this.calculator.Error == null)
                {
                    this.RxResult.Value = this.calculator.Result.ToString();
                }
                this.RxError.Value = this.calculator.Error;
                this.lastLineIndex = lineIndex;
            }

            if (selectedText.Length == 0)
            {
                int lineStartIndex = lines.Take(lineIndex).Select(line => line.Length + Environment.NewLine.Length).Sum();
                int charIndexOfLine = caretIndex - lineStartIndex;
                var offset = (charIndexOfLine < currentLine.Length) ? 0 : -1;
                selectedText = CharInfoConvertHelper.GetUnicodeElement(currentLine, charIndexOfLine + offset);
            }
            this.RxCurrentText.Value = selectedText;
            this.RxSelectionLength.Value = this.InputTextBox.SelectionLength;
        }

        public static RoutedCommand CopyCommand = new RoutedCommand();

        void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Blink the selection area to show the value has been copied
            var textBox = e.Parameter as TextBox;
            var copyText = this.GetResultTextForCopy(textBox);
            if (copyText == null) return;
            textBox.Focus();
            textBox.SelectAll();
            Clipboard.SetText(copyText);
            Task.Delay(100).ContinueWith(x => { this.Dispatcher.Invoke(() => { textBox.SelectionLength = 0; }); });
        }

        void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.GetResultTextForCopy(e.Parameter as TextBox) != null);
        }

        string GetResultTextForCopy(TextBox textBox)
        {
            if (textBox == null) return null;
            if (!int.TryParse((string)textBox.Tag, out var radix)) return null;
            if (!decimal.TryParse(this.RxResult.Value, out var number)) return null;
            return ResultConvertHelper.ConvertToResultString(number, radix, ResultConvertHelper.Styles.Prefix);
        }

        private void StatusBarItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var text = CharInfoConvertHelper.ConvertToElementInfoString(this.RxCurrentText.Value, false);
            Clipboard.SetText(text);
        }

        private void CloseFullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            this.RxIsFullScreen.Value = false;
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
