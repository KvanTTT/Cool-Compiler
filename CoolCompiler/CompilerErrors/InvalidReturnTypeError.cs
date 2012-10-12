using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CoolCompiler
{
	class InvalidReturnTypeError : CompilerError
	{
		public MethodBuilder Method;
		public Type RealType;

		public InvalidReturnTypeError(MethodBuilder method, Type type, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			Method = method;
			RealType = type;
		}

		public override string Description
		{
			get
			{
				return string.Format("The return type of function '{0}' must be '{1}', but '{2}' detected", Method.Name, Method.ReturnType.Name, RealType.Name);
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
				return enmCompilerErrorType.InvalidReturnType;
			}
		}
	}
}
