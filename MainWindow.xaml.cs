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

namespace Suconbu.Dentacs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ReactiveProperty<string> Expression { get; private set; }
        public ReactiveProperty<string> Result { get; private set; }
        public ReactiveProperty<bool> IsResultEnabled { get; private set; }

        Calculator calculator = Calculator.GetInstance();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Expression = ReactiveProperty.FromObject(this.calculator, x => x.Expression);
            this.Result = this.calculator.ObserveProperty(x => x.Result).ToReactiveProperty();
            this.IsResultEnabled = this.calculator.ObserveProperty(x => x.IsResultEnabled).ToReactiveProperty();

            this.InputTextBox.Focus();
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
}
