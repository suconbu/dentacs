using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Memezo = Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{

    public class Calculator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string Expression { get { return null; } set { this.SetExpression(value); } }
        public string Result { get { return this.result; } set { this.SetProperty(ref this.result, value); } }
        public bool IsResultEnabled { get { return this.isResultEnabled; } set { this.SetProperty(ref this.isResultEnabled, value); } }

        static Calculator instance = new Calculator();
        Memezo.Interpreter memezo = new Memezo.Interpreter();
        string result;
        bool isResultEnabled;

        public static Calculator GetInstance()
        {
            return Calculator.instance;
        }

        public Calculator()
        {
            this.memezo.Install(new MathmaticsLibrary());
            this.memezo.Output += (sender, e) =>
            {
                this.Result = e;
                this.IsResultEnabled = true;
            };
            this.memezo.Assigning += (sender, e) =>
            {
                this.Result = e.Value.ToString();
                this.IsResultEnabled = true;
            };
            this.memezo.ErrorOccurred += (sender, e) =>
            {
                if (e.Type == Memezo.ErrorType.UndeclaredIdentifier ||
                    e.Type == Memezo.ErrorType.UnknownOperator ||
                    e.Type == Memezo.ErrorType.NumberOverflow ||
                    e.Type == Memezo.ErrorType.UnknownToken)
                {
                    this.Result = e.Message;
                }
            };
        }

        void ClearResult()
        {
            this.Result = null;
            this.IsResultEnabled = true;
        }

        public void SetExpression(string expression)
        {
            if (!string.IsNullOrWhiteSpace(expression))
            {
                this.IsResultEnabled = false;
                this.memezo.Source = expression;
                this.memezo.Run();
            }
            else
            {
                this.ClearResult();
            }
        }

        void SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (object.Equals(field, value)) return;
            field = value;
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
