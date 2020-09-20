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

        private enum ContentType { Number, String, DateTime, TimeSpan }
        private readonly Memezo.Interpreter memezo = new Memezo.Interpreter();
        private CultureInfo culture;

        public Calculator(CultureInfo culture = null)
        {
            this.culture = culture ?? CultureInfo.InvariantCulture;
            this.memezo.Import(new MathmaticsModule());
            this.memezo.Import(new DateTimeModule());
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
                DateTimeUtility.TryParseDateTime(value.String, out var dateTime) ? $"'{DateTimeUtility.DateTimeToString(dateTime)}'" :
                DateTimeUtility.TryParseTimeSpan(value.String, out var timeSpan) ? $"'{DateTimeUtility.TimeSpanToString(timeSpan)}'" :
                value.ToString();
        }

        private void ClearResult()
        {
            this.Result = string.Empty;
            this.Error = null;
        }

        private Memezo.Value UnaryOperation(Memezo.Value first, Memezo.TokenType tokenType) => null;

        private Memezo.Value BinaryOperation(Memezo.Value first, Memezo.Value second, Memezo.TokenType tokenType)
        {
            DateTime firstDateTime = DateTime.MinValue;
            DateTime secondDateTime = DateTime.MinValue;
            TimeSpan firstTimeSpan = TimeSpan.MinValue;
            TimeSpan secondTimeSpan = TimeSpan.MinValue;

            var firstType =
                (first.DataType == Memezo.DataType.Number) ? ContentType.Number :
                DateTimeUtility.TryParseDateTime(first.String, out firstDateTime) ? ContentType.DateTime :
                DateTimeUtility.TryParseTimeSpan(first.String, out firstTimeSpan) ? ContentType.TimeSpan :
                ContentType.String;
            var secondType =
                (second.DataType == Memezo.DataType.Number) ? ContentType.Number :
                DateTimeUtility.TryParseDateTime(second.String, out secondDateTime) ? ContentType.DateTime :
                DateTimeUtility.TryParseTimeSpan(second.String, out secondTimeSpan) ? ContentType.TimeSpan :
                ContentType.String;

            if (firstType == ContentType.DateTime && secondType == ContentType.DateTime)
            {
                if (tokenType == Memezo.TokenType.Minus)
                {
                    var span = firstDateTime - secondDateTime;
                    return new Memezo.Value(DateTimeUtility.TimeSpanToString(span));
                }
            }
            else if (firstType == ContentType.DateTime && secondType == ContentType.TimeSpan)
            {
                if (tokenType == Memezo.TokenType.Plus || tokenType == Memezo.TokenType.Minus)
                {
                    var date = (tokenType == Memezo.TokenType.Plus) ?
                        (firstDateTime + secondTimeSpan) :
                        (firstDateTime - secondTimeSpan);
                    return new Memezo.Value(DateTimeUtility.DateTimeToString(date));
                }
            }
            else if (firstType == ContentType.TimeSpan && secondType == ContentType.TimeSpan)
            {
                if (tokenType == Memezo.TokenType.Plus || tokenType == Memezo.TokenType.Minus)
                {
                    var span = (tokenType == Memezo.TokenType.Plus) ?
                        (firstTimeSpan + secondTimeSpan) :
                        (firstTimeSpan - secondTimeSpan);
                    return new Memezo.Value(DateTimeUtility.TimeSpanToString(span));
                }
            }
            else if (firstType == ContentType.TimeSpan && secondType == ContentType.Number)
            {
                if (tokenType == Memezo.TokenType.Multiply || tokenType == Memezo.TokenType.Division)
                {
                    var ticks = (tokenType == Memezo.TokenType.Multiply) ?
                        (firstTimeSpan.Ticks * second.Number) :
                        (firstTimeSpan.Ticks / second.Number);
                    if (ticks < long.MinValue || long.MaxValue < ticks) throw new OverflowException();
                    return new Memezo.Value(DateTimeUtility.TimeSpanToString(new TimeSpan((long)ticks)));
                }
            }
            else
            {
                ;
            }
            return null;
        }
    }
}
