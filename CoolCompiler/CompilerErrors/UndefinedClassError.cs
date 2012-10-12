using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class UndefinedClassError : CompilerError
	{
		public string ClassName;

		public UndefinedClassError(string className, 
			int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			ClassName = className;
		}

		public override string Description
		{
			get
			{
				return string.Format("Class '{0}' is undefined", ClassName);
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
				return enmCompilerErrorType.UndefinedClass;
			}
		}
	}
}
