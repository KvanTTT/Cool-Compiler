using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class EntryPointError : CompilerError
	{
		public EntryPointError(int number)
			: base(number, null, null)
		{
		}

		public override string Description
		{
			get
			{
				return "Missing entry point function (The function 'main' must be exists in class 'Main')";
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
				return enmCompilerErrorType.EntryPoint;
			}
		}
	}
}
