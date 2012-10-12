using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	class Token
	{
		public string Name
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		public int Line
		{
			get;
			set;
		}

		public int Column
		{
			get;
			set;
		}
	}
}
