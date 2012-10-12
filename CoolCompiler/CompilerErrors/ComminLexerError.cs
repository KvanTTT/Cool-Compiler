using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class CommonLexerError : CompilerError
	{
		string Message;

		public CommonLexerError(string message, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			Message = message;
		}

		public override string Description
		{
			get
			{
				return Message;
			}
		}

		public override enmCompilerErrorStage Stage
		{
			get
			{
				return enmCompilerErrorStage.Lexer;
			}
		}

		public override enmCompilerErrorType Type
		{
			get
			{
				return enmCompilerErrorType.CommonLexer;
			}
		}
	}
}
