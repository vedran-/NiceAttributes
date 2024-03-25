using System;
using System.Collections.Generic;

namespace NiceAttributes.Editor
{
    /// <summary>
    /// Features:
    ///  - parses mathematical expressions
    ///  - supports variables and functions (if needed). Functions have () after the name, while variables don't.
    ///  - literal strings are between ''
    ///  - supports adding strings to each other, and numbers to strings
    ///  - supports multiple expressions separated by comma; in that case it will return a list of objects
    ///  - supports any other object type returned by GetVariableValue and GetFunctionValue, but it can do arthimetic only with double and string
    /// 
    /// Usage:
    ///     MathematicalParser.Evaluate( "'The result is ' + (3.5 + -4.5) / 3 + k + sin(3.1415/3)" )
    /// </summary>
    public class MathematicalParser
    {
        /// <summary>
        /// Should return value of the variable.
        /// Method should throw an exception if variable with that name does not exist.
        /// 
        /// Example usage: You can use it to access Fields and Properties inside some class.
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>Supported types: string, double, or List of objects</returns>
        public delegate object GetVariableValueDelegate( string variableName );

        /// <summary>
        /// Should return value of calling some function with parameters.
        /// Method should throw an exception if function with that name does not exist, or it has invalid parameters.
        /// 
        /// Example usage: You can use it to call Methods inside some class.
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="parameters">Parameters when calling the function - can be a string, a double, or List of objects</param>
        /// <returns>Supported types: string, double, or List of objects</returns>
        public delegate object GetFunctionValueDelegate( string functionName, object parameters );


        GetVariableValueDelegate GetVariableValue;
        GetFunctionValueDelegate GetFunctionValue;

        enum TokenType { None = 0, String, Number, Operator, Separator, Name }
        #region class TokenInfo
        class TokenInfo
        {
            public TokenType    type;
            public string       text;
            public object       calculatedValue = null;

            public TokenInfo( string text, TokenType type )
            {
                this.type = type;
                this.text = text;
            }
        }
        #endregion class TokenInfo

        List<TokenInfo> tokens = null;
        int       currentTokenIdx = 0;
        TokenType CurrentTokenType => currentTokenIdx < tokens.Count ? tokens[currentTokenIdx].type : TokenType.None;
        string    CurrentTokenText => currentTokenIdx < tokens.Count ? tokens[currentTokenIdx].text : "";
        void      SkipToken() { if( currentTokenIdx < tokens.Count ) currentTokenIdx++; }

        private MathematicalParser() {} // Use static Evaluate() instead

        #region Tokenize()
        private List<TokenInfo> Tokenize( string expression )
        {
            var tokens = new List<TokenInfo>();

            int idx = 0;
            #region ParseNextToken()
            TokenInfo ParseNextToken()
            {
                bool IsEnd() => idx >= expression.Length;
                char NextChar() => idx + 1 < expression.Length ? expression[idx + 1] : (char)0;
                void SkipChar() { if( !IsEnd() ) idx++; }

                while( !IsEnd() && char.IsWhiteSpace( expression[idx] ) ) idx++;    // Skip whitespace
                if( IsEnd() ) return new TokenInfo( null, TokenType.None );

                var ch = expression[idx];

                // String
                if( ch == '\'' )
                {
                    SkipChar();
                    var startPos = idx;
                    while( !IsEnd() && expression[idx] != '\'' ) idx++;
                    var endPos = idx;
                    if( !IsEnd() && expression[idx] == '\'' ) SkipChar();
                    return new TokenInfo( expression[startPos..endPos], TokenType.String );
                }

                // Number, can start with + or -
                if( char.IsDigit(ch) || ((ch == '+' || ch == '-') && char.IsDigit( NextChar() )) )
                {
                    var startPos = idx;
                    if( ch == '+' || ch == '-' ) SkipChar();

                    // Parse whole number
                    while( !IsEnd() && (char.IsDigit( expression[idx] ) || expression[idx] == '.') ) {
                        idx++;
                    }

                    return new TokenInfo( expression[startPos..idx], TokenType.Number );
                }

                // Name - variable or method name; can start with _
                if( ch == '_' || char.IsLetterOrDigit( ch ) )
                {
                    var startPos = idx;
                    while( !IsEnd() && char.IsLetterOrDigit( expression[idx] ) ) idx++;
                    return new TokenInfo( expression[startPos..idx], TokenType.Name );
                }

                // Operator
                if( "+-*/()^".Contains( ch ) ) return new TokenInfo( expression[idx++].ToString(), TokenType.Operator );
                
                if( ",".Contains( ch ) ) return new TokenInfo( expression[idx++].ToString(), TokenType.Separator );

                throw new InvalidOperationException( $"Unexpected token char '{ch}'!" );
            }
            #endregion ParseNextToken()

            while( idx < expression.Length )
            {
                var token = ParseNextToken();
                if( token.text == null || token.type == TokenType.None ) break;

                tokens.Add( token );
            }

            return tokens;
        }
        #endregion Tokenize()


