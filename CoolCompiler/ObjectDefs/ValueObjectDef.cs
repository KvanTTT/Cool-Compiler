using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CoolCompiler
{
	public class ValueObjectDef : ObjectDef
	{
		object Value;
		ConstructorBuilder Builder;

		public ValueObjectDef(Type type, object value, ConstructorBuilder builder = null)
			: base(type)
		{
			Value = value;
			Builder = builder;
		}

		public override enmObjectScope Scope
		{
			get
			{
				return enmObjectScope.Value;
			}
		}

		public override void Load()
		{
			if (Type == typeof(int))
				EmitInteger((int)Value);
			else if (Type == typeof(bool))
			{
				var boolean = (bool)Value;
				if (boolean)
					Generator_.Emit(OpCodes.Ldc_I4_1);
				else
					Generator_.Emit(OpCodes.Ldc_I4_0);
			}
			else if (Type == typeof(string))
				Generator_.Emit(OpCodes.Ldstr, (string)Value);
			else
				if (Builder == null)
					Generator_.Emit(OpCodes.Ldnull);
				else
					Generator_.Emit(OpCodes.Newobj, Builder);
		}


		public override void Remove()
		{
		}

		public override void Free()
		{
		}
	}
}
