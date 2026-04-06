using NUnit.Framework;
using System;
using System.Collections.Generic;
using NiceAttributes.Editor.Utility;

namespace NiceAttributes.Tests.Editor
{
    public class MathematicalParserTests
    {
        #region Helpers
        static (object result, bool successful) Eval( string expression )
            => MathematicalParser.Evaluate( expression );

        static (object result, bool successful) EvalWithVariables( string expression, Dictionary<string, object> variables )
            => MathematicalParser.Evaluate( expression, name => variables.TryGetValue( name, out var v ) ? v : throw new KeyNotFoundException( $"Variable '{name}' not found" ) );

        static (object result, bool successful) EvalWithFunctions( string expression, Dictionary<string, Func<object, object>> functions )
            => MathematicalParser.Evaluate( expression, getFunctionValue: ( name, parameters ) => functions.TryGetValue( name, out var fn ) ? fn( parameters ) : throw new KeyNotFoundException( $"Function '{name}' not found" ) );

        static (object result, bool successful) EvalFull( string expression, Dictionary<string, object> variables = null, Dictionary<string, Func<object, object>> functions = null )
            => MathematicalParser.Evaluate( expression,
                variables != null ? (MathematicalParser.GetVariableValueDelegate)( name => variables.TryGetValue( name, out var v ) ? v : throw new KeyNotFoundException( $"Variable '{name}' not found" ) ) : null,
                functions != null ? (MathematicalParser.GetFunctionValueDelegate)( ( name, parameters ) => functions.TryGetValue( name, out var fn ) ? fn( parameters ) : throw new KeyNotFoundException( $"Function '{name}' not found" ) ) : null );
        #endregion

        #region Basic Arithmetic
        [Test]
        public void Addition()
        {
            var (result, success) = Eval( "1 + 2" );
            Assert.IsTrue( success );
            Assert.AreEqual( 3.0, result );
        }

        [Test]
        public void Subtraction()
        {
            var (result, success) = Eval( "10 - 4" );
            Assert.IsTrue( success );
            Assert.AreEqual( 6.0, result );
        }

        [Test]
        public void Multiplication()
        {
            var (result, success) = Eval( "3 * 5" );
            Assert.IsTrue( success );
            Assert.AreEqual( 15.0, result );
        }

        [Test]
        public void Division()
        {
            var (result, success) = Eval( "20 / 4" );
            Assert.IsTrue( success );
            Assert.AreEqual( 5.0, result );
        }

        [Test]
        public void OrderOfOperations_MultiplicationBeforeAddition()
        {
            var (result, success) = Eval( "2 * 3 + 4" );
            Assert.IsTrue( success );
            Assert.AreEqual( 10.0, result );
        }

        [Test]
        public void OrderOfOperations_DivisionBeforeSubtraction()
        {
            var (result, success) = Eval( "10 - 6 / 2" );
            Assert.IsTrue( success );
            Assert.AreEqual( 7.0, result );
        }

        [Test]
        public void ChainedAddition()
        {
            var (result, success) = Eval( "1 + 2 + 3" );
            Assert.IsTrue( success );
            Assert.AreEqual( 6.0, result );
        }

        [Test]
        public void MixedOperations()
        {
            var (result, success) = Eval( "2 * 3 + 4 * 5" );
            Assert.IsTrue( success );
            Assert.AreEqual( 26.0, result );
        }
        #endregion

        #region Parentheses
        [Test]
        public void Parentheses_OverrideOrderOfOperations()
        {
            var (result, success) = Eval( "(1 + 2) * 3" );
            Assert.IsTrue( success );
            Assert.AreEqual( 9.0, result );
        }

        [Test]
        public void NestedParentheses()
        {
            var (result, success) = Eval( "((2 + 3) * 4)" );
            Assert.IsTrue( success );
            Assert.AreEqual( 20.0, result );
        }

        [Test]
        public void MultipleParenGroups()
        {
            var (result, success) = Eval( "(10 - 3) * (2 + 1)" );
            Assert.IsTrue( success );
            Assert.AreEqual( 21.0, result );
        }

        [Test]
        public void ParenthesesInsideComplexExpression()
        {
            var (result, success) = Eval( "(3.5 + -4.5) / 3" );
            Assert.IsTrue( success );
            Assert.AreEqual( -1.0 / 3.0, (double)result, 0.0001 );
        }
        #endregion

