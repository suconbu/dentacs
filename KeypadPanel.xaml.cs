using System;
using System.Collections.Generic;
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

            public Item(string label, string usage, string data = null)
            {
                this.Label = label;
                this.Usage = usage;
                this.Data = data ?? label;
                this.Command = Command.None;
            }

            public Item(string label, string usage, Command command)
            {
                this.Label = label;
                this.Usage = usage;
                this.Data = string.Empty;
                this.Command = command;
            }
        }

        public event EventHandler<Item> ItemMouseEnter = delegate { };
        public event EventHandler<Item> ItemMouseLeave = delegate { };
        public event EventHandler<Item> ItemClick = delegate { };

        readonly Item[] keypadItems;
        readonly int keypadRowCount;
        readonly int keypadColumnCount;

        public KeypadPanel()
        {
            InitializeComponent();

            this.keypadItems = new[]
            {
                new Item("/", "除算 : 5 / 2 -> 2.5"),
                new Item("*", "乗算 : 5 * 2 -> 10"),
                new Item("-", "減算 : 5 - 2 -> 3"),
                new Item("+", "加算 : 5 + 2 -> 7"),

                new Item("//", "整数除算 : 5 // 2 -> 2"),
                new Item("**", "べき乗 : 5 ** 2 -> 25"),
                new Item("0x", "16進数数値 : 0xA (0xa) -> 10"),
                new Item("0b", "2進数数値 : 0b1010 -> 10"),

                new Item("%", "剰余 : 5 % 2 -> 1"),
                new Item("( )", "選択中の文字列の両端に括弧を追加", "()"),
                new Item("<<", "算術左シフト : 0b0011 << 1 -> 0b0110"),
                new Item(">>", "算術右シフト : 0b1100 >> 1 -> 0b1110"),

                new Item("&", "論理積 : 0b0011 & 0b0110 -> 0b0010"),
                new Item("|", "論理和 : 0b0011 | 0b0110 -> 0b0111"),
                new Item("^", "排他的論理和 : 0b0110 ^ 0b0011 -> 0b0101"),
                new Item("~", "ビット反転 : ~0b0011 -> 0b1100"),

                new Item("trunc", "0に近づく方向へ小数部切り捨て : trunc(1.5) -> 1, trunc(-1.5) -> -1", "trunc()"),
                new Item("floor", "値の小さい方向へ小数部切り捨て : floor(-1.5) -> -2", "floor()"),
                new Item("ceil", "値の大きい方向へ小数部切り上げ : ceil(1.5) -> 2, ceil(-1.5) -> -1", "ceil()"),
                new Item("round", "小数部を四捨五入 : round(1.4) -> 1, round(1.5) -> 2", "round()"),

                new Item("sin", "正弦 (度) : sin(30) -> 0.5", "sin()"),
                new Item("cos", "余弦 (度) : cos(30) -> 0.866...", "cos()"),
                new Item("tan", "正接 (度) : tan(30) -> 0.577...", "tan()"),
                new Item("PI", "円周率 (3.141...)"),

                new Item("asin", "逆正弦 (度) : asin(0.5) -> 30", "asin()"),
                new Item("acos", "逆余弦 (度) : acos(0.5) -> 60", "acos()"),
                new Item("atan", "逆正接 (度) : atan(0.5) -> 26.565...", "atan()"),
                new Item("atan2", "逆正接 (度) : atan2(5, 10) -> 26.565...", "atan2()"),

                new Item("log10", "10を底とする対数 : log10(1000) -> 3", "log10()"),
                new Item("log2", "2を底とする対数 : log2(16) -> 4", "log2()"),
                new Item("log", "第2引数の数値を底とする対数 : log(9, 3) -> 2", "log()"),
                new Item("E", "自然対数の底 (2.718...)"),

                new Item("CLR", "式を消去", Command.Clear),
                new Item("↪", "元に戻す (Ctrl+Z)", Command.Undo),
                new Item("↩", "やり直し (Ctrl+Y)", Command.Redo),
                new Item("BS", "1文字削除 (BackSpace)", Command.BackSpace),
            };
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
    }
}
