grammar CoolGrammar;

options {
  language = CSharp3;
  output=AST;
  TokenLabelType=CommonToken;
  ASTLabelType=CommonTree;
}

tokens {
	ClassesList;
	Class;
	FeatureList;
	FuncDef;	
	FunctionArgsList;
	Term;
	AtSignTypeName;
	ImplicitInvoke;
	Expr;
	Curly;
	InvokeExprs;
	Exprs;
	LetExprs;
	LocalOrFieldInit;
	CaseExp;
	IdValue;
	IntegerValue;
	StringValue;
	BooleanValue;
	VoidValue;
	SelfValue;
}

@lexer::namespace {CoolCompiler} 
@parser::namespace {CoolCompiler} 

@lexer::header {#pragma warning disable 3021

using System;
using System.Text;
}

@parser::header {#pragma warning disable 3021

using System.Text;}

@rulecatch {
	catch (RecognitionException e) {
//throw e;
	}
}

public program:
	(classDef SEMI)+ -> ^(ClassesList classDef+);
	
classDef: 
	CLASS typeName (INHERITS typeName)? LCURLY featureList RCURLY ->
		^(Class typeName featureList typeName?);
		
featureList:
	(feature SEMI)* -> ^(FeatureList feature*);
	
feature: 
 	(ID LPAREN (formalList)? RPAREN COLON typeName LCURLY expr RCURLY) -> 
 		^(FuncDef ID typeName expr formalList?)
  	| localOrFieldInit ;

formalList:
	formal (COMMA formal)* -> ^(FunctionArgsList formal+);

formal: ID COLON^ typeName;

expr:
	(ID ASSIGN^)* not;
	
not: 
	(NOT^)* relation;
	
relation:
	addition ((LE^ | LT^ | GE^ | GT^ | EQUAL^) addition)*;
	
addition:
	multiplication ((PLUS^ | MINUS^) multiplication)*;

multiplication:
	isvoid ((MULT^ | DIV^) isvoid)*;

isvoid:
	(ISVOID^)* neg;

neg:
	(NEG^)* dot;

dot:
	term ((ATSIGN typeName)? DOT ID LPAREN invokeExprs? RPAREN)? ->
		^(Term term ID? typeName? invokeExprs?);

term:
	  ID LPAREN invokeExprs? RPAREN -> ^(ImplicitInvoke ID invokeExprs?)
	| IF^ expr THEN! expr ELSE! expr FI!
	| WHILE^ expr LOOP! expr POOL! 
	| LCURLY (expr SEMI)+ RCURLY -> ^(Exprs expr+)
	| LET^ letExprs IN! expr SEMI!
	| CASE^ expr OF! caseExpr+ ESAC!
	| NEW^ typeName SEMI!
	| LPAREN expr RPAREN -> ^(Expr expr)
	| ID^
	| INTEGER^
	| STRING^
	| TRUE^
	| FALSE^
	| VOID^
	| SELF^;

invokeExprs:
	expr (COMMA expr)* -> ^(InvokeExprs expr+);
	
letExprs:	
	localOrFieldInit (COMMA localOrFieldInit)* -> ^(LetExprs localOrFieldInit+);
	
localOrFieldInit:	
	ID COLON typeName (ASSIGN expr)? -> ^(LocalOrFieldInit ID typeName expr?);
	
caseExpr:	
	ID COLON typeName HENCE expr SEMI -> ^(CaseExp ID typeName expr);
	
typeName
	: IntTypeName
	| BoolTypeName
	| StringTypeName
	| ObjectTypeName
	| SelfTypeTypeName
	| ID;

//----------------------------------------------------------------------------
// KEYWORDS
//----------------------------------------------------------------------------
	CLASS : 'class';
	ELSE : 'else';
	FALSE : 'false';
	FI : 'fi';
	IF : 'if';
	IN : 'in';
	INHERITS : 'inherits';
	ISVOID : 'isvoid';
	LET : 'let';
	LOOP : 'loop';
	POOL : 'pool';
	THEN : 'then';
	WHILE : 'while';
	CASE : 'case';
	ESAC : 'esac';
	NEW : 'new';
	OF : 'of';
	NOT : 'not';
	TRUE : 'true';
	VOID : 'void';
	SELF : 'self';
	
	
//----------------------------------------------------------------------------
// OPERATORS
//----------------------------------------------------------------------------
	DOT: '.';
	ATSIGN: '@';
	NEG: '~';
	MULT: '*';
	DIV: '/';
	PLUS: '+';
	MINUS: '-';
	LE: '<=';
	LT: '<';
	GE: '>=';
	GT: '>';
	EQUAL: '=';
	ASSIGN: '<-' ;
	SEMI: ';';
	LPAREN: '(';
	RPAREN: ')';
	LCURLY: '{';
	RCURLY: '}';
	COMMA: ',';
	COLON: ':';
	HENCE: '=>';
	
	IntTypeName: 'Int';
	BoolTypeName: 'Bool';
	StringTypeName: 'String';
	ObjectTypeName: 'Object';
	SelfTypeTypeName: 'SELF_TYPE';

MULTILINE_COMMENT : '(*' .* '*)' {$channel = Hidden;} ;

STRING:	'"'
		{ StringBuilder b = new StringBuilder(); }
		(	'"' '"'				{ b.Append('"');}
		|	c=~('"'|'\r'|'\n')	{ b.Append((char)c);}
		)*
		'"'
		{ Text = b.ToString(); }
	;

fragment LETTER : ('a'..'z' | 'A'..'Z') ;
fragment DIGIT : '0'..'9';
INTEGER : DIGIT+ ;
ID : LETTER (LETTER | DIGIT | '_')*;
WS : (' ' | '\t' | '\n' | '\r' | '\f')+ {$channel = Hidden;};
COMMENT : '--' .* ('\n'|'\r') {$channel = Hidden;};