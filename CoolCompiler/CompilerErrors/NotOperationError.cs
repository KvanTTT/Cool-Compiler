using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class NotOperationError : CompilerError
	{
		public Type Type_;

		public NotOperationError(Type type, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			Type_ = type;
		}

		public override string Description
		{
			get
			{
				return string.Format("'not' operator is allowed only for 'Bool' not for '{0}'", Type_.Name);
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
				return enmCompilerErrorType.NotOperator;
			}
		}
	}
}
