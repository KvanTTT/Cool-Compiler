using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CoolCompiler
{
	public static class CoolTokens
	{
		public static Dictionary<int, string> Dictionary;

		public static void Load(string fileName)
		{
			var Lines = File.ReadAllLines(fileName);
			Dictionary = new Dictionary<int, string>(Lines.Length);
			foreach (var line in Lines)
			{
				var parts = line.Split('=');
				if (!Dictionary.ContainsKey(int.Parse(parts[1])))
					Dictionary.Add(int.Parse(parts[1]), parts[0]);
			}
		}
	}
}
