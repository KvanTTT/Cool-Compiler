using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class IsVoidCheckingError : CompilerError
	{
		public IsVoidCheckingError(int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
		}

		public override string Description
		{
			get
			{
				return "Stirngs, Ints and Booleans can not be null";
			}
		}

		public override enmCompilerErrorStage Stage
		{
			get
			{
				return enmCompilerErrorStage.Semantic;
			}
		}

		public override enmCompilerErrorType Type
		{
			get
			{
				return enmCompilerErrorType.IsVoidChecking;
			}
		}
	}
}
