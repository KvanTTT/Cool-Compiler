using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CoolCompiler
{
	public abstract class ObjectDef
	{
		protected static ILGenerator Generator_;

		public bool IsUsed
		{
			get;
			set;
		}

		public Type Type
		{
			get;
			protected set;
		}

		public abstract enmObjectScope Scope
		{
			get;
		}

		public ObjectDef(Type type)
		{
			Type = type;
			IsUsed = true;
		}

		public abstract void Load();

		public abstract void Remove();

		public abstract void Free();

		protected static void EmitSaveToLocal(int localVarNumber)
		{
			switch (localVarNumber)
			{
				case 0:
					Generator_.Emit(OpCodes.Stloc_0);
					break;
				case 1:
					Generator_.Emit(OpCodes.Stloc_1);
					break;
				case 2:
					Generator_.Emit(OpCodes.Stloc_2);
					break;
				case 3:
					Generator_.Emit(OpCodes.Stloc_3);
					break;
				default:
					if (localVarNumber < 256)
						Generator_.Emit(OpCodes.Stloc_S, localVarNumber);
					else
						Generator_.Emit(OpCodes.Stloc, localVarNumber);
					break;
			}
		}

		protected static void EmitLoadFromLocal(int localVarNumber)
		{
			switch (localVarNumber)
			{
				case 0:
					Generator_.Emit(OpCodes.Ldloc_0);
					break;
				case 1:
					Generator_.Emit(OpCodes.Ldloc_1);
					break;
				case 2:
					Generator_.Emit(OpCodes.Ldloc_2);
					break;
				case 3:
					Generator_.Emit(OpCodes.Ldloc_3);
					break;
				default:
					if (localVarNumber < 256)
						Generator_.Emit(OpCodes.Ldloc_S, localVarNumber);
					else
						Generator_.Emit(OpCodes.Ldloc, localVarNumber);
					break;
			}
		}

		protected static void EmitInteger(int value)
		{
			switch (value)
			{
				case -1:
					Generator_.Emit(OpCodes.Ldc_I4_M1);
					break;
				case 0:
					Generator_.Emit(OpCodes.Ldc_I4_0);
					break;
				case 1:
					Generator_.Emit(OpCodes.Ldc_I4_1);
					break;
				case 2:
					Generator_.Emit(OpCodes.Ldc_I4_2);
					break;
				case 3:
					Generator_.Emit(OpCodes.Ldc_I4_3);
					break;
				case 4:
					Generator_.Emit(OpCodes.Ldc_I4_4);
					break;
				case 5:
					Generator_.Emit(OpCodes.Ldc_I4_5);
					break;
				case 6:
					Generator_.Emit(OpCodes.Ldc_I4_6);
					break;
				case 7:
					Generator_.Emit(OpCodes.Ldc_I4_7);
					break;
				case 8:
					Generator_.Emit(OpCodes.Ldc_I4_8);
					break;
				default:
					if (value >= -128 && value <= 127)
						Generator_.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
					else
						Generator_.Emit(OpCodes.Ldc_I4, value);
					break;
			}
		}
	}
}
