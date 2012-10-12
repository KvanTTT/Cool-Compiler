using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class FunctionArgumentsError : CompilerError
	{
		public readonly string FunctionName;
		public readonly List<Type> FunctionTypes;

		public FunctionArgumentsError(string functionName, List<Type> functionTypes, 
			int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			FunctionName = functionName;
			FunctionTypes = functionTypes;
		}

		public override string Description
		{
			get
			{
				var result = new StringBuilder("Wrong count or types of argumets in function ");
				result.Append(FunctionName + "(");
				for (int i = 1; i < FunctionTypes.Count; i++)
					result.Append(FunctionTypes[i].Name + ", ");
				result.Remove(result.Length - 2, 2);
				result.Append(")");
				return result.ToString();
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
				return enmCompilerErrorType.FunctionArguments;
			}
		}
	}
}
