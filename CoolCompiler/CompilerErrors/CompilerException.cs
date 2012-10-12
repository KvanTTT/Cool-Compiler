using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class CompilerException : Exception
	{
		public int Line;
		public int Column;

		public CompilerException(string message, int line, int column)
		{
			//Message = message;
			Line = line;
			Column = column;
		}
	}
}
