using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace CoolCompiler
{
	class Compiler
	{
		//throw new CompilerException(nvae.Message, nvae.Line, nvae.CharPositionInLine);
		public void Compile(string FileName, string Source)
		{
			Tokens = new List<Token>();
			Errors = new List<CompilerError>();
			CoolGrammarLexer lexer = null;
			CoolGrammarParser parser = null;
			CoolCodeGenerator generator = null;

			try
			{
				var stream = new ANTLRStringStream(Source);
				lexer = new CoolGrammarLexer(stream, new RecognizerSharedState() { errorRecovery = true });

				IToken token;
				token = lexer.NextToken();
				while (token.Type != CoolGrammarLexer.EOF)
				{
					Tokens.Add(
						new Token
						{
							Name = CoolTokens.Dictionary[token.Type],
							Value = token.Text,
							Line = token.Line,
							Column = token.CharPositionInLine
						});
					token = lexer.NextToken();
				}
				lexer.Reset();
				lexer.Line = 0;
				lexer.CharPositionInLine = 0;
			}
			catch (EarlyExitException exception)
			{
				Errors.Add(new EarlyExitErrorLexer(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (MismatchedSetException exception)
			{
				Errors.Add(new MismatchedSetErrorLexer(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (NoViableAltException exception)
			{
				Errors.Add(new NoViableAltErrorLexer(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (CompilerException exception)
			{
				Errors.Add(new CommonLexerError(exception.Message, Errors.Count(), exception.Line, exception.Column));
			}
			catch
			{

			}
			
			try
			{
				var tokenStream = new CommonTokenStream(lexer);
				parser = new CoolGrammarParser(tokenStream);
				Tree = parser.program();
			}
			catch (EarlyExitException exception)
			{
				Errors.Add(new EarlyExitErrorParser(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (MismatchedSetException exception)
			{
				Errors.Add(new MismatchedSetErrorParser(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (NoViableAltException exception)
			{
				Errors.Add(new NoViableAltErrorParser(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (RecognitionException exception)
			{
				Errors.Add(new RecognitionError(exception.Message,
					Errors.Count(), exception.Line, exception.CharPositionInLine));
			}
			catch (RewriteEarlyExitException exception)
			{
				Errors.Add(new RewriteEarlyExitError(exception.Message,
					Errors.Count()));
			}
			catch (CompilerException exception)
			{
				Errors.Add(new CommonParserError(exception.Message, Errors.Count(), exception.Line, exception.Column));
			}
			catch (Exception exception)
			{
				Errors.Add(new CommonParserError(exception.Message, Errors.Count(), null, null));
			}

			try
			{
				generator = new CoolCodeGenerator(FileName, Tree.Tree);
				generator.Generate();

				GeneratedProgramName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".exe";

				foreach (var error in generator.CompilerErrors)
					Errors.Add(error);
			}
			catch (Exception e)
			{
				Errors.Add(new CommonError(e.Message, Errors.Count));
			}
			
			
			if (Tree == null)
				Tree = new AstParserRuleReturnScope<CommonTree, CommonToken>();
		}

		public ICollection<CompilerError> Errors
		{
			get;
			private set;
		}

		public bool HasErrors
		{
			get
			{
				return Errors.Count() != 0;
			}
		}

		public CoolGrammarLexer Lexer
		{
			get;
			private set;
		}

		public CoolGrammarParser Parser
		{
			get;
			private set;
		}

		public AstParserRuleReturnScope<CommonTree, CommonToken> Tree
		{
			get;
			private set;
		}

		public ICollection<Token> Tokens
		{
			get;
			private set;
		}

		public string GeneratedProgramName
		{
			get;
			private set;
		}
	}
}
