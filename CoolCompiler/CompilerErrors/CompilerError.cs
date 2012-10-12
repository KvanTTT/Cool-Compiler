using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	public abstract class CompilerError
	{
		public CompilerError(int number, int? line, int? columnStart, int? columnStop = null)
		{
			Number = number;
			Line = line;
			ColumnStart = columnStart;
			if (!columnStop.HasValue)
				ColumnStop = columnStart;
			else
				ColumnStop = columnStop;
		}

		public int Number
		{
			get;
			protected set;
		}

		public int? Line
		{
			get;
			protected set;
		}

		public int? ColumnStart
		{
			get;
			protected set;
		}

		public int? ColumnStop
		{
			get;
			protected set;
		}

		public abstract string Description
		{
			get;
		}

		public abstract enmCompilerErrorStage Stage
		{
			get;
		}

		public string StageString
		{
			get
			{
				return Stage.ToString();
			}
		}

		public abstract enmCompilerErrorType Type
		{
			get;
		}
	}
}
