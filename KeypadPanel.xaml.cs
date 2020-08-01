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
        public enum KeyType { Blank, Text, Function, Convert, BackSpace, Undo, Redo, Clear, SelectAll }

        // KeyType      | Content of 'Data'
        // -------------|----------------------
        // Blank        | Not in use
        // Text         | String of text
        // Function     | Function name
        // Convert      | Radix of result value
        // BackSpace    | Not in use
        // Undo         | Not in use
        // Redo         | Not in use
        // Clear        | Not in use
        // SelectAll    | Not in use

        public struct Item
        {
            public KeyType Type { get; }
            public string Label { get; }
            public string Usage { get; }
            public object Data { get; }

            public Item(KeyType type, string usageKey, string label, object data = null)
            {
                this.Type = type;
                this.Usage = Application.Current.TryFindResource(usageKey) as string;
                this.Label = label;
                this.Data = data ?? label;
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
                var style = (item.Type == KeyType.Clear) ?
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
                new Item(KeyType.Text, "Keypad.Division", "/"),
                new Item(KeyType.Text, "Keypad.Multiplication", "*"),
                new Item(KeyType.Text, "Keypad.Subtraction", "-"),
                new Item(KeyType.Text, "Keypad.Addition", "+"),

                new Item(KeyType.Text, "Keypad.IntegerDivision", "//"),
                new Item(KeyType.Text, "Keypad.Exponentiation", "**"),
                new Item(KeyType.Text, "Keypad.HexPrefix", "0x"),
                new Item(KeyType.Text, "Keypad.BinPrefix", "0b"),

                new Item(KeyType.Text, "Keypad.Reminder", "%"),
                new Item(KeyType.Function, "Keypad.Parentheses", "( )", ""),
                new Item(KeyType.Text, "Keypad.BitwiseLeftShift", "<<"),
                new Item(KeyType.Text, "Keypad.BitwiseRightShift", ">>"),

                new Item(KeyType.Text, "Keypad.BitwiseAnd", "&"),
                new Item(KeyType.Text, "Keypad.BitwiseOr", "|"),
                new Item(KeyType.Text, "Keypad.BitwiseXor", "^"),
                new Item(KeyType.Text, "Keypad.BitwiseNot", "~"),

                new Item(KeyType.Function, "Keypad.Trunc", "trunc"),
                new Item(KeyType.Function, "Keypad.Floor", "floor"),
                new Item(KeyType.Function, "Keypad.Ceil", "ceil"),
                new Item(KeyType.Function, "Keypad.Round", "round"),

                new Item(KeyType.Function, "Keypad.Sin", "sin"),
                new Item(KeyType.Function, "Keypad.Cos", "cos"),
                new Item(KeyType.Function, "Keypad.Tan", "tan"),
                new Item(KeyType.Text, "Keypad.PI", "PI"),

                new Item(KeyType.Function, "Keypad.Asin", "asin"),
                new Item(KeyType.Function, "Keypad.Acos", "acos"),
                new Item(KeyType.Function, "Keypad.Atan", "atan"),
                new Item(KeyType.Function, "Keypad.Atan2", "atan2"),

                new Item(KeyType.Function, "Keypad.Log10", "log10"),
                new Item(KeyType.Function, "Keypad.Log2", "log2"),
                new Item(KeyType.Function, "Keypad.Log", "log"),
                new Item(KeyType.Text, "Keypad.E", "E"),

                new Item(KeyType.Convert, "Keypad.ToDec", "=DEC", 10),
                new Item(KeyType.Convert, "Keypad.ToHex", "=HEX", 16),
                new Item(KeyType.Convert, "Keypad.ToBin", "=BIN", 2),
                new Item(KeyType.Undo, "Keypad.Undo", "↪"),

                new Item(KeyType.Clear, "Keypad.Clear", "CLR"),
                new Item(KeyType.BackSpace, "Keypad.BackSpace", "BS"),
                new Item(KeyType.SelectAll, "Keypad.SelectAll", "ALL"),
                new Item(KeyType.Redo, "Keypad.Redo", "↩"),
            };
        }
    }
}
