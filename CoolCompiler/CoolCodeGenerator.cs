using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace CoolCompiler
{
	public class CoolCodeGenerator
	{
		#region Constants & Statics

		public static Type BoolType = typeof(bool);
		public static Type IntegerType = typeof(int);
		public static Type StringType = typeof(string);
		protected MethodInfo WriteStringLineMethod;
		protected MethodInfo WriteIntLineMethod;
		protected MethodInfo WriteStringMethod;
		protected MethodInfo WriteIntMethod;
		protected MethodInfo ReadMethod;
		protected MethodInfo IntParseMethod;

		#endregion

		#region Fields

		protected CommonTree Tree_;
		
		protected string ProgramName_;
		protected string FileName_;
		protected AssemblyName AssemblyName_;
		protected AssemblyBuilder AssemblyBuilder_;
		protected ModuleBuilder ModuleBuilder_;
		
		protected TypeBuilder CurrentTypeBuilder_;
		protected ILGenerator CurrentILGenerator_;

		protected Dictionary<string, TypeBuilder> ClassBuilders_;
		protected Dictionary<string, Dictionary<string, FieldObjectDef>> Fields_;
		protected Dictionary<string, Dictionary<string, MethodDef>> Functions_;
		protected Dictionary<string, ConstructorBuilder> Constructors_;

		protected Dictionary<string, ArgObjectDef> CurrentArgs_;

		public List<CompilerError> CompilerErrors;

		protected MethodBuilder EntryPoint_;

		#endregion

		#region Constructors

		public CoolCodeGenerator(string coolFileName, CommonTree tree)
		{
			WriteStringLineMethod = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { StringType }, null);
			WriteIntLineMethod = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { IntegerType }, null);
			WriteStringMethod = typeof(Console).GetMethod("Write", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { StringType }, null);
			WriteIntMethod = typeof(Console).GetMethod("Write", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { IntegerType }, null);
			ReadMethod = typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { }, null);
			IntParseMethod = IntegerType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
				new Type[] { StringType }, null);

			FileName_ = coolFileName;
			ProgramName_ = Path.GetFileNameWithoutExtension(FileName_);
			Tree_ = tree;
		}

		#endregion

		#region Public Methods
		
		public void Generate()
		{
			CompilerErrors = new List<CompilerError>();

			GenerateAssemblyAndModuleBuilders();

			DefineAllClasses(Tree_);

			DefineAllFieldsAndFunctions(Tree_);

			EmitProgram(Tree_);

			ClassBuilders_["IO"].CreateType();

			if (!ClassBuilders_.ContainsKey("Main") || !Functions_["Main"].ContainsKey("main"))
				CompilerErrors.Add(new EntryPointError(CompilerErrors.Count));

			bool fileSaveError = false;
			if (File.Exists(ProgramName_ + ".exe"))
			{
				try
				{
					using (Stream stream = new FileStream(ProgramName_ + ".exe", FileMode.Create)) {}
					File.Delete(ProgramName_ + ".exe");
				}
				catch
				{
					fileSaveError = true;
					CompilerErrors.Add(new FileInUseError(ProgramName_, CompilerErrors.Count, null, null));
				}
			}

			if (!fileSaveError && CompilerErrors.Count == 0)
				AssemblyBuilder_.Save(ProgramName_ + ".exe");
		}

		#endregion

		#region Initialization

		protected void DefineAllClasses(ITree rootNode)
		{
			Fields_ = new Dictionary<string, Dictionary<string, FieldObjectDef>>();
			Functions_ = new Dictionary<string, Dictionary<string, MethodDef>>();
			ClassBuilders_ = new Dictionary<string, TypeBuilder>();
			Constructors_ = new Dictionary<string, ConstructorBuilder>();
			for (int i = 0; i < rootNode.ChildCount; i++)
			{
				var classNode = rootNode.GetChild(i);

				Type parentType;
				if (classNode.ChildCount == 2)
					parentType = typeof(object);
				else
					parentType = ModuleBuilder_.GetType(classNode.GetChild(2).Text);

				string className = classNode.GetChild(0).Text;

				TypeBuilder classBuilder;
				if (className != "Main")
					classBuilder = ModuleBuilder_.DefineType(className, TypeAttributes.Public | TypeAttributes.BeforeFieldInit, parentType);
				else
					classBuilder = ModuleBuilder_.DefineType(className, TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit, parentType);

				ConstructorBuilder constructorBuilder;
				if (className != "Main")
					constructorBuilder = classBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, Type.EmptyTypes);
				else
				{
					classBuilder.DefineDefaultConstructor(MethodAttributes.Public);
					constructorBuilder = classBuilder.DefineConstructor(
						MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig, CallingConventions.Standard, Type.EmptyTypes);
				}
				Constructors_.Add(className, constructorBuilder);

				ClassBuilders_.Add(className, classBuilder);
			}
			DefineDefaultClasses();
		}

		protected void DefineDefaultClasses()
		{
			var functionList = new Dictionary<string, MethodDef>();

			ClassBuilders_.Add("IO", ModuleBuilder_.DefineType("IO", TypeAttributes.Public, typeof(object)));
			var classBuilder = ClassBuilders_["IO"];
			
			var outStringBuilder = classBuilder.DefineMethod("out_string", MethodAttributes.Public, CallingConventions.Standard, 
				classBuilder, new Type[] { StringType });
			var ilGenerator = outStringBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Call, WriteStringMethod);
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("out_string", new MethodDef("out_string",
				new Dictionary<string,ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }, 
					{ "x", new ArgObjectDef(StringType, 1, "x") }
				}, outStringBuilder));

			var outStringLineBuilder = classBuilder.DefineMethod("out_string_line", MethodAttributes.Public, CallingConventions.Standard,
				classBuilder, new Type[] { StringType });
			ilGenerator = outStringLineBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Call, WriteStringLineMethod);
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("out_string_line", new MethodDef("out_string_line",
				new Dictionary<string, ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }, 
					{ "x", new ArgObjectDef(StringType, 1, "x") }
				}, outStringLineBuilder));

			var outIntBuilder = classBuilder.DefineMethod("out_int", MethodAttributes.Public, CallingConventions.Standard,
				classBuilder, new Type[] { IntegerType });
			ilGenerator = outIntBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Call, WriteIntMethod);
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("out_int", new MethodDef("out_int",
				new Dictionary<string, ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }, 
					{ "x", new ArgObjectDef(IntegerType, 1, "x") }
				}, outIntBuilder));

			var outIneLineBuilder = classBuilder.DefineMethod("out_int_line", MethodAttributes.Public, CallingConventions.Standard,
				classBuilder, new Type[] { IntegerType });
			ilGenerator = outIneLineBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Call, WriteIntLineMethod);
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("out_int_line", new MethodDef("out_int_line",
				new Dictionary<string, ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }, 
					{ "x", new ArgObjectDef(IntegerType, 1, "x") }
				}, outIneLineBuilder));

			var inStringBuilder = classBuilder.DefineMethod("in_string", MethodAttributes.Public, CallingConventions.Standard,
				classBuilder, Type.EmptyTypes);
			ilGenerator = inStringBuilder.GetILGenerator();
			//ilGenerator.Emit(OpCodes.Nop);
			ilGenerator.Emit(OpCodes.Call, ReadMethod);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("in_string", new MethodDef("in_string",
				new Dictionary<string, ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }
				}, inStringBuilder));


			var inIntBuilder = classBuilder.DefineMethod("in_int", MethodAttributes.Public, CallingConventions.Standard,
				classBuilder, Type.EmptyTypes);
			ilGenerator = inIntBuilder.GetILGenerator();
			//ilGenerator.Emit(OpCodes.Nop);
			ilGenerator.Emit(OpCodes.Call, ReadMethod);
			ilGenerator.Emit(OpCodes.Call, IntParseMethod);
			ilGenerator.Emit(OpCodes.Ret);
			functionList.Add("in_int", new MethodDef("in_int",
				new Dictionary<string, ArgObjectDef>() 
				{
					{ "this", new ArgObjectDef(classBuilder, 0, "this") }
				}, inIntBuilder));

			var constructorBuilder = classBuilder.DefineDefaultConstructor(MethodAttributes.Public);
			Constructors_.Add("IO", constructorBuilder);

			Functions_.Add("IO", functionList);
		}

		protected void FinalAllClasses()
		{
			foreach (var classBuilder in ClassBuilders_)
				classBuilder.Value.CreateType();
		}

		protected void DefineAllFieldsAndFunctions(ITree rootNode)
		{
			for (int i = 0; i < rootNode.ChildCount; i++)
			{
				var className = rootNode.GetChild(i).GetChild(0).Text;
				var featureListNode = rootNode.GetChild(i).GetChild(1);
				var fieldList = new Dictionary<string, FieldObjectDef>();
				var functionList = new Dictionary<string, MethodDef>();
				CurrentTypeBuilder_ = ClassBuilders_[className];

				for (int j = 0; j < featureListNode.ChildCount; j++)
				{
					var child = featureListNode.GetChild(j);
					switch (child.Type)
					{
						case (int)CoolGrammarLexer.LocalOrFieldInit:
							DefineField(child, ClassBuilders_[className], fieldList);
							break;

						case (int)CoolGrammarLexer.FuncDef:
							DefineFunction(child, ClassBuilders_[className], functionList);
							break;
					}
				}

				var baseTypeName = ClassBuilders_[className].BaseType.Name;
				if (ClassBuilders_.ContainsKey(baseTypeName))
				{
					foreach (var field in Fields_[baseTypeName])
						if (!fieldList.ContainsKey(field.Value.Name))
							fieldList.Add(field.Value.Name, field.Value);

					foreach (var function in Functions_[baseTypeName])
						if (!functionList.ContainsKey(function.Value.Name))
							functionList.Add(function.Value.Name, function.Value);
				}
				Fields_.Add(className, fieldList);
				Functions_.Add(className, functionList);
			}
		}

		protected void DefineField(ITree treeNode, TypeBuilder classBuilder, Dictionary<string, FieldObjectDef> fieldList)
		{
			var fieldName = treeNode.GetChild(0).Text;
			var fieldType = GetType(treeNode.GetChild(1).Text);

			FieldBuilder fieldBuilder;
			if (CurrentTypeBuilder_.Name != "Main")
				fieldBuilder = classBuilder.DefineField(fieldName, fieldType, FieldAttributes.Family);
			else
				fieldBuilder = classBuilder.DefineField(fieldName, fieldType, FieldAttributes.Family | FieldAttributes.Static);

			fieldList.Add(fieldName, new FieldObjectDef(fieldType, fieldName, fieldBuilder));
		}

		protected void DefineFunction(ITree treeNode, TypeBuilder classBuilder, Dictionary<string, MethodDef> functionList)
		{
			string functionName = treeNode.GetChild(0).Text;
			var functionReturnType = GetType(treeNode.GetChild(1).Text);

			Type[] inputTypes;
			var args = new Dictionary<string, ArgObjectDef>();
			args.Add("this", new ArgObjectDef(CurrentTypeBuilder_, 0, "this"));
			if (treeNode.ChildCount == 4)
			{
				var functionListNode = treeNode.GetChild(3);
				inputTypes = new Type[functionListNode.ChildCount];
				for (int k = 0; k < functionListNode.ChildCount; k++)
				{ 
					inputTypes[k] = GetType(functionListNode.GetChild(k).GetChild(1).Text);
					var argName = functionListNode.GetChild(k).GetChild(0).Text;
					args.Add(argName, new ArgObjectDef(inputTypes[k], k + 1, argName));
				}
			}
			else
				inputTypes = Type.EmptyTypes;

			MethodBuilder methodBuilder;

			// Define entry point.
			if (functionName == "main" && classBuilder.Name == "Main")
			{
				methodBuilder = classBuilder.DefineMethod(functionName,
					MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, typeof(void), Type.EmptyTypes);
				methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(
					typeof(STAThreadAttribute).GetConstructor(Type.EmptyTypes), new object[] { }));
				AssemblyBuilder_.SetEntryPoint(methodBuilder);
				EntryPoint_ = methodBuilder;
			}
			else
				methodBuilder = classBuilder.DefineMethod(functionName, MethodAttributes.Public | MethodAttributes.HideBySig, functionReturnType, inputTypes);


			functionList.Add(functionName, new MethodDef(functionName, args, methodBuilder));
		}

		#endregion

		#region Emit Classes, Fields, Functions

		protected void EmitProgram(ITree rootNode)
		{
			EmitClasses(rootNode);
		}

		protected void EmitClasses(ITree treeNode)
		{
			for (int i = 0; i < treeNode.ChildCount; i++)
			{
				if (treeNode.GetChild(i).Type == CoolGrammarLexer.Class)
					EmitClass(treeNode.GetChild(i));
				else
					CompilerErrors.Add(new UnexpectedTokenError(CoolGrammarLexer.Class, treeNode.Type, CompilerErrors.Count,
						treeNode.GetChild(i).Line, treeNode.GetChild(i).CharPositionInLine));
			}
		}

		protected void EmitClass(ITree treeNode)
		{
			CurrentTypeBuilder_ = ClassBuilders_[treeNode.GetChild(0).Text];
			
			var parentType = treeNode.ChildCount == 3 ? ModuleBuilder_.GetType(treeNode.GetChild(2).Text) : typeof(object);
			var constructorInfo = parentType.GetConstructor(Type.EmptyTypes);

			EmitFieldsAndFunction(treeNode.GetChild(1));

			CurrentILGenerator_ = Constructors_[treeNode.GetChild(0).Text].GetILGenerator();
			if (!Constructors_[treeNode.GetChild(0).Text].IsStatic)
			{
				CurrentILGenerator_.Emit(OpCodes.Ldarg_0);
				CurrentILGenerator_.Emit(OpCodes.Call, constructorInfo); // call parent constructor;
			}
			CurrentILGenerator_.Emit(OpCodes.Ret);

			CurrentTypeBuilder_.CreateType();
		}

		protected void EmitFieldsAndFunction(ITree fieldsAndFunctionsNode)
		{
			for (int i = 0; i < fieldsAndFunctionsNode.ChildCount; i++)
			{
				var child = fieldsAndFunctionsNode.GetChild(i);
				switch (child.Type)
				{
					case CoolGrammarLexer.LocalOrFieldInit:
						EmitField(fieldsAndFunctionsNode.GetChild(i));
						break;
				}
			}
			for (int i = 0; i < fieldsAndFunctionsNode.ChildCount; i++)
			{
				var child = fieldsAndFunctionsNode.GetChild(i);
				switch (child.Type)
				{
					case CoolGrammarLexer.FuncDef:
						EmitFunction(fieldsAndFunctionsNode.GetChild(i));
						break;
				}
			}
		}

		protected void EmitField(ITree fieldNode)
		{
			CurrentILGenerator_ = Constructors_[CurrentTypeBuilder_.Name].GetILGenerator();
			LocalObjectDef.InitGenerator(CurrentILGenerator_);
			ObjectDef returnObjectDef;
			if (fieldNode.ChildCount == 3)
				returnObjectDef = EmitExpression(fieldNode.GetChild(2));
			else
				returnObjectDef = EmitDefaultValue(GetType(fieldNode.GetChild(1).Text));

			if (!Constructors_[CurrentTypeBuilder_.Name].IsStatic)
			{
				CurrentILGenerator_.Emit(OpCodes.Ldarg_0);
				returnObjectDef.Load();
				CurrentILGenerator_.Emit(OpCodes.Stfld, Fields_[CurrentTypeBuilder_.Name][fieldNode.GetChild(0).Text].FieldInfo);
			}
			else
			{
				returnObjectDef.Load();
				CurrentILGenerator_.Emit(OpCodes.Stsfld, Fields_[CurrentTypeBuilder_.Name][fieldNode.GetChild(0).Text].FieldInfo);
			}
			returnObjectDef.Remove();
		}

		protected void EmitFunction(ITree functionNode)
		{
			string functionName = functionNode.GetChild(0).Text;
			Type functionReturnType = GetType(functionNode.GetChild(1).Text);

			CurrentArgs_ = Functions_[CurrentTypeBuilder_.Name][functionName].Args;
			CurrentILGenerator_ = Functions_[CurrentTypeBuilder_.Name][functionName].MethodBuilder.GetILGenerator();
			LocalObjectDef.InitGenerator(CurrentILGenerator_);

			var returnObjectDef = EmitExpression(functionNode.GetChild(2));
			returnObjectDef.Load();

			if (!functionReturnType.IsAssignableFrom(returnObjectDef.Type))
			//if (!returnObjectDef.Type.IsAssignableFrom(functionReturnType))
				CompilerErrors.Add(new InvalidReturnTypeError(
					Functions_[CurrentTypeBuilder_.Name][functionName].MethodBuilder,
					returnObjectDef.Type,
					CompilerErrors.Count, functionNode.GetChild(2).Line, functionNode.GetChild(2).CharPositionInLine));
			
			if (Functions_[CurrentTypeBuilder_.Name][functionName].MethodBuilder.ReturnType == typeof(void))
				CurrentILGenerator_.Emit(OpCodes.Pop);
			CurrentILGenerator_.Emit(OpCodes.Ret);

			returnObjectDef.Remove();
		}

		#endregion

		#region Expression Evaluate

		protected ObjectDef EmitExpression(ITree expressionNode)
		{
			ObjectDef result;
			switch (expressionNode.Type)
			{
				case CoolGrammarLexer.ASSIGN:
					result = EmitAssignOperation(expressionNode);
					break;
				case CoolGrammarLexer.NOT:
					result = EmitNotOperation(expressionNode);
					break;
				case CoolGrammarLexer.LE:
				case CoolGrammarLexer.LT:
				case CoolGrammarLexer.GE:
				case CoolGrammarLexer.GT:
					result = EmitComparsionOperation(expressionNode);
					break;
				case CoolGrammarLexer.EQUAL:
					result = EmitEqualOperation(expressionNode);
					break;
				case CoolGrammarLexer.PLUS:
				case CoolGrammarLexer.MINUS:
				case CoolGrammarLexer.MULT:
				case CoolGrammarLexer.DIV:
					result = EmitArithmeticOperation(expressionNode);
					break;
				case CoolGrammarLexer.ISVOID:
					result = EmitIsVoidChecking(expressionNode);
					break;
				case CoolGrammarLexer.NEG:
					result = EmitNegOperation(expressionNode);
					break;
				case CoolGrammarLexer.Term:
					if (expressionNode.ChildCount == 1)
						result = EmitExpression(expressionNode.GetChild(0));
					else
						result = EmitExplicitInvoke(expressionNode);
					break;
				case CoolGrammarLexer.ImplicitInvoke:
					result = EmitInplicitInvoke(expressionNode);
					break;
				case CoolGrammarLexer.IF:
					result = EmitIfBranch(expressionNode);
					break;
				case CoolGrammarLexer.WHILE:
					result = EmitWhileLoop(expressionNode);
					break;
				case CoolGrammarLexer.Exprs:
					for (int i = 0; i < expressionNode.ChildCount - 1; i++)
					{
						var objectDef = EmitExpression(expressionNode.GetChild(i));
						objectDef.Remove();
					}
					result = EmitExpression(expressionNode.GetChild(expressionNode.ChildCount - 1));
					break;
				case CoolGrammarLexer.LET:
					result = EmitLetOperation(expressionNode);
					break;
				case CoolGrammarLexer.CASE:
					result = EmitCaseOperation(expressionNode);
					break;
				case CoolGrammarLexer.Expr:
					result = EmitExpression(expressionNode.GetChild(0));
					break;				
				case CoolGrammarLexer.NEW:
					result = EmitObjectCreation(expressionNode);
					break;
				case CoolGrammarLexer.ID:
					result = EmitIdValue(expressionNode);
					break;
				case CoolGrammarLexer.INTEGER:
					result = EmitInteger(expressionNode);
					break;
				case CoolGrammarLexer.STRING:
					result = EmitString(expressionNode);
					break;
				case CoolGrammarLexer.TRUE:
					result = EmitBoolean(expressionNode);
					break;
				case CoolGrammarLexer.FALSE:
					result = EmitBoolean(expressionNode);
					break;
				case CoolGrammarLexer.VOID:
					result = EmitVoid(expressionNode);
					break;
				case CoolGrammarLexer.SELF:
					result = EmitSelf(expressionNode);
					break;
				default:
					result = null;
					throw new Exception();
					break;
			}
			return result;
		}

		protected ObjectDef EmitAssignOperation(ITree expressionNode)
		{
			var returnObjectDef = EmitExpression(expressionNode.GetChild(1));
			var Id = expressionNode.GetChild(0).Text;

			// Пока только для полей класса.
			// Сделать проверку типов.

			//if (CurrentArgs_.ContainsKey(CurrentTypeBuilder_.Name))

			if (!Fields_[CurrentTypeBuilder_.Name][Id].FieldInfo.IsStatic)
			{
				CurrentILGenerator_.Emit(OpCodes.Ldarg_0);
				returnObjectDef.Load();
				CurrentILGenerator_.Emit(OpCodes.Stfld, Fields_[CurrentTypeBuilder_.Name][Id].FieldInfo);
			}
			else
			{
				returnObjectDef.Load();
				CurrentILGenerator_.Emit(OpCodes.Stsfld, Fields_[CurrentTypeBuilder_.Name][Id].FieldInfo);
			}

			return returnObjectDef;
		}

		#region Arithmetic operations

		protected ObjectDef EmitNotOperation(ITree expressionNode)
		{
			var returnObject = EmitExpression(expressionNode.GetChild(0));

			if (returnObject.Type != BoolType)
				CompilerErrors.Add(new NotOperationError(returnObject.Type, CompilerErrors.Count,
						expressionNode.GetChild(0).Line, expressionNode.GetChild(0).CharPositionInLine));
			else
			{
				returnObject.Load();
				CurrentILGenerator_.Emit(OpCodes.Ldc_I4_0);
				CurrentILGenerator_.Emit(OpCodes.Ceq);
				returnObject.Remove();
			}

			return LocalObjectDef.AllocateLocal(BoolType);
		}

		protected ObjectDef EmitComparsionOperation(ITree expressionNode)
		{
			var returnObject2 = EmitExpression(expressionNode.GetChild(1));
			var returnObject1 = EmitExpression(expressionNode.GetChild(0));

			if (returnObject1.Type != IntegerType || returnObject1.Type != returnObject2.Type)
				CompilerErrors.Add(new ComparsionOperatorError(returnObject1.Type, returnObject2.Type,
					CompilerErrors.Count, expressionNode.Line, expressionNode.GetChild(0).CharPositionInLine,
					expressionNode.GetChild(1).CharPositionInLine));
			else
			{
				returnObject1.Load();
				returnObject2.Load();

				if (expressionNode.Type == CoolGrammarLexer.LT)
					CurrentILGenerator_.Emit(OpCodes.Clt);
				else if (expressionNode.Type == CoolGrammarLexer.LE)
				{
					CurrentILGenerator_.Emit(OpCodes.Cgt);
					CurrentILGenerator_.Emit(OpCodes.Ldc_I4_0);
					CurrentILGenerator_.Emit(OpCodes.Ceq);
				}
				else if (expressionNode.Type == CoolGrammarLexer.GT)
					CurrentILGenerator_.Emit(OpCodes.Cgt);
				else if (expressionNode.Type == CoolGrammarLexer.GE)
				{
					CurrentILGenerator_.Emit(OpCodes.Clt);
					CurrentILGenerator_.Emit(OpCodes.Ldc_I4_0);
					CurrentILGenerator_.Emit(OpCodes.Ceq);
				}

				returnObject1.Remove();
				returnObject2.Remove();
			}

			return LocalObjectDef.AllocateLocal(BoolType);
		}

		protected ObjectDef EmitEqualOperation(ITree expressionNode)
		{
			var returnObject1 = EmitExpression(expressionNode.GetChild(0));
			var returnObject2 = EmitExpression(expressionNode.GetChild(1));

			if (returnObject1.Type != IntegerType || returnObject1.Type != returnObject2.Type)
				CompilerErrors.Add(new ComparsionOperatorError(returnObject1.Type, returnObject2.Type,
					CompilerErrors.Count, expressionNode.Line, expressionNode.GetChild(0).CharPositionInLine,
					expressionNode.GetChild(1).CharPositionInLine));
			else
			{
				returnObject1.Load();
				returnObject2.Load();

				CurrentILGenerator_.Emit(OpCodes.Ceq);

				returnObject1.Remove();
				returnObject2.Remove();
			}

			return LocalObjectDef.AllocateLocal(BoolType);
		}

		protected ObjectDef EmitArithmeticOperation(ITree expressionNode)
		{
			var returnObject1 = EmitExpression(expressionNode.GetChild(0));
			var returnObject2 = EmitExpression(expressionNode.GetChild(1));

			if (returnObject1.Type != IntegerType || returnObject1.Type != returnObject2.Type)
				CompilerErrors.Add(new ArithmeticOperatorError(
					returnObject1.Type, returnObject2.Type,
					CompilerErrors.Count, expressionNode.Line, 
					expressionNode.GetChild(1).CharPositionInLine,
					expressionNode.GetChild(0).CharPositionInLine));
			else
			{
				returnObject1.Load();
				returnObject2.Load();
				
				if (expressionNode.Type == CoolGrammarLexer.PLUS)
					CurrentILGenerator_.Emit(OpCodes.Add);
				else if (expressionNode.Type == CoolGrammarLexer.MINUS)
					CurrentILGenerator_.Emit(OpCodes.Sub);
				else if (expressionNode.Type == CoolGrammarLexer.MULT)
					CurrentILGenerator_.Emit(OpCodes.Mul);
				else if (expressionNode.Type == CoolGrammarLexer.DIV)
					CurrentILGenerator_.Emit(OpCodes.Div);

				returnObject1.Remove();
				returnObject2.Remove();
			}

			return LocalObjectDef.AllocateLocal(IntegerType);
		}

		protected ObjectDef EmitIsVoidChecking(ITree expressionNode)
		{
			var returnObject = EmitExpression(expressionNode.GetChild(0));

			if (IsStaticType(returnObject.Type))
				CompilerErrors.Add(new IsVoidCheckingError(CompilerErrors.Count,
					expressionNode.GetChild(0).Line, expressionNode.GetChild(0).CharPositionInLine));
			else
			{
				returnObject.Load();
				CurrentILGenerator_.Emit(OpCodes.Ldnull);
				CurrentILGenerator_.Emit(OpCodes.Ceq);
				returnObject.Remove();
			}

			return LocalObjectDef.AllocateLocal(BoolType);
		}

		protected ObjectDef EmitNegOperation(ITree expressionNode)
		{
			var returnObject = EmitExpression(expressionNode.GetChild(0));

			if (IsStaticType(returnObject.Type))
				CompilerErrors.Add(new NegOperationError(returnObject.Type, CompilerErrors.Count,
					expressionNode.GetChild(0).Line, expressionNode.GetChild(0).CharPositionInLine));
			else
			{
				returnObject.Load();
				CurrentILGenerator_.Emit(OpCodes.Not);
				returnObject.Load();
			}

			return LocalObjectDef.AllocateLocal(IntegerType);
		}

		#endregion

		protected ObjectDef EmitExplicitInvoke(ITree expressionNode)
		{
			var result = EmitInvoke(expressionNode.GetChild(0),
				GetChild(expressionNode, CoolGrammarLexer.ATSIGN),
				expressionNode.GetChild(1),
				GetChild(expressionNode, CoolGrammarLexer.InvokeExprs));

			return result;
		}

		protected ObjectDef EmitInplicitInvoke(ITree expressionNode)
		{
			var result = EmitInvoke(null, null, expressionNode.GetChild(0), expressionNode.GetChild(1));

			return result;
		}

		protected ObjectDef EmitInvoke(ITree invokeExpressionNode, ITree atSignNode,
			ITree functionNode, ITree argsExpressionNode)
		{
			var functionName = functionNode.Text;
			ObjectDef invokeObjectDef;
			MethodBuilder methodBuilder;
			string invokeObjectName;
			if (invokeExpressionNode != null)
			{
				invokeObjectDef = EmitExpression(invokeExpressionNode);
				invokeObjectName = invokeObjectDef.Type.Name;
			}
			else
			{
				invokeObjectDef = new ArgObjectDef(CurrentTypeBuilder_, 0, "this");
				invokeObjectName = CurrentTypeBuilder_.Name;
			}

			ObjectDef result;
			if (!Functions_.ContainsKey(invokeObjectName) || !Functions_[invokeObjectName].ContainsKey(functionName))
			{
				CompilerErrors.Add(new UndefinedFunctionError(functionName, invokeObjectName,
					CompilerErrors.Count, functionNode.Line, functionNode.CharPositionInLine));
				result = LocalObjectDef.AllocateLocal(typeof(object));
			}
			else
			{
				methodBuilder = Functions_[invokeObjectName][functionName].MethodBuilder;
				var args = new List<ObjectDef>();
				args.Add(invokeObjectDef);

				if (argsExpressionNode != null)
				{
					for (int i = 0; i < argsExpressionNode.ChildCount; i++)
					{
						var objectDef = EmitExpression(argsExpressionNode.GetChild(i));
						args.Add(objectDef);
					}

					List<ArgObjectDef> args2 = new List<ArgObjectDef>();
					foreach (var arg in Functions_[invokeObjectDef.Type.Name][functionName].Args)
						args2.Add(arg.Value);
					if (!CheckTypes(args, args2))
					{
						var args2types = new List<Type>();
						foreach (var arg in args2)
							args2types.Add(arg.Type);
						CompilerErrors.Add(new FunctionArgumentsError(functionName, args2types,
							CompilerErrors.Count, functionNode.Line, functionNode.CharPositionInLine));
					}
				}

				foreach (var arg in args)
					arg.Load();

				if (invokeExpressionNode == null)
					CurrentILGenerator_.Emit(OpCodes.Call, methodBuilder);
				else
					CurrentILGenerator_.Emit(OpCodes.Callvirt, methodBuilder);

				foreach (var arg in args)
					arg.Remove();
				invokeObjectDef.Remove();

				if (functionNode.Parent.Parent.Type == CoolGrammarLexer.Exprs
						&& functionNode.Parent.ChildIndex != functionNode.Parent.Parent.ChildCount - 1)
				{
					CurrentILGenerator_.Emit(OpCodes.Pop);
					result = new ValueObjectDef(Functions_[invokeObjectDef.Type.Name][functionName].MethodBuilder.ReturnType, null);
				}
				else
					result = LocalObjectDef.AllocateLocal(Functions_[invokeObjectDef.Type.Name][functionName].MethodBuilder.ReturnType);
			}

			return result;
		}

		protected ObjectDef EmitIfBranch(ITree expressionNode)
		{
			// Возможна оптимизация (Brtrue, Brfalse_S).
			var checkObjectDef = EmitExpression(expressionNode.GetChild(0));
			checkObjectDef.Load();
			checkObjectDef.Remove();

			if (checkObjectDef.Type != BoolType)
				CompilerErrors.Add(new IfOperatorError(checkObjectDef.Type, CompilerErrors.Count,
					expressionNode.GetChild(0).Line, expressionNode.GetChild(0).CharPositionInLine));

			var exitLabel = CurrentILGenerator_.DefineLabel();
			var elseLabel = CurrentILGenerator_.DefineLabel();
			CurrentILGenerator_.Emit(OpCodes.Brfalse, elseLabel);

			var ifObjectDef = EmitExpression(expressionNode.GetChild(1));
			ifObjectDef.Load();
			ifObjectDef.Remove();
			CurrentILGenerator_.Emit(OpCodes.Br, exitLabel);

			CurrentILGenerator_.MarkLabel(elseLabel);
			var elseObjectDef = EmitExpression(expressionNode.GetChild(2));
			elseObjectDef.Load();
			elseObjectDef.Remove();

			CurrentILGenerator_.MarkLabel(exitLabel);

			return LocalObjectDef.AllocateLocal(GetMostNearestAncestor(ifObjectDef.Type, elseObjectDef.Type));
		}

		protected ObjectDef EmitWhileLoop(ITree expressionNode)
		{
			var checkLabel = CurrentILGenerator_.DefineLabel();
			CurrentILGenerator_.MarkLabel(checkLabel);

			var checkObjectDef = EmitExpression(expressionNode.GetChild(0));
			checkObjectDef.Load();

			if (checkObjectDef.Type != BoolType)
				CompilerErrors.Add(new WhileOperatorError(checkObjectDef.Type, CompilerErrors.Count,
					expressionNode.GetChild(0).Line, expressionNode.GetChild(0).CharPositionInLine));

			var exitLabel = CurrentILGenerator_.DefineLabel();
			CurrentILGenerator_.Emit(OpCodes.Brfalse, exitLabel);

			var bodyObjectDef = EmitExpression(expressionNode.GetChild(1));
			bodyObjectDef.Load();
			CurrentILGenerator_.Emit(OpCodes.Pop);
			CurrentILGenerator_.Emit(OpCodes.Br, checkLabel);
			
			CurrentILGenerator_.MarkLabel(exitLabel);

			checkObjectDef.Remove();
			bodyObjectDef.Remove();

			return new ValueObjectDef(typeof(object), null);
		}

		protected ObjectDef EmitLetOperation(ITree expressionNode)
		{
			List<ObjectDef> localObjectDefs = new List<ObjectDef>();
			for (int i = 0; i < expressionNode.GetChild(0).ChildCount; i++)
			{
				var localOrFieldInitNode = expressionNode.GetChild(0).GetChild(i);
				var type = GetType(localOrFieldInitNode.GetChild(1).Text);

				ObjectDef localReturnObjectDef; 
				if (localOrFieldInitNode.ChildCount == 3)
					localReturnObjectDef = EmitExpression(localOrFieldInitNode.GetChild(2));
				else
					localReturnObjectDef = EmitDefaultValue(type);
				localReturnObjectDef.Load();
				//localReturnObjectDef.Remove();
				var localObjectDef = LocalObjectDef.AllocateLocal(type, localOrFieldInitNode.GetChild(0).Text);
				localObjectDefs.Add(localObjectDef);
			}

			var returnObjectDef = EmitExpression(expressionNode.GetChild(1));

			foreach (var localObjectDef in localObjectDefs)
				localObjectDef.Free();

			return returnObjectDef;
		}

		protected ObjectDef EmitCaseOperation(ITree expressionNode)
		{
			return null;
		}

		protected ObjectDef EmitIdValue(ITree expressionNode)
		{
			var idValue = expressionNode.Text;
			LocalObjectDef localObjectDef;
			if ((localObjectDef = LocalObjectDef.GetLocalObjectDef(idValue)) != null)
				return localObjectDef;
			else if (CurrentArgs_.ContainsKey(idValue))
				return CurrentArgs_[idValue];
			else if (Fields_[CurrentTypeBuilder_.Name].ContainsKey(idValue))
				return Fields_[CurrentTypeBuilder_.Name][idValue];
			else
			{
				CompilerErrors.Add(new UndefinedIdError(idValue, CompilerErrors.Count,
					expressionNode.Line, expressionNode.CharPositionInLine));
				return new ValueObjectDef(typeof(object), null);
			}
		}

		#endregion

		#region Emitting simple types

		protected ObjectDef EmitObjectCreation(ITree expressionNode)
		{
			ObjectDef result;

			var objectName = expressionNode.GetChild(0).Text;
			if (!ClassBuilders_.ContainsKey(objectName))
			{
				CompilerErrors.Add(new UndefinedClassError(objectName, CompilerErrors.Count, expressionNode.Line,
					expressionNode.CharPositionInLine));
				result = new ValueObjectDef(typeof(object), null);
			}
			else
			{
				var returnType = ClassBuilders_[objectName];

				//CurrentILGenerator_.Emit(OpCodes.Newobj, Constructors_[objectName]);

				result = new ValueObjectDef(returnType, null, Constructors_[objectName]);
			}
			
			return result;
		}

		protected ObjectDef EmitInteger(ITree expressionNode)
		{
			var result = new ValueObjectDef(IntegerType, int.Parse(expressionNode.Text));
			return result;
		}

		protected ObjectDef EmitString(ITree expressionNode)
		{
			var result = new ValueObjectDef(StringType, expressionNode.Text);
			return result;
		}

		protected ObjectDef EmitBoolean(ITree expressionNode)
		{
			var result = new ValueObjectDef(BoolType, bool.Parse(expressionNode.Text));
			return result;
		}

		protected ObjectDef EmitVoid(ITree expressionNode)
		{
			var result = new ValueObjectDef(typeof(Nullable), null);
			return result;
		}

		protected ObjectDef EmitSelf(ITree expressionNode)
		{
			var result = new ArgObjectDef(CurrentTypeBuilder_, 0, "this");
			return result;
		}

		#endregion

		#region Helpers

		protected void GenerateAssemblyAndModuleBuilders()
		{
			AssemblyName_ = new AssemblyName() { Name = ProgramName_ };
			AssemblyBuilder_ = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName_, AssemblyBuilderAccess.Save);
			ModuleBuilder_ = AssemblyBuilder_.DefineDynamicModule(ProgramName_, ProgramName_ + ".exe", false);
		}

		protected Type GetType(string typeName)
		{
			Type result;
			switch (typeName)
			{
				case "Int":
					result = IntegerType;
					break;
				case "Bool":
					result = BoolType;
					break;
				case "String":
					result = StringType;
					break;
				case "Object":
					result = typeof(object);
					break;
				case "SELF_TYPE":
					result = ModuleBuilder_.GetType(CurrentTypeBuilder_.Name);
					break;
				default:
					result = ModuleBuilder_.GetType(typeName);
					break;
			}
			return result;
		}

		protected static ITree GetChild(ITree treeNode, int nodeType)
		{
			for (int i = 0; i < treeNode.ChildCount; i++)
				if (treeNode.GetChild(i).Type == nodeType)
					return treeNode.GetChild(i);
			return null;
		}

		protected static Type GetMostNearestAncestor(Type type1, Type type2)
		{
			List<Type> type1ParentTypes = new List<Type>();
			List<Type> type2ParentTypes = new List<Type>();
			Type t;

			Type result = typeof(object);

			if (type1 == typeof(Nullable) && !IsStaticType(type2))
				result = type2;
			else if (type2 == typeof(Nullable) && !IsStaticType(type1))
				result = type1;
			else
			{
				t = type1;
				while (t != typeof(object))
				{
					type1ParentTypes.Add(t);
					t = t.BaseType;
				}
				type1ParentTypes.Add(t);

				t = type2;
				while (t != typeof(object))
				{
					type2ParentTypes.Add(t);
					t = t.BaseType;
				}
				type2ParentTypes.Add(t);


				int ind1 = type1ParentTypes.Count - 1;
				int ind2 = type2ParentTypes.Count - 1;
				while (ind1 >= 0 && ind2 >= 0)
				{
					if (type1ParentTypes[ind1].Name != type2ParentTypes[ind2].Name)
						break;
					else
						result = type1ParentTypes[ind1];
					ind1--;
					ind2--;
				}
			}

			if (result == typeof(Nullable))
				result = typeof(object);

			return result;
		}

		protected static Type GetMostNearestAncestor(Type[] types)
		{
			Type result = types[0];

			for (int i = 1; i < types.Length; i++)
				result = GetMostNearestAncestor(result, types[i]);

			return result;
		}

		protected static bool IsStaticType(Type type)
		{
			return type == StringType || type == IntegerType || type == BoolType;
		}

		protected static bool CheckTypes(List<ObjectDef> args1, List<ArgObjectDef> args2)
		{
			if (args1.Count != args2.Count)
				return false;
			for (int i = 0; i < args1.Count; i++)
			{
				var tempType = args1[i].Type == typeof(Nullable) ? typeof(object) : args1[i].Type;
				if (!tempType.IsAssignableFrom(args2[i].Type))
					return false;
			}
			return true;
		}

		protected static ValueObjectDef EmitDefaultValue(Type type)
		{
			object value;
			if (type == BoolType)
				value = false;
			else if (type == IntegerType)
				value = 0;
			else if (type == StringType)
				value = "";
			else
				value = null;
			var result = new ValueObjectDef(type, value);

			return result;
		}

		#endregion
	}
}
