﻿//Copyright (c) 2024 Tianyi Ma

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.


using GDWeave.Godot;
using static GDWeave.Godot.TokenType;

namespace util.LexicalTransformer;

using MultiTokenPattern = Func<Token, bool>[];

public static class TransformationPatternFactory
{
	/// <summary>
	/// Creates a new pattern array which matches `extends &lt;Identifier&gt;\n`. Useful for patching into the global
	/// scope.
	/// </summary>
	/// <returns></returns>
	public static MultiTokenPattern CreateGlobalsPattern()
	{
		return [t => t.Type is PrExtends, t => t.Type is Identifier, t => t.Type is Newline];
	}

	/// <summary>
	/// Creates a new pattern array which matches a function definition. This will not match the preceding or trailing
	/// Newline tokens.
	/// </summary>
	/// <param name="name">The name of the function to match.</param>
	/// <param name="args">
	/// An array of the names of arguments of the function to match. If null or empty, the returned checks will only
	/// match a function which does not accept arguments.
	/// </param>
	/// <returns></returns>
	public static MultiTokenPattern CreateFunctionDefinitionPattern(string name, string[]? args = null)
	{
		var checks = new List<Func<Token, bool>>();

		checks.Add(t => t.Type is PrFunction);
		checks.Add(t => t is IdentifierToken token && token.Name == name);
		checks.Add(t => t.Type is ParenthesisOpen);
		if (args is { Length: > 0 })
		{
			foreach (var arg in args)
			{
				checks.Add(t => t is IdentifierToken token && token.Name == arg);
				checks.Add(t => t.Type is Comma);
			}

			checks.RemoveAt(checks.Count - 1);
		}

		checks.Add(t => t.Type is ParenthesisClose);
		checks.Add(t => t.Type is Colon);

		return checks.ToArray();
	}

	/// <summary>
	/// Creates a new pattern array which matches the provided GDScript snippet.
	/// </summary>
	/// <param name="snippet">A GDScript snippet to match.</param>
	/// <param name="indent">The base indent of the snippet.</param>
	/// <returns></returns>
	public static MultiTokenPattern CreateGdSnippetPattern(string snippet, uint indent = 0)
	{
		var tokens = ScriptTokenizer.Tokenize(snippet, indent);

		return tokens
			.Select(snippetToken =>
				(Func<Token, bool>)(
					t =>
					{
						if (t.Type == Identifier)
						{
							return snippetToken is IdentifierToken snippetIdentifier
								&& t is IdentifierToken identifier
								&& snippetIdentifier.Name == identifier.Name;
						}

						if (t.Type == Constant)
						{
							return snippetToken is ConstantToken snippetConstant
								&& t is ConstantToken constant
								&& snippetConstant.Value.Equals(constant.Value);
						}

						return t.Type == snippetToken.Type;
					}
				)
			)
			.ToArray();
	}
}
