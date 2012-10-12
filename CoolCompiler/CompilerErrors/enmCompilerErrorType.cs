using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolCompiler
{
	public enum enmCompilerErrorType
	{
		EarleyExitLexer,
		MismatchedSetLexer,
		NoViableAltLexer,

		EarleyExitParser,
		MismatchedSetParser,
		NoViableAltParser,
		RecognitionParser,
		RewriteEarlyExitParser,
		UnexpectedToken,

		IncompatibleTypes,
		InvalidReturnType,
		ComparsionOperator,
		ArithmeticOperator,
		NegOperator,
		NotOperator,
		IsVoidChecking,
		IfOperator,
		WhileOperator,
		FunctionArguments,
		EntryPoint,
		UndefinedId,
		UndefinedFunction,
		UndefinedClass,

		Common,
		CommonLexer,
		CommonParser,
		FileInUse,
	}
}
