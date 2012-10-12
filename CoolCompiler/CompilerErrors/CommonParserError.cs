using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class CommonParserError : CompilerError
	{
		string Message;

		public CommonParserError(string message, int number, int? line, int? columnStart, int? columnStop = null)
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
				return enmCompilerErrorStage.Syntax;
			}
		}

		public override enmCompilerErrorType Type
		{
			get
			{
				return enmCompilerErrorType.CommonParser;
			}
		}
	}
}
