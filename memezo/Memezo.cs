using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Suconbu.Scripting.Memezo
{
    public enum DataType { Number, String }
    public enum ErrorType
    {
        NothingSource, UnexpectedToken, UnknownToken, MissingToken, UndeclaredIdentifier,
        NotSupportedOperation, UnknownOperator, InvalidNumberOfArguments, InvalidDataType, InvalidOperation, InvalidParameter,
        InvalidNumberFormat, InvalidStringLiteral, UnknownError
    }
    public delegate Value Function(List<Value> args);

    public class Interpreter
    {
        public event EventHandler<string> Output = delegate { };
        public event EventHandler<ErrorInfo> ErrorOccurred = delegate { };
        public event EventHandler<string> FunctionInvoking = delegate { };

        public static readonly string[] Keywords = Lexer.Keywords;
        public static readonly string LineCommentMarker = Lexer.LineCommentMarker;
        public static readonly char[] StringQuoteMarkers = Lexer.StringQuoteMarkers;
        public Dictionary<string, Function> Functions { get; private set; } = new Dictionary<string, Function>();
        public Dictionary<string, Value> Vars { get; private set; } = new Dictionary<string, Value>();
        public ErrorInfo LastError { get; private set; }
        public RunStat Stat { get; private set; } = new RunStat();
        public IEnumerable<IFunctionLibrary> InstalledLibraries { get { return this.installedLibraries; } }
        public string Source { get => this.source; set => this.UpdateSource(value); }

        string source;
        Lexer lexer;
        SourceLocation statementLocation;
        int nestingLevelOfDeferredSource;
        readonly StringBuilder interactiveSource = new StringBuilder();
        readonly Stack<Clause> clauses = new Stack<Clause>();
        readonly Dictionary<TokenType, int> operatorPrecs = new Dictionary<TokenType, int>()
        {
            { TokenType.Exponent, 7 },
            { TokenType.Multiply, 6 }, {TokenType.Division, 6 }, {TokenType.FloorDivision, 6 }, {TokenType.Remainder, 6 },
            { TokenType.Plus, 5 }, { TokenType.Minus, 5 },
            { TokenType.Equal, 4 }, { TokenType.NotEqual, 4 }, { TokenType.Less, 4 }, { TokenType.Greater, 4 }, { TokenType.LessEqual, 4 },  { TokenType.GreaterEqual, 4 },
            { TokenType.Not, 3 },
            { TokenType.And, 2 },
            { TokenType.Or, 1 }
        };
        public List<IFunctionLibrary> installedLibraries = new List<IFunctionLibrary>();

        public Interpreter() { }

        public void Install(params IFunctionLibrary[] libraries)
        {
            foreach (var library in libraries)
            {
                foreach (var f in library.GetFunctions())
                {
                    this.Functions[f.Key] = f.Value;
                }
                this.installedLibraries.Add(library);
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
                if (this.lexer == null) throw new InternalErrorException(ErrorType.NothingSource);
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

            if (type == TokenType.Unkown) throw new InternalErrorException(ErrorType.UnknownToken, $"{this.lexer.Token}");
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
            else this.Output(this, this.Expr().ToString());

            return continueToRun;
        }

        void OnIf()
        {
        Start:
            var statementToken = this.lexer.Token;
            this.lexer.ReadToken();
            if (statementToken.Type == TokenType.If)
                this.clauses.Push(new Clause(statementToken, null));
            var result = (statementToken.Type == TokenType.If || statementToken.Type == TokenType.Elif) ? this.Expr().Boolean() : true;

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
                throw new InternalErrorException(ErrorType.UnexpectedToken, $"{this.lexer.Token}");

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
            var name = this.lexer.Token.Text;

            var token = this.lexer.ReadToken();
            if (token.Type != TokenType.Assign && token.Type != TokenType.In)
                throw new InternalErrorException(ErrorType.UnexpectedToken, $"{token}");

            this.lexer.ReadToken();
            var fromValue = this.Expr();

            this.VerifyToken(this.lexer.Token, TokenType.To);

            this.lexer.ReadToken();
            var toValue = this.Expr();

            if (this.clauses.Count == 0 || this.clauses.Peek().Var != name)
            {
                this.Vars[name] = fromValue;
                this.clauses.Push(new Clause(statementToken, name));
            }

            if (this.lexer.Token.Type == TokenType.Colon || this.lexer.Token.Type == TokenType.Do) this.lexer.ReadToken();

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: For {this.Vars[name]} to {toValue}");

            if (this.Vars[name].BinaryOperation(toValue, TokenType.Greater).Number == 1) this.FinishLoop();
        }

        void OnRepeat()
        {
            var statementToken = this.lexer.Token;
            this.lexer.ReadToken();
            var count = this.Expr();
            var name = $"${this.statementLocation.CharIndex}";
            if (this.clauses.Count == 0 || this.clauses.Peek().Var != name)
            {
                this.Vars[name] = Value.Zero;
                this.clauses.Push(new Clause(statementToken, name));
            }

            if (this.lexer.Token.Type == TokenType.Colon || this.lexer.Token.Type == TokenType.Do) this.lexer.ReadToken();

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: Repeat {count}");

            if (this.Vars[name].BinaryOperation(count, TokenType.GreaterEqual).Number == 1) this.FinishLoop();
        }

        void OnEnd()
        {
            this.lexer.ReadToken();

            if (this.clauses.Count <= 0)
                throw new InternalErrorException(ErrorType.UnexpectedToken, $"{TokenType.End}");

            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: End");

            var clause = this.clauses.Peek();
            if (clause.Token.Type == TokenType.If)
                this.EndIf(clause);
            else if (clause.Token.IsLoop())
                this.EndFor(clause);
            else
                throw new InternalErrorException(ErrorType.UnexpectedToken, $"{clause.Token}");
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
            throw new InternalErrorException(ErrorType.UnexpectedToken, $"{TokenType.Continue}");
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
            throw new InternalErrorException(ErrorType.UnexpectedToken, $"{TokenType.Break}");
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
            if (clause.Var.StartsWith("$")) this.Vars.Remove(clause.Var);
        }

        void EndIf(Clause clause)
        {
            this.clauses.Pop();
        }

        void EndFor(Clause clause)
        {
            this.Vars[clause.Var] = this.Vars[clause.Var].BinaryOperation(new Value(1), TokenType.Plus);
            this.lexer.Move(clause.Token.Location);
            this.lexer.ReadToken();
        }

        void OnExit() { }

        void OnEof()
        {
            if (this.clauses.Count > 0) throw new InternalErrorException(ErrorType.MissingToken, $"{TokenType.End}");
        }

        void OnAssign()
        {
            var name = this.lexer.Token.Text;
            if (this.Functions.ContainsKey(name)) throw new InternalErrorException(ErrorType.InvalidOperation, $"'{name}' is a function");
            this.VerifyToken(this.lexer.ReadToken(), TokenType.Assign);
            this.lexer.ReadToken();
            this.Vars[name] = this.Expr();
            this.DebugLog($"{this.lexer.Token.Location.Line + 1}: Assign {name}={this.Vars[name].ToString()}");
        }

        Value Expr(int lowestPrec = int.MinValue + 1)
        {
            var lhs = this.Primary();
            this.lexer.ReadToken();
            while (true)
            {
                if (!this.lexer.Token.IsOperator()) break;
                if (!this.operatorPrecs.TryGetValue(this.lexer.Token.Type, out var prec)) prec = int.MinValue;
                if (prec <= lowestPrec) break;

                var type = this.lexer.Token.Type;
                this.lexer.ReadToken();
                var rhs = this.Expr(prec);
                lhs = lhs.BinaryOperation(rhs, type);
                RunStat.Increment(this.Stat.OperatorCounts, type.ToString());
            }

            return lhs;
        }

        Value Primary()
        {
            var primary = Value.Zero;

            if (this.lexer.Token.Type == TokenType.Value)
            {
                primary = this.lexer.Token.Value;
            }
            else if (this.lexer.Token.Type == TokenType.Plus || this.lexer.Token.Type == TokenType.Minus)
            {
                var type = this.lexer.Token.Type;
                this.lexer.ReadToken();
                primary = Value.Zero.BinaryOperation(this.Primary(), type); // we dont realy have a unary operators
            }
            else if (this.lexer.Token.Type == TokenType.Not)
            {
                this.lexer.ReadToken();
                primary = new Value(this.Primary().Boolean() ? 0.0 : 1.0);
            }
            else if (this.lexer.Token.Type == TokenType.LeftParen)
            {
                this.lexer.ReadToken();
                primary = this.Expr();
                this.VerifyToken(this.lexer.Token, TokenType.RightParen);
            }
            else if (this.lexer.Token.Type == TokenType.Identifer)
            {
                var identifier = this.lexer.Token.Text;
                if (this.Vars.ContainsKey(identifier))
                {
                    primary = this.Vars[identifier];
                }
                else if (this.Functions.ContainsKey(identifier))
                {
                    this.VerifyToken(this.lexer.ReadToken(), TokenType.LeftParen);
                    var args = this.ReadArguments();
                    this.FunctionInvoking(this, identifier);
                    primary = this.Functions[identifier](args);
                    RunStat.Increment(this.Stat.FunctionInvokedCounts, identifier);
                }
                else
                {
                    throw new InternalErrorException(ErrorType.UndeclaredIdentifier, $"'{identifier}'");
                }
            }
            else
            {
                throw new InternalErrorException(ErrorType.UnexpectedToken, $"{this.lexer.Token}");
            }

            return primary;
        }

        void VerifyToken(Token token, TokenType expectedType)
        {
            if (token.Type != expectedType) throw new InternalErrorException(ErrorType.MissingToken, $"{expectedType}");
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
                break;
            }
            return args;
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

        void UpdateSource(string source)
        {
            this.source = source;
            this.lexer = null;
            this.clauses.Clear();
        }

        void HandleException(Exception ex)
        {
            var errorType = (ex as InternalErrorException)?.ErrorType ?? ErrorType.UnknownError;
            this.LastError = new ErrorInfo(errorType, ex.Message, this.lexer?.Token.Location ?? new SourceLocation());
            this.ErrorOccurred(this, this.LastError);
            this.clauses.Clear();
        }

        void DebugLog(string s) { /*Debug.WriteLine(s);*/ }

        struct Clause
        {
            public Token Token;
            public string Var;

            public Clause(Token token, string var)
            {
                this.Token = token;
                this.Var = var;
            }
        }
    }

    public interface IFunctionLibrary
    {
        string Name { get; }
        IEnumerable<KeyValuePair<string, Function>> GetFunctions();
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

    public struct SourceLocation
    {
        // All properties is 0 based value.
        public int CharIndex { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString() => $"Line:{this.Line + 1} Column:{this.Column + 1}";
    }

    public struct ErrorInfo
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

    public struct Value
    {
        public static readonly Value Zero = new Value(0.0);

        public DataType Type { get; private set; }
        public double Number { get; private set; }
        public string String { get; private set; }

        public Value(double n) : this()
        {
            this.Type = DataType.Number;
            this.Number = n;
            this.String = n.ToString();
        }

        public Value(string s) : this()
        {
            this.Type = DataType.String;
            this.Number = 0.0;
            this.String = s;
        }

        public override string ToString() =>
            (this.Type == DataType.Number) ? this.String : $"'{this.String}'";

        internal bool Boolean() =>
            (this.Type == DataType.Number) ? (this.Number != 0.0) : !string.IsNullOrEmpty(this.String);

        internal Value BinaryOperation(Value b, TokenType tokenType)
        {
            var a = this;

            if (tokenType == TokenType.Multiply)
            {
                return
                    (a.Type == DataType.Number && b.Type == DataType.Number) ?
                        new Value(a.Number * b.Number) :
                    (a.Type == DataType.String && b.Type == DataType.Number) ?
                        new Value((new StringBuilder().Insert(0, a.String, (int)Math.Max(b.Number, 0.0))).ToString()) :
                    (a.Type == DataType.Number && b.Type == DataType.String) ?
                        new Value((new StringBuilder().Insert(0, b.String, (int)Math.Max(a.Number, 0.0))).ToString()) :
                    throw new InternalErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.Type}");
            }

            if (a.Type != b.Type)
            {
                if (a.Type == DataType.Number && b.Type == DataType.String)
                    a = new Value(a.ToString());
                else if (a.Type == DataType.String && b.Type == DataType.Number)
                    b = new Value(b.ToString());
                else
                    throw new InternalErrorException(ErrorType.InvalidDataType, $"{a.Type} x {b.Type}");
            }

            if (tokenType == TokenType.Plus)
            {
                return
                    (a.Type == DataType.Number) ? new Value(a.Number + b.Number) :
                    (a.Type == DataType.String) ? new Value(a.String + b.String) :
                    throw new InternalErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.Type}");
            }
            else if (tokenType == TokenType.Equal)
            {
                return
                    (a.Type == DataType.Number) ? new Value(a.Number == b.Number ? 1 : 0) :
                    (a.Type == DataType.String) ? new Value(a.String == b.String ? 1 : 0) :
                    throw new InternalErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.Type}");
            }
            else if (tokenType == TokenType.NotEqual)
            {
                return
                    (a.Type == DataType.Number) ? new Value(a.Number != b.Number ? 1 : 0) :
                    (a.Type == DataType.String) ? new Value(a.String != b.String ? 1 : 0) :
                    throw new InternalErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.Type}");
            }
            else
            {
                if (a.Type != DataType.Number) throw new InternalErrorException(ErrorType.NotSupportedOperation, $"{tokenType} for {a.Type}");

                return
                    (tokenType == TokenType.Minus) ? new Value(a.Number - b.Number) :
                    (tokenType == TokenType.Division) ? new Value(a.Number / b.Number) :
                    (tokenType == TokenType.FloorDivision) ? new Value(Math.Floor(a.Number / b.Number)) :
                    (tokenType == TokenType.Remainder) ? new Value(a.Number % b.Number) :
                    (tokenType == TokenType.Exponent) ? new Value(Math.Pow(a.Number, b.Number)) :
                    (tokenType == TokenType.Less) ? new Value(a.Number < b.Number ? 1 : 0) :
                    (tokenType == TokenType.Greater) ? new Value(a.Number > b.Number ? 1 : 0) :
                    (tokenType == TokenType.LessEqual) ? new Value(a.Number <= b.Number ? 1 : 0) :
                    (tokenType == TokenType.GreaterEqual) ? new Value(a.Number >= b.Number ? 1 : 0) :
                    (tokenType == TokenType.And) ? new Value(a.Number != 0.0 && b.Number != 0.0 ? 1 : 0) :
                    (tokenType == TokenType.Or) ? new Value(a.Number != 0.0 || b.Number != 0.0 ? 1 : 0) :
                    throw new InternalErrorException(ErrorType.UnknownOperator, $"{tokenType}");
            }
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
            while (char.IsDigit(this.currentChar) || this.currentChar == '.')
            {
                sb.Append(this.currentChar);
                this.ReadChar();
            }
            var s = sb.ToString();
            if (!double.TryParse(s, out var n))
                throw new InternalErrorException(ErrorType.InvalidNumberFormat, $"'{sb}'");
            return new Token(TokenType.Value, location, s, new Value(n));
        }

        Token ReadString(char enclosure)
        {
            var location = this.currentLocation;
            var sb = new StringBuilder();
            while (this.ReadChar() != enclosure)
            {
                if (this.currentChar == (char)0) throw new InternalErrorException(ErrorType.InvalidStringLiteral, "EOF while scanning string literal");
                if (this.currentChar == '\n') throw new InternalErrorException(ErrorType.InvalidStringLiteral, "NewLine while scanning string literal");
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
            return new Token(TokenType.Value, location, s, new Value(s));
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
            else if (this.currentChar == '<') type = TokenType.Less;
            else if (this.currentChar == '>' && this.nextChar == '=') { type = TokenType.GreaterEqual; this.ReadChar(); }
            else if (this.currentChar == '>') type = TokenType.Greater;
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

    enum TokenType
    {
        None, Unkown,

        Identifer, Value,

        // Statement keyword
        If, Elif, Else, Then, For, In, To, Repeat, Do, End, Continue, Break, Exit,

        // Symbol
        NewLine, Colon, Comma, Assign, LeftParen, RightParen,

        OperatorsBegin,

        // Arithmetic operator
        Plus, Minus, Multiply, Division, FloorDivision, Remainder, Exponent,

        // Comparison operator
        Equal, Less, Greater, NotEqual, LessEqual, GreaterEqual,

        // Logical operator
        Or, And, Not,

        OperatorsEnd,

        Eof = -1
    }

    struct Token
    {
        public static Token None = new Token() { Type = TokenType.None, Text = string.Empty };

        public TokenType Type { get; private set; }
        public SourceLocation Location { get; private set; }
        public string Text { get; private set; }
        public Value Value { get; private set; }

        public Token(TokenType type, SourceLocation location) : this(type, location, string.Empty, Value.Zero) { }
        public Token(TokenType type, SourceLocation location, string text) : this(type, location, text, Value.Zero) { }
        public Token(TokenType type, SourceLocation location, string text, Value value) : this()
        {
            this.Type = type;
            this.Location = location;
            this.Text = text;
            this.Value = value;
        }

        public bool IsCompoundStatement() { return this.Type == TokenType.If || this.IsLoop(); }
        public bool IsLoop() { return this.Type == TokenType.For || this.Type == TokenType.Repeat; }
        public bool IsOperator() { return TokenType.OperatorsBegin < this.Type && this.Type < TokenType.OperatorsEnd; }

        public override string ToString() => $"'{this.Text.Replace("\n", "\\n")}'({this.Type})";
    }

    class InternalErrorException : Exception
    {
        public ErrorType ErrorType { get; private set; }

        public InternalErrorException(ErrorType type, string message = null) : base($"{type}:{message}")
        {
            this.ErrorType = type;
        }
        public InternalErrorException() : base() { }
        public InternalErrorException(string message) : base(message) { }
        public InternalErrorException(string message, Exception inner) : base(message, inner) { }
    }
}