        #region Variables
        [Test]
        public void SingleVariable()
        {
            var vars = new Dictionary<string, object> { { "x", 42.0 } };
            var (result, success) = EvalWithVariables( "x", vars );
            Assert.IsTrue( success );
            Assert.AreEqual( 42.0, result );
        }

        [Test]
        public void TwoVariables_Addition()
        {
            var vars = new Dictionary<string, object> { { "a", 5.0 }, { "b", 3.0 } };
            var (result, success) = EvalWithVariables( "a + b", vars );
            Assert.IsTrue( success );
            Assert.AreEqual( 8.0, result );
        }

        [Test]
        public void VariableInComplexExpression()
        {
            var vars = new Dictionary<string, object> { { "k", 10.0 } };
            var (result, success) = EvalWithVariables( "k * 2 + 1", vars );
            Assert.IsTrue( success );
            Assert.AreEqual( 21.0, result );
        }

        [Test]
        public void VariableNotFound_ReturnsError()
        {
            var (result, success) = EvalWithVariables( "undefinedVar", new Dictionary<string, object>() );
            Assert.IsFalse( success );
            StringAssert.Contains( "ERROR", result.ToString() );
        }
        #endregion

        #region Functions
        [Test]
        public void FunctionWithSingleParameter()
        {
            var funcs = new Dictionary<string, Func<object, object>> {
                { "double", parameters => GetDoubleValue( (parameters as List<object>)[0] ) * 2 }
            };
            var (result, success) = EvalWithFunctions( "double(5)", funcs );
            Assert.IsTrue( success );
            Assert.AreEqual( 10.0, result );
        }

        [Test]
        public void FunctionWithoutParameters()
        {
            var funcs = new Dictionary<string, Func<object, object>> {
                { "pi", parameters => 3.14159 }
            };
            var (result, success) = EvalWithFunctions( "pi()", funcs );
            Assert.IsTrue( success );
            Assert.AreEqual( 3.14159, result );
        }

        [Test]
        public void FunctionInExpression()
        {
            var funcs = new Dictionary<string, Func<object, object>> {
                { "square", parameters => GetDoubleValue( (parameters as List<object>)[0] ) * GetDoubleValue( (parameters as List<object>)[0] ) }
            };
            var (result, success) = EvalWithFunctions( "square(3) + 1", funcs );
            Assert.IsTrue( success );
            Assert.AreEqual( 10.0, result );
        }

        [Test]
        public void FunctionNotFound_ReturnsError()
        {
            var (result, success) = EvalWithFunctions( "nonexistent(1)", new Dictionary<string, Func<object, object>>() );
            Assert.IsFalse( success );
            StringAssert.Contains( "ERROR", result.ToString() );
        }

        static double GetDoubleValue( object obj ) => obj switch {
            float f => f, double d => d, int i => i, long l => l, short s => s, _ => double.NaN
        };
        #endregion

        #region String Concatenation
        [Test]
        public void StringConcatenation()
        {
            var (result, success) = Eval( "'Hello ' + 'World'" );
            Assert.IsTrue( success );
            Assert.AreEqual( "Hello World", result );
        }

        [Test]
        public void NumberToStringConcatenation()
        {
            var (result, success) = Eval( "'Result: ' + 42" );
            Assert.IsTrue( success );
            Assert.AreEqual( "Result: 42", result );
        }

        [Test]
        public void StringNumberStringConcatenation()
        {
            var (result, success) = Eval( "'The result is ' + (3.5 + -4.5) / 3" );
            Assert.IsTrue( success );
            Assert.AreEqual( "The result is " + ((-1.0) / 3.0).ToString(), result );
        }

        [Test]
        public void StringTest()
        {
            var (result, success) = Eval( "'Hello ' + 'World'" );
            Assert.IsTrue( success );
            Assert.AreEqual( "Hello World", result );
        }
        #endregion

        #region Number Formatting
        [Test]
        public void PercentageFormatting()
        {
            var (result, success) = Eval( "3.5:'P'" );
            Assert.IsTrue( success );
            Assert.AreEqual( "350.00%", result );
        }

        [Test]
        public void StringPlusFormattedNumber()
        {
            var (result, success) = Eval( "'Chance: ' + 3.5:'P'" );
            Assert.IsTrue( success );
            Assert.AreEqual( "Chance: 350.00%", result );
        }