        double GetDoubleValue( object obj )
        {
            double val = obj is float ? (float)obj
                        : obj is double ? (double)obj
                        : obj is int ? (int)obj
                        : obj is long ? (long)obj
                        : 0;
            return val;
        }


        #region [Math] Factor()
        private object Factor()
        {
            if( currentTokenIdx >= tokens.Count ) return null;
            var token = tokens[currentTokenIdx];

            // Check if we already calculated value for this Factor token
            if( token.calculatedValue != null )
            {
                SkipToken();
                return token.calculatedValue;
            }

            // Number
            if( token.type == TokenType.Number ) {
                token.calculatedValue = double.Parse( token.text );
                SkipToken();
                return token.calculatedValue;
            }

            // String
            if( token.type == TokenType.String ) {
                token.calculatedValue = token.text;
                SkipToken();
                return token.calculatedValue;
            }

            // Variable or Method name
            string name = null;
            if( token.type == TokenType.Name )
            {
                name = token.text;
                SkipToken();
            }

            // Expression in parentheses - can follow 'name', then it's a function, and not variable
            object expression = null;
            //if( currentTokenIdx < tokens.Count && tokens[currentTokenIdx] is { text: "(", type: TokenType.Operator } )
            if( CurrentTokenType == TokenType.Operator && CurrentTokenText == "(" )
            {
                SkipToken();

                if( CurrentTokenText == ")" )       // Function without parameters
                {
                    expression = new List<object>();
                    SkipToken();

                } else {                            // Function with parameters
                    // Parse expression between ( and )
                    expression = ExpressionList();

                    if( CurrentTokenText != ")" ) throw new InvalidOperationException( $"Invalid {CurrentTokenType} token '{CurrentTokenText}', expecting ')'" );
                    SkipToken();
                }
            }

            if( name == null && expression == null ) throw new Exception( "Invalid syntax - nothing parsed!" );
            if( name == null && expression != null ) token.calculatedValue = expression;
            if( name != null && expression == null ) token.calculatedValue = GetVariableValue?.Invoke( name );
            if( name != null && expression != null ) token.calculatedValue = GetFunctionValue?.Invoke( name, expression );
            return token.calculatedValue;
        }
        #endregion Factor()

        #region [Math] Term()
        private object Term()
        {
            var result = Factor();
            while( currentTokenIdx < tokens.Count )
            {
                var token = tokens[currentTokenIdx];
                if( token.type != TokenType.Operator ) break;

                if( token.text == "*" )
                {
                    SkipToken();
                    var right = Factor();
                    result = GetDoubleValue(result) * GetDoubleValue(right);
                } else if( token.text == "/" )
                {
                    SkipToken();
                    var right = Factor();
                    result = GetDoubleValue(result) / GetDoubleValue(right);
                } else break;
            }
            return result;
        }
        #endregion Term()

        #region [Math] Expression()
        private object Expression()
        {
            var result = Term();
            while( currentTokenIdx < tokens.Count )
            {
                var token = tokens[currentTokenIdx];
                if( token.type != TokenType.Operator ) break;

                if( token.text == "+" )
                {
                    SkipToken();
                    var right = Term();
                    if( result is string || right is string ) {
                        result = result.ToString() + right.ToString();
                    } else {
                        result = GetDoubleValue(result) + GetDoubleValue(right);
                    }
                } else if( token.text == "-" )
                {
                    SkipToken();
                    var right = Term();
                    result = GetDoubleValue(result) - GetDoubleValue(right);
                } else break;
            }


            return result;
        }
        #endregion Expression()

        #region [Math] ExpressionList()
        private object ExpressionList()
        {
            var result = Expression();
            if( CurrentTokenType != TokenType.Separator ) return result;    // Not a list, just single expression

            var list = new List<object> { result };
            while( CurrentTokenType == TokenType.Separator )
            {
                SkipToken();
                result = Expression();
                list.Add( result );
            }

            return list;
        }
        #endregion ExpressionList()


        #region [API] Evaluate()
        public static (object result, bool successful) Evaluate( string expression,
            GetVariableValueDelegate getVariableValue = null,
            GetFunctionValueDelegate getFunctionValue = null )
        {
            var n = new MathematicalParser() {
                GetVariableValue = getVariableValue,
                GetFunctionValue = getFunctionValue
            };

            try {
                n.tokens = n.Tokenize( expression );
                n.currentTokenIdx = 0;
                return (n.ExpressionList(), true);
            } catch( Exception ex ) {
                return ($"<color=red><b>ERROR</b></color>: {ex.Message}", false);
                //return $"<color=red><b>ERROR</b></color>: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}";
            }
        }
        #endregion Evaluate()
    }
}
