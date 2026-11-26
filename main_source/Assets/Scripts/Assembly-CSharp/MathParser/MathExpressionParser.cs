using System.Collections.Generic;

namespace MathParser
{
	public static class MathExpressionParser
	{
		private enum TokenType
		{
			Number = 0,
			Operator = 1,
			Function = 2,
			LeftParen = 3,
			RightParen = 4,
			Comma = 5,
			End = 6
		}

		private struct Token
		{
			public TokenType Type;

			public string Value;

			public float Number;
		}

		public static bool Evaluate(string expression, out float result)
		{
			result = default;
			return false;
		}

		public static string SubstituteParameters(string formula, Dictionary<string, float> parameters)
		{
			return null;
		}

		private static List<Token> Tokenize(string expression)
		{
			return null;
		}

		private static bool IsOperator(char c)
		{
			return false;
		}

		private static float ParseExpression(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParseAddition(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParseMultiplication(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParsePower(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParseUnary(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParsePrimary(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float ParseFunction(List<Token> tokens, int startIndex, out int nextIndex)
		{
			nextIndex = default;
			return 0f;
		}

		private static float EvaluateFunction(string functionName, List<float> args)
		{
			return 0f;
		}
	}
}
