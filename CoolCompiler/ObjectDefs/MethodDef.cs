using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CoolCompiler
{
	public class MethodDef
	{
		public string Name;
		public MethodBuilder MethodBuilder;
		public Dictionary<string, ArgObjectDef> Args;

		public MethodDef(string name, Dictionary<string, ArgObjectDef> args, MethodBuilder methodBuilder)
		{
			Name = name;
			Args = args;
			MethodBuilder = methodBuilder;
		}
	}
}
