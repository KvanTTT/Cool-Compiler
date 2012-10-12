using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class RewriteEarlyExitError : CompilerError
	{
		string Message;

		public RewriteEarlyExitError(string message, int number)
			: base(number, null, null, null)
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
				return enmCompilerErrorType.RewriteEarlyExitParser;
			}
		}
	}
}
