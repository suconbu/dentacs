using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Suconbu.Dentacs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public enum ErrorState { None, ErrorWithoutMessage, ErrorWithMessage }

        public ReactivePropertySlim<string> RxResult { get; }
        public ReactivePropertySlim<string> RxErrorText { get; }
        public ReactivePropertySlim<string> RxUsageText { get; }
        public ReadOnlyReactivePropertySlim<ErrorState> RxErrorState { get; }
        public ReadOnlyReactivePropertySlim<double> RxZoomRatio { get; }
        public ReadOnlyReactivePropertySlim<double> RxMildZoomRatio { get; }
        public ReadOnlyReactivePropertySlim<double> RxMildMildZoomRatio { get; }
        public ReactivePropertySlim<int> RxZoomIndex { get; }
        public ReadOnlyReactivePropertySlim<string> RxTitleText { get; }
        public ReactivePropertySlim<bool> RxFullScreenEnabled { get; }
        public ReactivePropertySlim<bool> RxKeypadEnabled { get; }
        public ReactivePropertySlim<string> RxCurrentText { get; }
        public ReactivePropertySlim<int> RxSelectionLength { get; }
        public ReadOnlyReactivePropertySlim<bool> RxCaptionVisible { get; }
        public ReadOnlyReactivePropertySlim<bool> RxStatusVisible { get; }
        public ReadOnlyReactivePropertySlim<bool> RxKeypadVisible { get; }
        public ReadOnlyReactivePropertySlim<bool> RxFullScreenKeypadVisible { get; }
        public ReadOnlyReactivePropertySlim<bool> RxErrorTextVisible { get; }
        public ReadOnlyReactivePropertySlim<bool> RxCharInfoVisible { get; }
        public Color AccentColor = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

        readonly Calculator calculator = new Calculator(CultureInfo.CurrentCulture);
        int lastCalculatedLineIndex = -1;
        int zoomIndexBackup = 2;
        int fullScreenZoomIndexBackup = 6;
        double maxWidthBackup = SystemParameters.WorkArea.Width;

        static readonly double[] kZoomTable = new[] { 0.5, 0.75, 1.0, 1.5, 2.0, 3.0, 4.0 };
        static readonly int kCopyFlashInterval = 100;
        readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.RxResult = new ReactivePropertySlim<string>();
            this.RxErrorText = new ReactivePropertySlim<string>();
            this.RxErrorState = this.RxErrorText.Select(x =>
                x == null ? ErrorState.None :
                x == string.Empty ? ErrorState.ErrorWithoutMessage :
                ErrorState.ErrorWithMessage)
                .ToReadOnlyReactivePropertySlim();
            this.RxUsageText = new ReactivePropertySlim<string>();
            this.RxZoomIndex = new ReactivePropertySlim<int>();
            this.RxZoomRatio = this.RxZoomIndex.Select(x => kZoomTable[x]).ToReadOnlyReactivePropertySlim();
            this.RxMildZoomRatio = this.RxZoomRatio.Select(x => Math.Pow(x, 1 / 3.0)).ToReadOnlyReactivePropertySlim();
            this.RxMildMildZoomRatio = this.RxZoomRatio.Select(x => Math.Pow(x, 1 / 5.0)).ToReadOnlyReactivePropertySlim();
            this.RxTitleText = this.RxZoomRatio.Select(_ => this.MakeTitleText()).ToReadOnlyReactivePropertySlim();
            this.RxFullScreenEnabled = new ReactivePropertySlim<bool>(false);
            this.RxFullScreenEnabled.Subscribe(x => this.IsFullScreenChanged(x));
            this.RxKeypadEnabled = new ReactivePropertySlim<bool>();
            this.RxCurrentText = new ReactivePropertySlim<string>();
            this.RxSelectionLength = new ReactivePropertySlim<int>();

            this.RxCaptionVisible = this.RxFullScreenEnabled.Select(x => !x).ToReadOnlyReactivePropertySlim();
            this.RxStatusVisible = this.RxFullScreenEnabled.Select(x => !x).ToReadOnlyReactivePropertySlim();
            this.RxCharInfoVisible = this.RxUsageText.Select(x => string.IsNullOrEmpty(x)).ToReadOnlyReactivePropertySlim();
            this.RxKeypadVisible = Observable.CombineLatest(
                this.RxFullScreenEnabled, this.RxKeypadEnabled, (f, k) => !f && k)
                .ToReadOnlyReactivePropertySlim();
            this.RxFullScreenKeypadVisible = Observable.CombineLatest(
                this.RxFullScreenEnabled, this.RxKeypadEnabled, (f, k) => f && k)
                .ToReadOnlyReactivePropertySlim();
            this.RxErrorTextVisible = Observable.CombineLatest(
                this.RxErrorText, this.RxUsageText, (e, u) => !string.IsNullOrEmpty(e) && string.IsNullOrEmpty(u))
                .ToReadOnlyReactivePropertySlim();

            this.KeypadPanel.ItemMouseEnter += (s, item) => { this.RxUsageText.Value = item.Usage; };
            this.KeypadPanel.ItemMouseLeave += (s, item) => { this.RxUsageText.Value = null; };
            this.KeypadPanel.ItemClick += (s, item) => this.KeypadPanel_ItemClick(item);

            this.FullScreenKeypadPanel.ItemMouseEnter += (s, item) => { this.RxUsageText.Value = item.Usage; };
            this.FullScreenKeypadPanel.ItemMouseLeave += (s, item) => { this.RxUsageText.Value = null; };
            this.FullScreenKeypadPanel.ItemClick += (s, item) => this.KeypadPanel_ItemClick(item);
        }

        string MakeTitleText()
        {
            return "dentacs" +
                (this.RxZoomRatio.Value != 1.0 ? $" {this.RxZoomRatio.Value * 100:#}%" : "");
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            this.InputTextBox.Focus();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                this.ChangeZoom(0 < e.Delta ? +1 : -1);
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            var handled = true;
            var control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (control && e.Key == Key.PageUp)
            {
                this.ChangeZoom(+1);
            }
            else if (control && e.Key == Key.PageDown)
            {
                this.ChangeZoom(-1);
            }
            else if (e.Key == Key.F11)
            {
                // Toggle full-screen
                this.RxFullScreenEnabled.Value = !this.RxFullScreenEnabled.Value;
            }
            else if (e.Key == Key.Tab)
            {
                // Toggle keypad
                this.RxKeypadEnabled.Value = !this.RxKeypadEnabled.Value;
            }
            else
            {
                handled = false;
            }

            e.Handled = handled;
        }

        void ChangeZoom(int offset)
        {
            this.RxZoomIndex.Value = Math.Max(0, Math.Min(this.RxZoomIndex.Value + offset, kZoomTable.Length - 1));
        }

        void KeypadPanel_ItemClick(KeypadPanel.Item item)
        {
            var target = this.InputTextBox;

            if (item.Type == KeypadPanel.KeyType.BackSpace)
            {
                System.Windows.Forms.SendKeys.SendWait("{BACKSPACE}");
            }
            else if (item.Type == KeypadPanel.KeyType.Undo)
            {
                target.Undo();
            }
            else if (item.Type == KeypadPanel.KeyType.Redo)
            {
                target.Redo();
            }
            else if (item.Type == KeypadPanel.KeyType.Clear)
            {
                target.Clear();
            }
            else if (item.Type == KeypadPanel.KeyType.SelectAll)
            {
                target.SelectAll();
            }
            else if (item.Type == KeypadPanel.KeyType.Convert)
            {
                this.ConvertItemClicked(target, (int)item.Data);
            }
            else if (item.Type == KeypadPanel.KeyType.Function)
            {
                this.FunctionItemClicked(target, (string)item.Data);
            }
            else if (item.Type == KeypadPanel.KeyType.Operator || item.Type == KeypadPanel.KeyType.Constant)
            {
                this.StringItemClicked(target, (string)item.Data);
            }
            else
            {
                // NOOP
            }
        }

        void ConvertItemClicked(TextBox target, int radix)
        {
            this.calculator.Reset();

            var lines = LineString.Split(target.Text).ToArray();
            var selectionStart = target.SelectionStart;
            var selectionEnd = target.SelectionStart + target.SelectionLength;
            TextBoxHelper.GetStartEndLineIndex(lines, selectionStart, selectionEnd,
                out var lineIndexStart, out var lineIndexEnd);

            for (int i = lineIndexStart; i <= lineIndexEnd; i++)
            {
                if (this.calculator.Calculate(lines[i].Text))
                {
                    if (decimal.TryParse(this.calculator.Result, out var value))
                    {
                        lines[i].Text = ResultConvertHelper.ConvertToResultString(value, radix, ResultConvertHelper.Styles.Prefix);
                    }
                }
            }

            target.Text = LineString.Join(lines);
            target.SelectionStart = TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, lineIndexStart);
            target.SelectionLength = TextBoxHelper.GetCharacterIndexOfLineEndFromLineIndex(lines, lineIndexEnd) - target.SelectionStart;
        }

        void FunctionItemClicked(TextBox target, string name)
        {
            var lines = LineString.Split(target.Text).ToArray();
            var selectionStart = target.SelectionStart;
            var selectionEnd = target.SelectionStart + target.SelectionLength;
            TextBoxHelper.GetStartEndLineIndex(lines, selectionStart, selectionEnd,
                out var lineIndexStart, out var lineIndexEnd);

            if (lineIndexStart < lineIndexEnd)
            {
                // Multi lines are selected
                for(int i = lineIndexStart; i <= lineIndexEnd; i++)
                {
                    lines[i].Text = $"{name}({lines[i].Text})";
                }

                target.Text = LineString.Join(lines);
                target.SelectionStart = TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, lineIndexStart);
                target.SelectionLength = TextBoxHelper.GetCharacterIndexOfLineEndFromLineIndex(lines, lineIndexEnd) - target.SelectionStart;
            }
            else
            {
                var prevSelectedLength = target.SelectedText.Length;
                if (0 < name.Length &&
                    selectionEnd < target.Text.Length &&
                    target.Text[selectionEnd] == '(')
                {
                    // xxxx(... -> FUNC(...
                    // ~~~~             |
                    target.SelectedText = name;
                    target.SelectionLength = 0;
                }
                else
                {
                    // xxxx -> xFUNC(xx)x
                    //  ~~           ~~
                    target.SelectedText = $"{name}({target.SelectedText})";
                    target.SelectionLength = prevSelectedLength;
                }
                target.SelectionStart += name.Length + 1;
            }
        }

        void StringItemClicked(TextBox target, string text)
        {
            if (0 < target.SelectionLength)
            {
                // Keep the selection on replacing text
                target.SelectedText = text;
            }
            else
            {
                target.SelectedText = text;
                target.SelectionLength = 0;
                target.SelectionStart += text.Length;
            }
        }

        void IsFullScreenChanged(bool enable)
        {
            if (enable)
            {
                this.zoomIndexBackup = this.RxZoomIndex.Value;
                this.RxZoomIndex.Value = this.fullScreenZoomIndexBackup;
                this.maxWidthBackup = this.MaxWidth;
                this.MaxWidth = Double.PositiveInfinity;
                this.WindowBorder.BorderThickness = new Thickness(0);
                this.SizeToContent = SizeToContent.Manual;
                //this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.Topmost = false;
            }
            else
            {
                if (this.IsLoaded)
                {
                    this.fullScreenZoomIndexBackup = this.RxZoomIndex.Value;
                }
                //this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                this.Topmost = true;
                this.MaxWidth = this.maxWidthBackup;
                this.WindowBorder.BorderThickness = new Thickness(1);
                this.RxZoomIndex.Value = this.zoomIndexBackup;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lastCalculatedLineIndex = -1;
        }

        private void InputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;

            var caretIndex = this.InputTextBox.CaretIndex;
            var lineIndex = this.InputTextBox.GetLineIndexFromCharacterIndex(caretIndex);
            var lines = LineString.Split(this.InputTextBox.Text).ToArray();
            // GetLineText and GetCharacterIndexFromLineIndex does not work correctly in full-screen mode.
            var currentLine = lines[lineIndex]; //this.InputTextBox.GetLineText(lineIndex);
            var selectedText = this.InputTextBox.SelectedText;

            if (lineIndex != this.lastCalculatedLineIndex || this.RxSelectionLength.Value != selectedText.Length)
            {
                // Recalculate current line
                this.calculator.Reset();
                for (int i = 0; i <= lineIndex; i++)
                {
                    this.calculator.Calculate(lines[i].Text);
                }
                if (0 < selectedText.Length)
                {
                    this.calculator.Calculate(selectedText);
                }
                if (this.calculator.Error == null)
                {
                    this.RxResult.Value = this.calculator.Result.ToString();
                }
                this.RxErrorText.Value = this.calculator.Error;
                this.lastCalculatedLineIndex = lineIndex;
            }

            if (selectedText.Length == 0)
            {
                int lineStartCharIndex = TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, lineIndex);
                int charIndexOfLine = caretIndex - lineStartCharIndex;
                var offset = (charIndexOfLine < currentLine.Text.Length) ? 0 : -1;
                selectedText = CharInfoConvertHelper.GetUnicodeElement(currentLine.Text, charIndexOfLine + offset);
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
            Clipboard.SetText(copyText);
            var visibilityBackup = textBox.Visibility;
            textBox.Visibility = Visibility.Hidden;
            Task.Delay(kCopyFlashInterval).ContinueWith(x => { this.Dispatcher.Invoke(() => { textBox.Visibility = visibilityBackup; }); });
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

        private void CharInfo_Click(object sender, RoutedEventArgs e)
        {
            var text = CharInfoConvertHelper.ConvertToElementInfoTableString(this.RxCurrentText.Value);
            Clipboard.SetText(text);
            var item = sender as Control;
            item.Visibility = Visibility.Hidden;
            Task.Delay(kCopyFlashInterval).ContinueWith(x => { this.Dispatcher.Invoke(() => { item.Visibility = Visibility.Visible; }); });
        }

        private void FullScreenCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.RxFullScreenEnabled.Value = false;
        }

        private void Caption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.RxFullScreenEnabled.Value = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
