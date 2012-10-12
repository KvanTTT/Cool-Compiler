using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class UndefinedFunctionError : CompilerError
	{
		public string FunctionName;
		public string ClassName;

		public UndefinedFunctionError(string functionName, string className, 
			int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			FunctionName = functionName;
			ClassName = className;
		}

		public override string Description
		{
			get
			{
				return string.Format("Function '{0}' is not defined in class '{1}'", FunctionName, ClassName);
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
				return enmCompilerErrorType.UndefinedFunction;
			}
		}
	}
}
