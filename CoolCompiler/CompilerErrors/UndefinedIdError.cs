using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class UndefinedIdError : CompilerError
	{
		public string IdName;

		public UndefinedIdError(string idName, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			IdName = idName;
		}

		public override string Description
		{
			get
			{
				return string.Format("Local var, argument or field '{0}' could not be found", IdName);
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
				return enmCompilerErrorType.UndefinedId;
			}
		}
	}
}
