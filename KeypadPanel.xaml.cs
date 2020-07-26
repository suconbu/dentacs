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
        public struct Item
        {
            public enum Type { Key, Text }

            public string Label { get; }
            public string Usage { get; }
            public string Data { get; }
            public Type DataType { get; }

            public Item(string label, string usage = null, string data = null)
            {
                this.Label = label;
                this.Usage = usage;
                this.Data = data ?? label;
                this.DataType = (this.Data.StartsWith("{") && this.Data.EndsWith("}")) ? Type.Key : Type.Text;
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
                new Item("7"),
                new Item("4"),
                new Item("1"),
                new Item("BS", "手前の文字または選択中の文字列を削除", "{BACKSPACE}"),

                new Item("8"),
                new Item("5"),
                new Item("2"),
                new Item("0"),

                new Item("9"),
                new Item("6"),
                new Item("3"),
                new Item(".", "小数点"),

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

                new Item("TRUNC", "0に近づく方向へ小数部切り捨て : TRUNC(1.5) -> 1, TRUNC(-1.5) -> -1", "TRUNC()"),
                new Item("FLOOR", "値の小さい方向へ小数部切り捨て : FLOOR(-1.5) -> -2", "FLOOR()"),
                new Item("CEIL", "値の大きい方向へ小数部切り上げ : CEIL(1.5) -> 2, CEIL(-1.5) -> -1", "CEIL()"),
                new Item("ROUND", "小数部を四捨五入 : ROUND(1.4) -> 1, CEIL(1.5) -> 2", "ROUND()"),

                new Item("SIN", "正弦(度) : SIN(30) -> 0.5", "SIN()"),
                new Item("COS", "余弦(度) : COS(30) -> 0.866...", "COS()"),
                new Item("TAN", "正接(度) : TAN(30) -> 0.577...", "TAN()"),
                new Item("PI", "円周率"),

                new Item("ASIN", "逆正弦(度) : ASIN(0.5) -> 30", "ASIN()"),
                new Item("ACOS", "逆余弦(度) : ACOS(0.5) -> 60", "ACOS()"),
                new Item("ATAN", "逆正接(度) : ATAN(0.5) -> 26.565...", "ATAN()"),
                new Item("ATAN2", "逆正接(度) : ATAN2(5, 10) -> 26.565...", "ATAN2()"),

                new Item("LOG10", "10を底とする対数 : LOG10(1000) -> 3", "LOG10()"),
                new Item("LOG2", "2を底とする対数 : LOG2(16) -> 4", "LOG2()"),
                new Item("LOG", "ネイピア数または第2引数の数値を底とする対数 : LOG(9, 3) -> 2", "LOG()"),
                new Item("EXP", "ネイピア数のべき乗 : EXP(3) -> 20.085...", "EXP()"),
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
                var button = new Button()
                {
                    Content = item.Label,
                    Style = this.FindResource("KeypadButtonStyle") as Style
                };
                button.MouseEnter += (s, e) => this.ItemMouseEnter(this, item);
                button.MouseLeave += (s, e) => this.ItemMouseLeave(this, item);
                button.Click += (s, e) => this.ItemClick(this, item);

                Grid.SetRow(button, i % this.keypadRowCount);
                Grid.SetColumn(button, i / this.keypadRowCount);
                this.Container.Children.Add(button);
            }
        }
    }
}
