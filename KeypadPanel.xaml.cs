using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Suconbu.Dentacs
{
    /// <summary>
    /// KeypadPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class KeypadPanel : UserControl
    {
        public enum Command { None, BackSpace, Undo, Redo, Clear }

        public struct Item
        {
            public Command Command { get; }
            public string Label { get; }
            public string Usage { get; }
            public string Data { get; }

            public Item(string label, string usageKey, string data = null)
            {
                this.Label = label;
                this.Usage = Application.Current.TryFindResource(usageKey) as string;
                this.Data = data ?? label;
                this.Command = Command.None;
            }

            public Item(string label, string usageKey, Command command)
            {
                this.Label = label;
                this.Usage = Application.Current.TryFindResource(usageKey) as string;
                this.Data = string.Empty;
                this.Command = command;
            }
        }

        public event EventHandler<Item> ItemMouseEnter = delegate { };
        public event EventHandler<Item> ItemMouseLeave = delegate { };
        public event EventHandler<Item> ItemClick = delegate { };

        Item[] keypadItems;
        int keypadRowCount;
        int keypadColumnCount;

        public KeypadPanel()
        {
            InitializeComponent();

            this.SetupPanel();
        }

        public void Rebuild()
        {
            this.Container.Children.Clear();
            this.SetupPanel();
        }

        private void SetupPanel()
        {
            this.keypadItems = this.CreateItems();
            this.keypadRowCount = 4;
            this.keypadColumnCount = this.keypadItems.Length / this.keypadRowCount;

            for (int i = 0; i < this.keypadRowCount; i++)
            {
                var row = new RowDefinition() { Height = new GridLength(0.0, GridUnitType.Star) };
                this.Container.RowDefinitions.Add(row);
            }
            for (int i = 0; i < this.keypadColumnCount; i++)
            {
                var column = new ColumnDefinition() { Width = GridLength.Auto };
                this.Container.ColumnDefinitions.Add(column);
            }
            for (int i = 0; i < this.keypadItems.Length; i++)
            {
                var item = this.keypadItems[i];
                var style = (item.Command == Command.Clear) ?
                    this.FindResource("KeypadClearButtonStyle") as Style :
                    this.FindResource("KeypadButtonStyle") as Style;
                var button = new Button()
                {
                    Content = item.Label,
                    Style = style
                };
                button.Click += (s, e) => this.ItemClick(this, item);
                var border = new Border()
                {
                    Style = this.FindResource("KeypadButtonBorderStyle") as Style,
                    Child = button
                };
                border.MouseEnter += (s, e) => this.ItemMouseEnter(this, item);
                border.MouseLeave += (s, e) => this.ItemMouseLeave(this, item);
                Grid.SetRow(border, i % this.keypadRowCount);
                Grid.SetColumn(border, i / this.keypadRowCount);
                this.Container.Children.Add(border);
            }
        }

        private Item[] CreateItems()
        {
            return new[]
            {
                new Item("/", "Keypad.Division"),
                new Item("*", "Keypad.Multiplication"),
                new Item("-", "Keypad.Subtraction"),
                new Item("+", "Keypad.Addition"),

                new Item("//", "Keypad.IntegerDivision"),
                new Item("**", "Keypad.Exponentiation"),
                new Item("0x", "Keypad.HexPrefix"),
                new Item("0b", "Keypad.BinPrefix"),

                new Item("%", "Keypad.Reminder"),
                new Item("( )", "Keypad.Parentheses", "()"),
                new Item("<<", "Keypad.BitwiseLeftShift"),
                new Item(">>", "Keypad.BitwiseRightShift"),

                new Item("&", "Keypad.BitwiseAnd"),
                new Item("|", "Keypad.BitwiseOr"),
                new Item("^", "Keypad.BitwiseXor"),
                new Item("~", "Keypad.BitwiseNot"),

                new Item("trunc", "Keypad.Trunc", "trunc()"),
                new Item("floor", "Keypad.Floor", "floor()"),
                new Item("ceil", "Keypad.Ceil", "ceil()"),
                new Item("round", "Keypad.Round", "round()"),

                new Item("sin", "Keypad.Sin", "sin()"),
                new Item("cos", "Keypad.Cos", "cos()"),
                new Item("tan", "Keypad.Tan", "tan()"),
                new Item("PI", "Keypad.PI"),

                new Item("asin", "Keypad.Asin", "asin()"),
                new Item("acos", "Keypad.Acos", "acos()"),
                new Item("atan", "Keypad.Atan", "atan()"),
                new Item("atan2", "Keypad.Atan2", "atan2()"),

                new Item("log10", "Keypad.Log10", "log10()"),
                new Item("log2", "Keypad.Log2", "log2()"),
                new Item("log", "Keypad.Log", "log()"),
                new Item("E", "Keypad.E"),

                new Item("CLR", "Keypad.Clear", Command.Clear),
                new Item("↪", "Keypad.Undo", Command.Undo),
                new Item("↩", "Keypad.Redo", Command.Redo),
                new Item("BS", "Keypad.BackSpace", Command.BackSpace),
            };
        }
    }
}
