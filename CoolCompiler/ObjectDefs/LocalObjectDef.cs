using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CoolCompiler
{
	public class LocalObjectDef : ObjectDef
	{
		protected static List<LocalObjectDef> Locals_;

		public int Number;
		public string Name;

		protected LocalObjectDef(Type type, int number, string name = "")
			: base(type)
		{
			Name = name;
			Number = number;
		}

		public override enmObjectScope Scope
		{
			get
			{
				return enmObjectScope.Local;
			}
		}

		protected List<LocalObjectDef> DuplicatedLocals = new List<LocalObjectDef>();

		public static LocalObjectDef AllocateLocal(Type type, string name = "")
		{
			List<LocalObjectDef> duplicatedLocals = new List<LocalObjectDef>();
			int number = 0;
			int i;
			for (i = 0; i < Locals_.Count; i++)
				if (Locals_[i].Scope == enmObjectScope.Local && (Locals_[i] as LocalObjectDef).Name == name && name != "")
				{
					duplicatedLocals.Add(Locals_[i] as LocalObjectDef);
					Locals_[i].IsUsed = false;
				}

			for (i = 0; i < Locals_.Count; i++)
				if (Locals_[i].Type.Name == type.Name && !Locals_[i].IsUsed)
				{
					number = i;
					Locals_[i] = new LocalObjectDef(type, number, name);
					break;
				}
			if (i == Locals_.Count)
			{
				var localVar = Generator_.DeclareLocal(type);
				number = localVar.LocalIndex;
				Locals_.Add(new LocalObjectDef(type, number, name));
			}
			EmitSaveToLocal(number);
			return Locals_[number];
		}

		public override void Load()
		{
			EmitLoadFromLocal((int)Number);
		}

		public override void Remove()
		{
			if (Name == "")
				IsUsed = false;
		}

		public override void Free()
		{
			for (int i = 0; i < DuplicatedLocals.Count; i++)
			{
				Locals_[DuplicatedLocals[i].Number] = DuplicatedLocals[i];
				Locals_[DuplicatedLocals[i].Number].IsUsed = true;
			}
			IsUsed = false;
		}

		public static void InitGenerator(ILGenerator generator)
		{
			Generator_ = generator;
			Locals_ = new List<LocalObjectDef>();
		}

		public static LocalObjectDef GetLocalObjectDef(string Name)
		{
			for (int i = 0; i < Locals_.Count; i++)
				if (Locals_[i].IsUsed && (Locals_[i] as LocalObjectDef).Name == Name)
					return Locals_[i];
			return null;
		}
	}
}