        [Test]
        public void FixedPointFormatting()
        {
            var (result, success) = Eval( "3.14159:'F2'" );
            Assert.IsTrue( success );
            Assert.AreEqual( "3.14", result );
        }
        #endregion

        #region Multiple Expressions
        [Test]
        public void MultipleExpressions_ReturnsList()
        {
            var (result, success) = Eval( "1, 2, 3" );
            Assert.IsTrue( success );
            Assert.IsInstanceOf<List<object>>( result );
            var list = result as List<object>;
            Assert.AreEqual( 3, list.Count );
            Assert.AreEqual( 1.0, list[0] );
            Assert.AreEqual( 2.0, list[1] );
            Assert.AreEqual( 3.0, list[2] );
        }

        [Test]
        public void MultipleExpressionsWithStrings()
        {
            var (result, success) = Eval( "'a', 'b'" );
            Assert.IsTrue( success );
            Assert.IsInstanceOf<List<object>>( result );
            var list = result as List<object>;
            Assert.AreEqual( 2, list.Count );
            Assert.AreEqual( "a", list[0] );
            Assert.AreEqual( "b", list[1] );
        }

        [Test]
        public void MultipleExpressionsWithArithmetic()
        {
            var (result, success) = Eval( "1 + 1, 2 * 3, 10 - 5" );
            Assert.IsTrue( success );
            var list = result as List<object>;
            Assert.AreEqual( 3, list.Count );
            Assert.AreEqual( 2.0, list[0] );
            Assert.AreEqual( 6.0, list[1] );
            Assert.AreEqual( 5.0, list[2] );
        }
        #endregion

        #region Error Handling
        [Test]
        public void InvalidTokenChar_ReturnsError()
        {
            var (result, success) = Eval( "1 @ 2" );
            Assert.IsFalse( success );
            StringAssert.Contains( "ERROR", result.ToString() );
        }

        [Test]
        public void MismatchedParentheses_ReturnsError()
        {
            var (result, success) = Eval( "(1 + 2" );
            Assert.IsFalse( success );
            StringAssert.Contains( "ERROR", result.ToString() );
        }

        [Test]
        public void DivisionByZero_ReturnsInfinityOrError()
        {
            var (result, success) = Eval( "1 / 0" );
            // Division by zero in C# double returns Infinity, not an exception
            // The parser may or may not catch this - test that it doesn't crash
            Assert.IsTrue( success || result.ToString().Contains( "ERROR" ) );
        }

        [Test]
        public void EmptyExpression_ReturnsError()
        {
            var (result, success) = Eval( "" );
            Assert.IsFalse( success );
        }
        #endregion

        #region Negative Numbers
        [Test]
        public void NegativeLiteral()
        {
            var (result, success) = Eval( "-5" );
            Assert.IsTrue( success );
            Assert.AreEqual( -5.0, result );
        }

        [Test]
        public void NegativePlusPositive()
        {
            var (result, success) = Eval( "-5 + 3" );
            Assert.IsTrue( success );
            Assert.AreEqual( -2.0, result );
        }

        [Test]
        public void PositivePlusNegative()
        {
            var (result, success) = Eval( "3 + -5" );
            Assert.IsTrue( success );
            Assert.AreEqual( -2.0, result );
        }

        [Test]
        public void NegativeInParentheses()
        {
            var (result, success) = Eval( "(-5) * 2" );
            Assert.IsTrue( success );
            Assert.AreEqual( -10.0, result );
        }

        [Test]
        public void DoubleNegative()
        {
            var (result, success) = Eval( "-5 + -3" );
            Assert.IsTrue( success );
            Assert.AreEqual( -8.0, result );
        }
        #endregion

        #region Edge Cases
        [Test]
        public void SingleNumber()
        {
            var (result, success) = Eval( "42" );
            Assert.IsTrue( success );
            Assert.AreEqual( 42.0, result );
        }

        [Test]
        public void WhitespaceHandling()
        {
            var (result, success) = Eval( "  1   +   2  " );
            Assert.IsTrue( success );
            Assert.AreEqual( 3.0, result );
        }

        [Test]
        public void DecimalNumber()
        {
            var (result, success) = Eval( "3.14" );
            Assert.IsTrue( success );
            Assert.AreEqual( 3.14, (double)result, 0.0001 );
        }

        [Test]
        public void PositiveSignPrefix()
        {
            var (result, success) = Eval( "+5" );
            Assert.IsTrue( success );
            Assert.AreEqual( 5.0, result );
        }
        #endregion
    }
}
