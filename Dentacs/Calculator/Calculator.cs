using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Memezo = Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class Calculator
    {
        public string Result { get; private set; }
        public string Error { get; private set; }

        private static class ContentType
        {
            public static readonly string DateTime = nameof(DateTime);
            public static readonly string TimeSpan = nameof(TimeSpan);
        }

        private readonly Memezo.Interpreter memezo = new Memezo.Interpreter();
        private CultureInfo culture;

        public Calculator(CultureInfo culture = null)
        {
            this.culture = culture ?? CultureInfo.InvariantCulture;
            this.memezo.Import(new MathmaticsModule());
            this.memezo.Output += (sender, e) => { this.SetResult(e); };
            this.memezo.Assigning += (sender, e) => { this.SetResult(e.Value); };
            this.memezo.UnaryOperationOverride += (first, tokenType) => this.UnaryOperation(first, tokenType);
            this.memezo.BinaryOperationOverride += (first, second, tokenType) => this.BinaryOperation(first, second, tokenType);
            this.memezo.ErrorOccurred += (sender, e) =>
            {
                if (e.Type == Memezo.ErrorType.UnexpectedToken ||
                    e.Type == Memezo.ErrorType.NothingSource ||
                    e.Type == Memezo.ErrorType.MissingToken)
                {
                    this.Error = this.Error ?? string.Empty;
                }
                else
                {
                    this.Error = e.Message;
                }
            };
        }

        public void Reset()
        {
            this.memezo.Vars.Clear();
            this.ClearResult();
        }

        public bool Calculate(string expression)
        {
            this.ClearResult();
            this.memezo.Source = expression;
            this.memezo.Run();
            return this.Error == null;
        }

        private void SetResult(Memezo.Value value)
        {
            this.Result =
                (value.ContentType == ContentType.DateTime) ? DateTimeUtility.DateTimeFromSeconds(value.Number).ToString() :
                (value.ContentType == ContentType.TimeSpan) ? this.TimeSpanToString(DateTimeUtility.TimeSpanFromSeconds(value.Number)) :
                value.ToString();
        }

        private string TimeSpanToString(TimeSpan t)
        {
            var sign = (t.Ticks < 0) ? "-" : "";
            var d = t.Duration();
            var seconds = d.TotalSeconds - Math.Truncate(d.TotalSeconds);
            if (this.culture.TwoLetterISOLanguageName == "ja")
            {
                return $"{sign}{d.Days}日{d.Hours}時間{d.Minutes}分{seconds}秒";
            }
            else
            {
                return $"{sign}{d.Days}day{d.Hours}hour{d.Minutes}min{seconds}sec";
            }
        }

        private void ClearResult()
        {
            this.Result = string.Empty;
            this.Error = null;
        }

        private Memezo.Value UnaryOperation(Memezo.Value first, Memezo.TokenType tokenType) => null;

        private Memezo.Value BinaryOperation(Memezo.Value first, Memezo.Value second, Memezo.TokenType tokenType)
        {
            string firstType = null;
            string secondType = null;
            DateTime firstDateTime = DateTime.MinValue;
            DateTime secondDateTime = DateTime.MinValue;
            TimeSpan firstTimeSpan = TimeSpan.MinValue;
            TimeSpan secondTimeSpan = TimeSpan.MinValue;

            if (first.ContentType != null)
            {
                firstType = first.ContentType;
                if (first.ContentType == ContentType.DateTime)
                {
                    firstDateTime = DateTimeUtility.DateTimeFromSeconds(first.Number);
                }
                else if (first.ContentType == ContentType.TimeSpan)
                {
                    firstTimeSpan = DateTimeUtility.TimeSpanFromSeconds(first.Number);
                }
            }
            else if (first.DataType == Memezo.DataType.String)
            {
                firstType =
                    DateTimeUtility.TryParseDateTime(first.String, out firstDateTime) ? ContentType.DateTime :
                    DateTimeUtility.TryParseTimeSpan(first.String, out firstTimeSpan) ? ContentType.TimeSpan :
                    null;
            }

            if (second.ContentType != null)
            {
                secondType = second.ContentType;
                if (second.ContentType == ContentType.DateTime)
                {
                    secondDateTime = DateTimeUtility.DateTimeFromSeconds(second.Number);
                }
                else if (second.ContentType == ContentType.TimeSpan)
                {
                    secondTimeSpan = DateTimeUtility.TimeSpanFromSeconds(second.Number);
                }
            }
            else if (second.DataType == Memezo.DataType.String)
            {
                secondType =
                    DateTimeUtility.TryParseDateTime(second.String, out secondDateTime) ? ContentType.DateTime :
                    DateTimeUtility.TryParseTimeSpan(second.String, out secondTimeSpan) ? ContentType.TimeSpan :
                    null;
            }

            if (firstType != null && secondType != null)
            {
                if (firstType == ContentType.DateTime && secondType == ContentType.DateTime)
                {
                    if (tokenType == Memezo.TokenType.Minus)
                    {
                        var span = firstDateTime - secondDateTime;
                        var seconds = DateTimeUtility.TimeSpanToSeconds(span);
                        return new Memezo.Value(seconds, ContentType.TimeSpan);
                    }
                }
                else if (firstType == ContentType.DateTime && secondType == ContentType.TimeSpan)
                {
                    if (tokenType == Memezo.TokenType.Plus || tokenType == Memezo.TokenType.Minus)
                    {
                        var date = (tokenType == Memezo.TokenType.Plus) ?
                            (firstDateTime + secondTimeSpan) :
                            (firstDateTime - secondTimeSpan);
                        var seconds = DateTimeUtility.DateTimeToSeconds(date);
                        return new Memezo.Value(seconds, ContentType.DateTime);
                    }
                }
                else if (firstType == ContentType.TimeSpan && secondType == ContentType.TimeSpan)
                {
                    if (tokenType == Memezo.TokenType.Plus || tokenType == Memezo.TokenType.Minus)
                    {
                        var span = (tokenType == Memezo.TokenType.Plus) ?
                            (firstTimeSpan + secondTimeSpan) :
                            (firstTimeSpan - secondTimeSpan);
                        var seconds = DateTimeUtility.TimeSpanToSeconds(span);
                        return new Memezo.Value(seconds, ContentType.TimeSpan);
                    }
                }
                else
                {
                    ;
                }
            }

            return null;
        }
    }
}
