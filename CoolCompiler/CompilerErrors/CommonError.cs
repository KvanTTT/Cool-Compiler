using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class CommonError : CompilerError
	{
		public string Message;

		public CommonError(string message, int number, int? Line = null, int? Column = null)
			: base(number, Line, Column)
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
				return enmCompilerErrorStage.Common;
			}
		}

		public override enmCompilerErrorType Type
		{
			get
			{
				return enmCompilerErrorType.Common;
			}
		}
	}
}
