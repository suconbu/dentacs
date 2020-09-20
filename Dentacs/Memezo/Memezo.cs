using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Suconbu.Scripting.Memezo
{
    public enum DataType { Number, String }
    public enum ErrorType
    {
        NothingSource, UnexpectedToken, UnknownToken, MissingToken, UndeclaredIdentifier,
        NotSupportedOperation, CannotAssignToConstant, CannotAssignToFunction, UnknownOperator,
        InvalidDataType, InvalidOperation, InvalidArgument,
        InvalidNumberFormat, InvalidStringLiteral, NumberOverflow, StringOverflow, UnknownError
    }
    public delegate Value Function(List<Value> args);

    public class Interpreter
    {
        public event EventHandler<Value> Output = delegate { };
        public event EventHandler<ErrorInfo> ErrorOccurred = delegate { };
        public event EventHandler<string> FunctionInvoking = delegate { };
        public event EventHandler<AssigningEventArgs> Assigning = delegate { };

        public Func<Value, TokenType, Value> UnaryOperationOverride = delegate { return null; };
        public Func<Value, Value, TokenType, Value> BinaryOperationOverride = delegate { return null; };

        public static readonly string[] Keywords = Lexer.Keywords;
        public static readonly string LineCommentMarker = Lexer.LineCommentMarker;
        public static readonly char[] StringQuoteMarkers = Lexer.StringQuoteMarkers;
        public Dictionary<string, Value> Vars { get; private set; } = new Dictionary<string, Value>();
        public ErrorInfo LastError { get; private set; }
        public RunStat Stat { get; private set; } = new RunStat();
        public IEnumerable<IModule> Modules { get { return this.modules; } }
        public string Source { get => this.source; set => this.SetSource(value); }

        string source;
        Lexer lexer;
        SourceLocation statementLocation;
        int nestingLevelOfDeferredSource;
        readonly StringBuilder interactiveSource = new StringBuilder();
        readonly Stack<Clause> clauses = new Stack<Clause>();
        readonly Dictionary<TokenType, int> binaryOperatorPrecs = new Dictionary<TokenType, int>()
        {
            { TokenType.Exponent, 12 },
            { TokenType.Multiply, 10 }, {TokenType.Division, 10 }, {TokenType.FloorDivision, 10 }, {TokenType.Remainder, 10 },
            { TokenType.Plus, 9 }, { TokenType.Minus, 9 },
            { TokenType.BitwiseLeftShift, 8 }, { TokenType.BitwiseRightShift, 8 },
            { TokenType.BitwiseAnd, 7 },
            { TokenType.BitwiseXor, 6 },
            { TokenType.BitwiseOr, 5 },
            { TokenType.Equal, 4 }, { TokenType.NotEqual, 4 }, { TokenType.Less, 4 }, { TokenType.Greater, 4 }, { TokenType.LessEqual, 4 },  { TokenType.GreaterEqual, 4 },
            { TokenType.And, 2 },
            { TokenType.Or, 1 }
        };
        readonly Dictionary<TokenType, int> unaryOperatorPrecs = new Dictionary<TokenType, int>()
        {
            { TokenType.Plus, 11 }, { TokenType.Minus, 11 }, { TokenType.BitwiseNot, 11 },
            { TokenType.Not, 3 },
        };
        readonly int maxOperatedStringLength = 10000;
        List<IModule> modules = new List<IModule>();

        public Interpreter() { }

        public void Import(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                this.modules.Add(module);
            }
        }

        public bool Run()
        {
            this.lexer = this.lexer ?? this.PrepareLexer(this.source);
            return this.RunInternal(false, out var _);
        }

        public bool Step(out int nextIndex)
        {
            this.lexer = this.lexer ?? this.PrepareLexer(this.source);
            return this.RunInternal(true, out nextIndex);
        }

        public bool ForwardToNextStatement(out int nextIndex)
        {
            nextIndex = -1;
            this.lexer = this.lexer ?? this.PrepareLexer(this.source);
            if (this.lexer == null) return false;
            while (this.lexer.Token.Type == TokenType.NewLine) this.lexer.ReadToken();
            nextIndex = this.lexer.Token.Location.CharIndex;
            return true;
        }

        public bool RunInteractive(string source, out bool deferred)
        {
            deferred = true;
            this.interactiveSource.AppendLine(source);
            var tokens = Lexer.SplitTokens(source);
            this.nestingLevelOfDeferredSource += tokens.Count(t => t.IsCompoundStatement());
            this.nestingLevelOfDeferredSource -= tokens.Count(t => t.Type == TokenType.End);
            if (this.nestingLevelOfDeferredSource > 0) return true;

            this.lexer = this.PrepareLexer(this.interactiveSource.ToString());
            var result = this.RunInternal(false, out var nextIndex);

            this.interactiveSource.Clear();
            this.nestingLevelOfDeferredSource = this.clauses.Count;
            deferred = (this.nestingLevelOfDeferredSource > 0);

            return result;
        }

        bool RunInternal(bool stepByStep, out int nextIndex)
        {
            nextIndex = -1;
            try
            {
                if (this.lexer == null) throw new ErrorException(ErrorType.NothingSource);
                if (stepByStep)
                {
                    nextIndex = this.Statement() ? this.lexer.Token.Location.CharIndex : nextIndex;
                }
                else
                {
                    while (this.Statement()) ;
                }
                return true;
            }
            catch (OverflowException ex)
            {
                this.HandleException(new ErrorException(ErrorType.NumberOverflow, ex));
                return false;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
                return false;
            }
        }

        bool Statement()
        {
            while (this.lexer.Token.Type == TokenType.NewLine) this.lexer.ReadToken();

            this.statementLocation = this.lexer.Token.Location;
            this.Stat.StatementCount++;

            var type = this.lexer.Token.Type;
            var nextType = this.lexer.NextToken.Type;
            var continueToRun = true;

            if (type == TokenType.Unkown) throw new ErrorException(ErrorType.UnknownToken, $"{this.lexer.Token}");
            else if (type == TokenType.If) this.OnIf();
            else if (type == TokenType.Elif) this.OnAfterIf();
            else if (type == TokenType.Else) this.OnAfterIf();
            else if (type == TokenType.For) this.OnFor();
            else if (type == TokenType.Repeat) this.OnRepeat();
            else if (type == TokenType.End) this.OnEnd();
            else if (type == TokenType.Continue) this.OnContinue();
            else if (type == TokenType.Break) this.OnBreak();
            else if (type == TokenType.Exit) { this.OnExit(); continueToRun = false; }
            else if (type == TokenType.Eof) { this.OnEof(); continueToRun = false; }
            else if (type == TokenType.Identifer && nextType == TokenType.Assign) this.OnAssign();
            else this.Output(this, this.Expr());

            return continueToRun;
        }

        void OnIf()
        {
        Start:
            var statementToken = this.lexer.Token;
            this.lexer.ReadToken();
            if (statementToken.Type == TokenType.If)
                this.clauses.Push(new Clause(statementToken, null));
            var result = (statementToken.Type == TokenType.If || statementToken.Type == TokenType.Elif) ? this.Expr().IsTrue() : true;

            if (this.lexer.Token.Type == TokenType.Colon) this.lexer.ReadToken();
            else if (statementToken.Type != TokenType.Else && this.lexer.Token.Type == TokenType.Then) this.lexer.ReadToken();

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: {this.lexer.Token} {result}");

            if (!result)
            {
                int count = 0;
                do
                {
                    if (this.lexer.Token.IsCompoundStatement())
                    {
                        count++;
                    }
                    else if (this.lexer.Token.Type == TokenType.Elif || this.lexer.Token.Type == TokenType.Else)
                    {
                        if (count == 0) goto Start;
                    }
                    else if (this.lexer.Token.Type == TokenType.End)
                    {
                        if (count-- == 0) break;
                    }
                } while (this.lexer.ReadToken().Type != TokenType.Eof);
            }
        }

        void OnAfterIf()
        {
            if (this.clauses.Count <= 0 || this.clauses.Peek().Token.Type != TokenType.If)
                throw new ErrorException(ErrorType.UnexpectedToken, $"{this.lexer.Token}");

            int count = 0;
            while (this.lexer.ReadToken().Type != TokenType.Eof)
            {
                if (this.lexer.Token.IsCompoundStatement())
                {
                    count++;
                }
                else if (this.lexer.Token.Type == TokenType.End)
                {
                    if (count-- == 0) break;
                }
            }
        }

        void OnFor()
        {
            var statementToken = this.lexer.Token;
            this.VerifyToken(this.lexer.ReadToken(), TokenType.Identifer);
            var name = this.lexer.Token.String;

            var token = this.lexer.ReadToken();
            if (token.Type != TokenType.Assign && token.Type != TokenType.In)
                throw new ErrorException(ErrorType.UnexpectedToken, $"{token}");

            this.lexer.ReadToken();
            var fromValue = this.Expr();

            this.VerifyToken(this.lexer.Token, TokenType.To);

            this.lexer.ReadToken();
            var toValue = this.Expr();

            if (this.clauses.Count == 0 || this.clauses.Peek().VarName != name)
            {
                this.AssignVar(name, fromValue);
                this.clauses.Push(new Clause(statementToken, name));
            }

            if (this.lexer.Token.Type == TokenType.Colon || this.lexer.Token.Type == TokenType.Do) this.lexer.ReadToken();

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: For {this.Vars[name]} to {toValue}");

            if (this.BinaryOperation(this.Vars[name], toValue, TokenType.Greater).IsTrue()) this.FinishLoop();
        }

        void OnRepeat()
        {
            var statementToken = this.lexer.Token;
            this.lexer.ReadToken();
            var count = this.Expr();
            var name = $"${this.statementLocation.CharIndex}";
            if (this.clauses.Count == 0 || this.clauses.Peek().VarName != name)
            {
                this.AssignVar(name, Value.Zero);
                this.clauses.Push(new Clause(statementToken, name));
            }

            if (this.lexer.Token.Type == TokenType.Colon || this.lexer.Token.Type == TokenType.Do) this.lexer.ReadToken();

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: Repeat {count}");

            if (this.BinaryOperation(this.Vars[name], count, TokenType.GreaterEqual).IsTrue()) this.FinishLoop();
        }

        void OnEnd()
        {
            this.lexer.ReadToken();

            if (this.clauses.Count <= 0)
                throw new ErrorException(ErrorType.UnexpectedToken, $"{TokenType.End}");

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: End");

            var clause = this.clauses.Peek();
            if (clause.Token.Type == TokenType.If)
                this.EndIf(clause);
            else if (clause.Token.IsLoop())
                this.EndFor(clause);
            else
                throw new ErrorException(ErrorType.UnexpectedToken, $"{clause.Token}");
        }

        void OnContinue()
        {
            while (this.clauses.Count > 0)
            {
                var clause = this.clauses.Peek();
                if(clause.Token.IsLoop())
                {
                    this.EndFor(clause);
                    return;
                }
                this.clauses.Pop();
            }
            throw new ErrorException(ErrorType.UnexpectedToken, $"{TokenType.Continue}");
        }

        void OnBreak()
        {
            this.lexer.ReadToken();

            int counter = 0;
            while (this.clauses.Count > 0)
            {
                var clause = this.clauses.Peek();
                if (clause.Token.IsLoop())
                {
                    this.FinishLoop(counter);
                    return;
                }
                this.clauses.Pop();
                counter++;
            }
            throw new ErrorException(ErrorType.UnexpectedToken, $"{TokenType.Break}");
        }

        void FinishLoop(int initialCounter = 0)
        {
            int counter = initialCounter;
            while (counter >= 0)
            {
                if (this.lexer.Token.IsCompoundStatement()) counter++;
                else if (this.lexer.Token.Type == TokenType.End) counter--;
                this.lexer.ReadToken();
            }
            var clause = this.clauses.Pop();
            if (clause.VarName.StartsWith("$")) this.Vars.Remove(clause.VarName);
        }

        void EndIf(Clause clause)
        {
            this.clauses.Pop();
        }

        void EndFor(Clause clause)
        {
            var value = this.BinaryOperation(this.Vars[clause.VarName], new Value(1), TokenType.Plus);
            this.AssignVar(clause.VarName, value);
            this.lexer.Move(clause.Token.Location);
            this.lexer.ReadToken();
        }

        void OnExit() { }

        void OnEof()
        {
            if (this.clauses.Count > 0) throw new ErrorException(ErrorType.MissingToken, $"{TokenType.End}");
        }

        void OnAssign()
        {
            var name = this.lexer.Token.String;
            this.VerifyToken(this.lexer.ReadToken(), TokenType.Assign);
            this.lexer.ReadToken();
            var value = this.Expr();
            this.Assigning(this, new AssigningEventArgs(name, value));
            this.AssignVar(name, value);
            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: Assign {name}={this.Vars[name].ToString()}");
        }

        Value Expr(int lowestPrec = int.MinValue + 1)
        {
            var lhs = this.Primary();
            while (true)
            {
                if (!this.lexer.Token.IsOperator()) break;
                if (!this.binaryOperatorPrecs.TryGetValue(this.lexer.Token.Type, out var prec)) prec = int.MinValue;
                if (prec <= lowestPrec) break;

                var type = this.lexer.Token.Type;
                this.lexer.ReadToken();
                var rhs = this.Expr(prec);
                lhs = this.BinaryOperation(lhs, rhs, type);
                RunStat.Increment(this.Stat.OperatorCounts, type.ToString());
            }
            if (lhs.DataType == DataType.Number)
            {
                if (lhs.Number < long.MinValue || long.MaxValue < lhs.Number)
                {
                    throw new ErrorException(ErrorType.NumberOverflow);
                }
            }

            return lhs;
        }

        Value Primary()
        {
            var primary = Value.Zero;
            var token = this.lexer.Token;

            if (this.unaryOperatorPrecs.TryGetValue(token.Type, out var prec))
            {
                this.lexer.ReadToken();
                primary = this.UnaryOperation(this.Expr(prec), token.Type);
            }
            else if (token.Type == TokenType.String)
            {
                primary = new Value(token.String);
                this.lexer.ReadToken();
            }
            else if (token.Type == TokenType.Number)
            {
                primary = new Value(token.Number);
                this.lexer.ReadToken();
            }
            else if (token.Type == TokenType.LeftParen)
            {
                this.lexer.ReadToken();
                primary = this.Expr();
                this.VerifyToken(this.lexer.Token, TokenType.RightParen);
                this.lexer.ReadToken();
            }
            else if (token.Type == TokenType.Identifer)
            {
                var identifier = token.String;
                if (this.Vars.TryGetValue(identifier, out var value))
                {
                    primary = value;
                    this.lexer.ReadToken();
                }
                else if (this.TryGetConstant(identifier, out var constant))
                {
                    primary = constant;
                    this.lexer.ReadToken();
                }
                else if (this.TryGetFunction(identifier, out var function))
                {
                    this.VerifyToken(this.lexer.ReadToken(), TokenType.LeftParen);
                    var args = this.ReadArguments();
                    this.FunctionInvoking(this, identifier);
                    primary = function(args);
                    RunStat.Increment(this.Stat.FunctionInvokedCounts, identifier);
                }
                else
                {
                    throw new ErrorException(ErrorType.UndeclaredIdentifier, $"'{identifier}'");
                }
            }
            else
            {
                throw new ErrorException(ErrorType.UnexpectedToken, $"{token}");
            }

            return primary;
        }

        void VerifyToken(Token token, TokenType expectedType)
        {
            if (token.Type != expectedType) throw new ErrorException(ErrorType.MissingToken, $"{expectedType}");
        }

        List<Value> ReadArguments()
        {
            var args = new List<Value>();
            while (true)
            {
                if (this.lexer.ReadToken().Type != TokenType.RightParen)
                {
                    while (this.lexer.Token.Type == TokenType.NewLine) this.lexer.ReadToken();
                    args.Add(this.Expr());
                    while (this.lexer.Token.Type == TokenType.NewLine) this.lexer.ReadToken();
                    if (this.lexer.Token.Type == TokenType.Comma) continue;
                }
                this.lexer.ReadToken();
                break;
            }
            return args;
        }

        void AssignVar(string name, Value value)
        {
            if (this.TryGetFunction(name, out _)) throw new ErrorException(ErrorType.CannotAssignToFunction, $"'{name}' is a function");
            if (this.TryGetConstant(name, out _)) throw new ErrorException(ErrorType.CannotAssignToConstant, $"'{name}' is a constant");
            this.Vars[name] = value;
        }

        Value UnaryOperation(Value a, TokenType tokenType)
        {
            if (this.UnaryOperationOverride(a, tokenType) is Value result) return result;

            if (tokenType == TokenType.Not)
            {
                return new Value(!a.IsTrue());
            }
            else if (tokenType == TokenType.Plus || tokenType == TokenType.Minus)
            {
                if (a.DataType != DataType.Number)
                {
                    throw new ErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.DataType}");
                }
                return new Value(tokenType == TokenType.Minus ? -a.Number : a.Number);
            }
            else if (tokenType == TokenType.BitwiseNot)
            {
                if (!a.TryToInteger(out var integer))
                {
                    throw new ErrorException(ErrorType.NotSupportedOperation, $"Non-integer number for {a.DataType}");
                }
                return new Value(~integer);
            }
            else
            {
                throw new ErrorException(ErrorType.UnknownOperator, $"{tokenType}");
            }
        }

        Value BinaryOperation(Value a, Value b, TokenType tokenType)
        {
            if (this.BinaryOperationOverride(a, b, tokenType) is Value result) return result;

            if (tokenType == TokenType.Multiply)
            {
                if (a.DataType == DataType.Number && b.DataType == DataType.String ||
                    a.DataType == DataType.String && b.DataType == DataType.Number)
                {
                    var n = (a.DataType == DataType.Number) ? a.Number : b.Number;
                    var s = (a.DataType == DataType.String) ? a.String : b.String;
                    n = Math.Max(n, 0m);
                    if (this.maxOperatedStringLength < s.Length * n) throw new ErrorException(ErrorType.StringOverflow);
                    return new Value((new StringBuilder().Insert(0, s, (int)n)).ToString());
                }
            }

            if (a.DataType != b.DataType)
            {
                if (a.DataType == DataType.Number && b.DataType == DataType.String)
                    a = new Value(a.ToString());
                else if (a.DataType == DataType.String && b.DataType == DataType.Number)
                    b = new Value(b.ToString());
                else
                    throw new ErrorException(ErrorType.InvalidDataType, $"{a.DataType} x {b.DataType}");
            }

            if (tokenType == TokenType.Plus)
            {
                if (a.DataType == DataType.String && b.DataType == DataType.String)
                {
                    if (this.maxOperatedStringLength < a.String.Length + b.String.Length) throw new ErrorException(ErrorType.StringOverflow);
                    return new Value(a.String + b.String);
                }
            }
            else if (tokenType == TokenType.Equal)
            {
                return
                    (a.DataType == DataType.Number) ? new Value(a.Number == b.Number ? 1 : 0) :
                    (a.DataType == DataType.String) ? new Value(a.String == b.String ? 1 : 0) :
                    throw new ErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.DataType}");
            }
            else if (tokenType == TokenType.NotEqual)
            {
                return
                    (a.DataType == DataType.Number) ? new Value(a.Number != b.Number ? 1 : 0) :
                    (a.DataType == DataType.String) ? new Value(a.String != b.String ? 1 : 0) :
                    throw new ErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.DataType}");
            }

            if (a.DataType != DataType.Number) throw new ErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.DataType}");

            if (Token.IsBitwiseOperator(tokenType))
            {
                if (!a.TryToInteger(out var ia) || !b.TryToInteger(out var ib))
                {
                    throw new ErrorException(ErrorType.NotSupportedOperation, $"Non-integer number for {tokenType}");
                }

                if (tokenType == TokenType.BitwiseLeftShift || tokenType == TokenType.BitwiseRightShift)
                {
                    if (ib < 0 || int.MaxValue <= ib)
                    {
                        throw new ErrorException(ErrorType.NotSupportedOperation, $"Invalid shift count");
                    }
                    if (sizeof(long) * 8 <= ib)
                    {
                        return new Value((0 <= ia) ? 0 : -1);
                    }
                    return (tokenType == TokenType.BitwiseLeftShift) ? new Value(ia << (int)ib) : new Value(ia >> (int)ib);
                }
                else
                {
                    return
                        (tokenType == TokenType.BitwiseAnd) ? new Value(ia & ib) :
                        (tokenType == TokenType.BitwiseOr) ? new Value(ia | ib) :
                        (tokenType == TokenType.BitwiseXor) ? new Value(ia ^ ib) :
                        throw new ErrorException(ErrorType.UnknownOperator, $"{tokenType}");
                }
            }

            try
            {
                return
                    (tokenType == TokenType.Plus) ? new Value(a.Number + b.Number) :
                    (tokenType == TokenType.Minus) ? new Value(a.Number - b.Number) :
                    (tokenType == TokenType.Multiply) ? new Value(a.Number * b.Number) :
                    (tokenType == TokenType.Division) ? new Value(a.Number / b.Number) :
                    (tokenType == TokenType.FloorDivision) ? new Value(Math.Floor(a.Number / b.Number)) :
                    (tokenType == TokenType.Remainder) ? new Value((a.Number - (b.Number * Math.Floor(a.Number / b.Number)))) :
                    (tokenType == TokenType.Exponent) ? new Value((decimal)Math.Pow((double)a.Number, (double)b.Number)) :
                    (tokenType == TokenType.Less) ? new Value(a.Number < b.Number ? 1 : 0) :
                    (tokenType == TokenType.Greater) ? new Value(a.Number > b.Number ? 1 : 0) :
                    (tokenType == TokenType.LessEqual) ? new Value(a.Number <= b.Number ? 1 : 0) :
                    (tokenType == TokenType.GreaterEqual) ? new Value(a.Number >= b.Number ? 1 : 0) :
                    (tokenType == TokenType.And) ? new Value(a.Number != 0m && b.Number != 0m ? 1 : 0) :
                    (tokenType == TokenType.Or) ? new Value(a.Number != 0m || b.Number != 0m ? 1 : 0) :
                    throw new ErrorException(ErrorType.UnknownOperator, $"{tokenType}");
            }
            catch (OverflowException)
            {
                throw new ErrorException(ErrorType.NumberOverflow);
            }
        }

        bool TryGetFunction(string name, out Function function)
        {
            foreach (var module in this.modules)
            {
                if (module.Functions != null && module.Functions.TryGetValue(name, out function)) return true;
            }
            function = null;
            return false;
        }

        bool TryGetConstant(string name, out Value value)
        {
            foreach (var module in this.modules)
            {
                if (module.Constants != null && module.Constants.TryGetValue(name, out value)) return true;
            }
            value = null;
            return false;
        }

        Lexer PrepareLexer(string input)
        {
            if (input == null) return null;
            try
            {
                var lexer = new Lexer(input);
                lexer.TokenRead += (s, e) => RunStat.Increment(this.Stat.TokenCounts, e.Type.ToString());
                lexer.ReadToken();
                return lexer;
            }
            catch(Exception ex)
            {
                this.HandleException(ex);
                return null;
            }
        }

        void SetSource(string source)
        {
            this.source = source;
            this.lexer = null;
            this.clauses.Clear();
        }

        void HandleException(Exception ex)
        {
            var errorType = (ex as ErrorException)?.ErrorType ?? ErrorType.UnknownError;
            this.LastError = new ErrorInfo(errorType, ex.Message, this.lexer?.Token.Location ?? new SourceLocation());
            this.ErrorOccurred(this, this.LastError);
            this.clauses.Clear();
        }

        void DebugLog(string s) { /*Debug.WriteLine(s);*/ }

        struct Clause
        {
            public Token Token;
            public string VarName;

            public Clause(Token token, string var)
            {
                this.Token = token;
                this.VarName = var;
            }
        }
    }

    public interface IModule
    {
        string Name { get; }
        IReadOnlyDictionary<string, Function> Functions { get; }
        IReadOnlyDictionary<string, Value> Constants { get; }
    }

    public class RunStat
    {
        public static void Increment(Dictionary<string, int> target, string key)
        {
            target[key] = target.TryGetValue(key, out var count) ? count + 1 : 1;
        }
        public int StatementCount;
        public Dictionary<string, int> TokenCounts = new Dictionary<string, int>(); // Key:TokenType name
        public Dictionary<string, int> OperatorCounts = new Dictionary<string, int>(); // Key:Operator name
        public Dictionary<string, int> FunctionInvokedCounts = new Dictionary<string, int>(); // Key:Function name
        public int TotalTokenCount { get { return this.TokenCounts.Sum(kv => kv.Value); } }
        public int TotalOperatorCount { get { return this.OperatorCounts.Sum(kv => kv.Value); } }
        public int TotalFunctionInvokedCount { get { return this.FunctionInvokedCounts.Sum(kv => kv.Value); } }
    }

    public class SourceLocation
    {
        // All properties is 0 based value.
        public int CharIndex { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString() => $"Line:{this.Line + 1} Column:{this.Column + 1}";
    }

    public class ErrorInfo
    {
        public ErrorType Type { get; private set; }
        public string Message { get; private set; }
        public SourceLocation Location { get; private set; }

        internal ErrorInfo(ErrorType type, string message, SourceLocation location)
        {
            this.Type = type;
            this.Message = message;
            this.Location = location;
        }

        public override string ToString() =>
            !string.IsNullOrEmpty(this.Message) ? $"{this.Message} at {this.Location}" : string.Empty;
    }

    public class Value
    {
        public static readonly Value Zero = new Value(0m);

        public DataType DataType { get; }
        public decimal Number { get; }
        public string String { get; }

        public Value(decimal n) : this()
        {
            this.DataType = DataType.Number;
            this.Number = n;
            this.String = this.Number.ToString();
        }

        public Value(long n) : this((decimal)n) { }
        public Value(double n) : this((decimal)n) { }
        public Value(bool b) : this(b ? 1m : 0m) { }

        public Value(string s) : this()
        {
            this.DataType = DataType.String;
            this.Number = 0m;
            this.String = s;
        }

        Value() { }

        public override string ToString() =>
            (this.DataType == DataType.Number) ? this.String : $"'{this.String}'";

        public bool TryToInteger(out long integer)
        {
            if (this.Number == decimal.Truncate(this.Number) &&
                long.MinValue <= this.Number && this.Number <= long.MaxValue)
            {
                integer = (long)this.Number;
                return true;
            }
            else
            {
                integer = 0;
                return false;
            }
        }

        internal bool IsTrue() =>
            (this.DataType == DataType.Number) ? (this.Number != 0m) : !string.IsNullOrEmpty(this.String);
    }

    public class AssigningEventArgs : EventArgs
    {
        public string VarName { get; private set; }
        public Value Value { get; private set; }

        public AssigningEventArgs(string varName, Value value)
        {
            this.VarName = varName;
            this.Value = value;
        }
    }

    class Lexer
    {
        public event EventHandler<Token> TokenRead = delegate { };

        public Token Token { get; private set; }
        public Token NextToken { get; private set; }
        public static readonly string[] Keywords = new[] { "if", "elif", "else", "then", "for", "in", "to", "repeat", "do", "end", "continue", "break", "exit", "or", "and", "not" };
        public static readonly string LineCommentMarker = "#";
        public static readonly char[] StringQuoteMarkers = { '"', '\'' };

        string source;
        SourceLocation currentLocation;
        char currentChar;
        char nextChar;

        public Lexer(string input)
        {
            this.source = input;
            this.Move(new SourceLocation());
        }

        Lexer() { }

        public static List<Token> SplitTokens(string input)
        {
            var tokens = new List<Token>();
            try
            {
                var lexer = new Lexer(input);
                while (lexer.ReadToken().Type != TokenType.Eof)
                    tokens.Add(lexer.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return tokens;
        }

        public void Move(SourceLocation location)
        {
            this.currentLocation = location;
            this.currentChar = this.GetCharAt(location.CharIndex);
            this.nextChar = this.GetCharAt(location.CharIndex + 1);
            this.Token = Token.None;
            this.NextToken = Token.None;
        }

        public Token ReadToken()
        {
            if (this.NextToken.Type == TokenType.None)
                this.NextToken = this.ReadTokenInternal();
            this.Token = this.NextToken;
            this.NextToken = this.ReadTokenInternal();
            return this.Token;
        }

        Token ReadTokenInternal()
        {
            while (this.currentChar != '\n' && char.IsWhiteSpace(this.currentChar))
                this.ReadChar();

            Token token;
            if (this.currentChar == (char)0) token = new Token(TokenType.Eof, this.currentLocation);
            else if (this.currentChar == '#') token = this.ReadComment();
            else if (this.IsLetterOrUnderscore(this.currentChar)) token = this.ReadIdentifier();
            else if (char.IsDigit(this.currentChar)) token = this.ReadNumber();
            else if (this.IsStringEnclosure(this.currentChar)) token = this.ReadString(this.currentChar);
            else token = this.ReadOperator();

            this.TokenRead(this, token);
            return token;
        }

        Token ReadComment()
        {
            var location = this.currentLocation;
            this.ReadChar();
            while (this.currentChar != '\n' && this.currentChar != (char)0) this.ReadChar();
            var type = (this.currentChar == '\n') ? TokenType.NewLine : TokenType.Eof;
            this.ReadChar();
            return new Token(type, location);
        }

        Token ReadIdentifier()
        {
            var location = this.currentLocation;
            var identifier = this.currentChar.ToString();
            while (this.IsLetterOrDigitOrUnderscore(this.ReadChar())) identifier += this.currentChar;
            var type = TokenType.Identifer;
            foreach (var keyword in Lexer.Keywords)
            {
                if(identifier == keyword)
                {
                    var capitalized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(identifier);
                    type = (TokenType)Enum.Parse(typeof(TokenType), capitalized);
                    break;
                }
            }
            return new Token(type, location, identifier);
        }

        Token ReadNumber()
        {
            var location = this.currentLocation;
            var sb = new StringBuilder();
            int radix = 10;
            if (this.currentChar == '0')
            {
                var type = this.nextChar;
                radix =
                    (type == 'x') ? 16 :
                    (type == 'o') ? 8 :
                    (type == 'b') ? 2 :
                    10;
                if (radix != 10)
                {
                    sb.Append(this.currentChar);
                    sb.Append(this.nextChar);
                    this.ReadChar();
                    this.ReadChar();
                }
            }
            while (char.IsLetterOrDigit(this.currentChar) || this.currentChar == '.')
            {
                sb.Append(this.currentChar);
                this.ReadChar();
            }
            var s = sb.ToString();
            decimal n;
            if (radix <= 0 || 36 < radix || s.Length == 0)
            {
                throw new ErrorException(ErrorType.InvalidNumberFormat, $"'{sb}'");
            }
            else if (radix == 10)
            {
                if (!decimal.TryParse(s, out n))
                {
                    throw new ErrorException(ErrorType.InvalidNumberFormat, $"'{sb}'");
                }
            }
            else
            {
                long n64 = 0;
                foreach (char d in s.Substring(2).ToLower())
                {
                    n64 *= radix;
                    int v =
                        ('0' <= d && d <= '9') ? (d - '0') :
                        ('a' <= d && d <= 'z') ? (d - 'a' + 10) :
                        throw new ErrorException(ErrorType.InvalidNumberFormat, $"'{sb}'");
                    if (radix <= v)
                    {
                        throw new ErrorException(ErrorType.InvalidNumberFormat, $"'{sb}'");
                    }
                    n64 += v;
                }
                n = n64;
            }
            return new Token(TokenType.Number, location, s, n);
        }

        Token ReadString(char enclosure)
        {
            var location = this.currentLocation;
            var sb = new StringBuilder();
            while (this.ReadChar() != enclosure)
            {
                if (this.currentChar == (char)0) throw new ErrorException(ErrorType.InvalidStringLiteral, "EOF while scanning string literal");
                if (this.currentChar == '\n') throw new ErrorException(ErrorType.InvalidStringLiteral, "NewLine while scanning string literal");
                if (this.currentChar == '\\')
                {
                    // Escape sequence
                    var c = char.ToLower(this.ReadChar());
                    if (c == 'n') sb.Append('\n');
                    else if (c == 'r') sb.Append('\r');
                    else if (c == 't') sb.Append('\t');
                    else if (c == '\\') sb.Append('\\');
                    else if (c == enclosure) sb.Append(enclosure);
                    else sb.Append(c);
                }
                else
                {
                    sb.Append(this.currentChar);
                }
            }
            this.ReadChar();
            var s = sb.ToString();
            return new Token(TokenType.String, location, s);
        }

        Token ReadOperator()
        {
            var location = this.currentLocation;
            var index = this.currentLocation.CharIndex;
            TokenType type;
            if (this.currentChar == '\n') type = TokenType.NewLine;
            else if (this.currentChar == ':') type = TokenType.Colon;
            else if (this.currentChar == ',') type = TokenType.Comma;
            else if (this.currentChar == '=' && this.nextChar == '=') { type = TokenType.Equal; this.ReadChar(); }
            else if (this.currentChar == '=') type = TokenType.Assign;
            else if (this.currentChar == '!' && this.nextChar == '=') { type = TokenType.NotEqual; this.ReadChar(); }
            else if (this.currentChar == '+') type = TokenType.Plus;
            else if (this.currentChar == '-') type = TokenType.Minus;
            else if (this.currentChar == '*' && this.nextChar == '*') { type = TokenType.Exponent; this.ReadChar(); }
            else if (this.currentChar == '*') type = TokenType.Multiply;
            else if (this.currentChar == '/' && this.nextChar == '/') { type = TokenType.FloorDivision; this.ReadChar(); }
            else if (this.currentChar == '/') type = TokenType.Division;
            else if (this.currentChar == '%') type = TokenType.Remainder;
            else if (this.currentChar == '(') type = TokenType.LeftParen;
            else if (this.currentChar == ')') type = TokenType.RightParen;
            else if (this.currentChar == '<' && this.nextChar == '=') { type = TokenType.LessEqual; this.ReadChar(); }
            else if (this.currentChar == '<' && this.nextChar == '<') { type = TokenType.BitwiseLeftShift; this.ReadChar(); }
            else if (this.currentChar == '<') type = TokenType.Less;
            else if (this.currentChar == '>' && this.nextChar == '=') { type = TokenType.GreaterEqual; this.ReadChar(); }
            else if (this.currentChar == '>' && this.nextChar == '>') { type = TokenType.BitwiseRightShift; this.ReadChar(); }
            else if (this.currentChar == '>') type = TokenType.Greater;
            else if (this.currentChar == '&') type = TokenType.BitwiseAnd;
            else if (this.currentChar == '|') type = TokenType.BitwiseOr;
            else if (this.currentChar == '^') type = TokenType.BitwiseXor;
            else if (this.currentChar == '~') type = TokenType.BitwiseNot;
            else type = TokenType.Unkown;
            this.ReadChar();
            return new Token(type, location, this.source.Substring(index, this.currentLocation.CharIndex - index));
        }

        char ReadChar()
        {
            this.AdvanceLocation();
            this.currentChar = this.nextChar;
            this.nextChar = this.GetCharAt(this.currentLocation.CharIndex + 1);
            return this.currentChar;
        }

        void AdvanceLocation()
        {
            if (this.currentChar == '\n')
            {
                this.currentLocation.Column = 0;
                this.currentLocation.Line++;
            }
            else
            {
                this.currentLocation.Column++;
            }
            this.currentLocation.CharIndex++;
        }

        char GetCharAt(int index) =>
            (0 <= index && index < this.source.Length) ? this.source[index] : (char)0;

        bool IsLetterOrUnderscore(char c) => (char.IsLetter(c) || c == '_');

        bool IsLetterOrDigitOrUnderscore(char c) => (char.IsLetterOrDigit(c) || c == '_');

        bool IsStringEnclosure(char c) => (c == '"' || c == '\'');
    }

    public enum TokenType
    {
        None, Unkown,

        Identifer, String, Number,

        // Statement keyword
        If, Elif, Else, Then, For, In, To, Repeat, Do, End, Continue, Break, Exit,

        // Symbol
        NewLine, Colon, Comma, Assign, LeftParen, RightParen,

        OperatorBegin,

        // Arithmetic operator
        Plus, Minus, Multiply, Division, FloorDivision, Remainder, Exponent,

        // Bitwise operator
        BitwiseOperatorBegin,
        BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseNot, BitwiseLeftShift, BitwiseRightShift,
        BitwiseOperatorEnd,

        // Comparison operator
        Equal, Less, Greater, NotEqual, LessEqual, GreaterEqual,

        // Logical operator
        Or, And, Not,

        OperatorEnd,

        Eof = -1
    }

    struct Token
    {
        public static Token None = new Token() { Type = TokenType.None, String = string.Empty };

        public TokenType Type { get; private set; }
        public SourceLocation Location { get; private set; }
        public string String { get; private set; }
        public decimal Number { get; private set; }

        public Token(TokenType type, SourceLocation location) : this(type, location, string.Empty, 0m) { }
        public Token(TokenType type, SourceLocation location, string str) : this(type, location, str, 0m) { }
        public Token(TokenType type, SourceLocation location, string str, decimal number) : this()
        {
            this.Type = type;
            this.Location = location;
            this.String = str;
            this.Number = number;
        }

        public bool IsCompoundStatement() { return this.Type == TokenType.If || this.IsLoop(); }
        public bool IsLoop() { return this.Type == TokenType.For || this.Type == TokenType.Repeat; }
        public bool IsOperator() { return TokenType.OperatorBegin < this.Type && this.Type < TokenType.OperatorEnd; }
        public static bool IsBitwiseOperator(TokenType type) { return TokenType.BitwiseOperatorBegin < type && type < TokenType.BitwiseOperatorEnd; }

        public override string ToString() => $"'{this.String.Replace("\n", "\\n")}'({this.Type})";
    }

    public class ErrorException : Exception
    {
        public ErrorType ErrorType { get; private set; }

        public ErrorException(ErrorType type, string message = null) : base((message != null) ? $"{type}:{message}" : $"{type}")
        {
            this.ErrorType = type;
        }
        public ErrorException(ErrorType type, Exception inner) : base($"{type}", inner)
        {
            this.ErrorType = type;
        }
    }
}
