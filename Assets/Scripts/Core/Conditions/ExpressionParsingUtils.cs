using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Conditions {
	public static class ExpressionParsingUtils {
		public static string InfixToReversePolishNotation( string expression ) {
			var tokens = expression.Split( ' ' );

			var s = new Stack<string>();
			var outputList = new List<string>();

			foreach ( var c in tokens ) {
				switch ( c ) {
					case "(":
						s.Push( c );
						break;
					case ")":
						while ( s.Count != 0 && s.Peek() != "(" ) {
							outputList.Add( s.Pop() );
						}
						if ( s.Count > 0 ) {
							s.Pop();
						}
						else {
							Debug.LogError( "[ExpressionParsingUtils][InfixToReversePolishNotation] Invalid brackets order" );
						}
						break;
					default:
						if ( !IsOperator( c ) ) {
							outputList.Add( c );
						}
						break;
				}
				if ( IsOperator( c ) ) {
					while ( s.Count != 0 && Priority( s.Peek() ) >= Priority( c ) ) {
						outputList.Add( s.Pop() );
					}
					s.Push( c );
				}
			}

			while ( s.Count > 0 ) //if any operators remain in the stack, pop all & add to output list until stack is empty
			{
				outputList.Add( s.Pop() );
			}
			return string.Join( " ", outputList.ToArray() );
		}

		private static int Priority( string c ) {
			if ( c == "!" || c == "^" ) {
				return 3;
			}
			if ( c == "&" || c == "*" || c == "/" ) {
				return 2;
			}
			if ( c == "|" || c == "+" || c == "-" ) {
				return 1;
			}
			return 0;
		}

		private static bool IsOperator( string c ) {
			return c == "!" || c == "&" || c == "|" || c == "+" || c == "-" || c == "*" || c == "/" || c == "^";
		}

		public static void ValidateExpression( string expression, string errorMessage ) {
			ValidateBracketsForExpression( expression, errorMessage );
			ValidateExpressionsDelimeters( expression, errorMessage );
		}

		private static void ValidateBracketsForExpression( string expression, string errorMessage ) {
			var counter = 0;
			foreach ( var c in expression ) {
				if ( c == '(' ) {
					counter += 1;
				}
				else if ( c == ')' ) {
					counter -= 1;
				}
				Assert.IsFalse( counter < 0, errorMessage );
			}
			Assert.AreEqual( 0, counter, errorMessage );
		}

		private static void ValidateExpressionsDelimeters( string expression, string errorMessage ) {
			for ( var i = 0; i < expression.Length; ++i ) {
				Assert.IsTrue( expression[i] != '(' || expression[i + 1] == ' ', errorMessage );
				Assert.IsTrue( expression[i] != ')' || expression[i - 1] == ' ', errorMessage );
				Assert.IsTrue( expression[i] != '!' || expression[i + 1] == ' ', errorMessage );
				Assert.IsTrue( !( IsOperator( expression[i].ToString() ) && expression[i] != '!' ) || expression[i - 1] == ' ' && expression[i + 1] == ' ', errorMessage );
			}
		}
	}
}