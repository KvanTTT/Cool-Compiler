using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class ArithmeticOperatorError : CompilerError
	{
		public Type Type1;
		public Type Type2;

		public ArithmeticOperatorError(Type type1, Type type2, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			Type1 = type1;
			Type2 = type2;
		}

		public override string Description
		{
			get
			{
				return string.Format("The arithmetic operators ('+','-','*','/') are allowed for only 'Int' types (not for '{0}' and '{1}')",
					Type1.Name, Type2.Name);
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
				return enmCompilerErrorType.ArithmeticOperator;
			}
		}
	}
}
