using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class UnexpectedTokenError : CompilerError
	{
		public int ExpectedTokenId;
		public int UnexpectedTokenId;

		public UnexpectedTokenError(int expectedTokenId, int unexpectedTokenId,
			int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			ExpectedTokenId = expectedTokenId;
			UnexpectedTokenId = unexpectedTokenId;
		}

		public override string Description
		{
			get
			{
				return string.Format("Expected token is '{0}', but found '{1}'",
					CoolGrammarParser.tokenNames[ExpectedTokenId], CoolGrammarParser.tokenNames[UnexpectedTokenId]);
			}
		}

		public override enmCompilerErrorStage Stage
		{
			get
			{
				return enmCompilerErrorStage.Syntax;
			}
		}

		public override enmCompilerErrorType Type
		{
			get
			{
				return enmCompilerErrorType.UnexpectedToken;
			}
		}
	}
}
