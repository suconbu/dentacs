using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Memezo = Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class Calculator
    {
        public string Result { get; private set; }
        public string Error { get; private set; }

        readonly Memezo.Interpreter memezo = new Memezo.Interpreter();

        public Calculator()
        {
            this.memezo.Import(new MathmaticsModule());
            this.memezo.Output += (sender, e) => { this.Result = e.ToString(); };
            this.memezo.Assigning += (sender, e) => { this.Result = e.Value.ToString(); };
            this.memezo.ErrorOccurred += (sender, e) =>
            {
                if (e.Type == Memezo.ErrorType.UnexpectedToken ||
                    e.Type == Memezo.ErrorType.NothingSource ||
                    e.Type == Memezo.ErrorType.MissingToken ||
                    e.Type == Memezo.ErrorType.UnknownError)
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

        void ClearResult()
        {
            this.Result = string.Empty;
            this.Error = null;
        }
    }
}
