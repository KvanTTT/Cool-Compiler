using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolCompiler
{
	class FileInUseError : CompilerError
	{
		public string ProgramName;

		public FileInUseError(string programName, int number, int? line, int? columnStart, int? columnStop = null)
			: base(number, line, columnStart, columnStop)
		{
			ProgramName = programName;
		}

		public override string Description
		{
			get
			{
				return string.Format("Could not save '{0}.exe' because file is in use", ProgramName);
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
				return enmCompilerErrorType.FileInUse;
			}
		}
	}
}
