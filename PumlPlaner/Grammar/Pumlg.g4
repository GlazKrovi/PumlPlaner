grammar Pumlg;

// we can't use PascalCase because of g4 syntax rules

uml:
    '@startuml'
    (NEWLINE | class_diagram)
    '@enduml'
    ;

class_diagram
    : (class_declaration | enum_declaration | connection | hide_declaration | NEWLINE)*
    ;

class_declaration:
    class_type ident template_parameter_list? stereotype? inheritance_declaration? ('{'
    (class_member | NEWLINE)*
    '}' )?
    ;

inheritance_declaration:
    extends_declaration implements_declaration?
    | implements_declaration
    ;

extends_declaration:
    'extends' ident
    ;

implements_declaration:
    'implements' ident (',' ident)*
    ;

class_member:
    attribute
    | method
    ;

hide_declaration:
    'hide' ident;

attribute:
    visibility?
    modifiers?
    (type_declaration? ident | ident ':' type_declaration)
    NEWLINE
    ;

method:
    visibility?
    modifiers?
    (type_declaration? ident '(' function_argument_list? ')' | ident '(' function_argument_list? ')' ':' type_declaration)
    NEWLINE
    ;

connection_left:
    instance=ident ('"' attrib=ident mult=multiplicity? '"')?
    ;

connection_right:
    ('"' attrib=ident mult=multiplicity? '"')? instance=ident
    ;

connection:
    left=connection_left
    CONNECTOR
    right=connection_right
    (':' stereotype)?
    NEWLINE
    ;

multiplicity: ('*' | '0..1' | '0..*' | '1..*' | '1');

visibility:
    '+'     # visibility_public
    |'-'    # visibility_private
    |'#'    # visibility_protected
    ;

function_argument:
    type_declaration? ident
    ;

function_argument_list:
    function_argument (',' function_argument)*
    ;

template_argument:
    type_declaration
    ;

template_argument_list:
    template_argument (',' template_argument)*
    ;

template_parameter_list:
    '<' template_parameter (',' template_parameter)* '>'
    ;

template_parameter:
    ident
    ;

ident:
    IDENT
    ;

modifiers:
    static_mod='{static}'
    | abstract_mod='{abstract}'
    | override_mod='{override}'
    | virtual_mod='{virtual}'
    | sealed_mod='{sealed}'
    | readonly_mod='{readonly}'
    | const_mod='{const}'
    ;

stereotype:
    '<<' name=ident ('(' args+=ident ')')? '>>'
    ;

type_declaration:
    ident '<' template_argument_list? '>'               # template_type
    | ident LBRACKET RBRACKET                           # list_type
    | template_parameter LBRACKET RBRACKET              # generic_list_type
    | template_parameter                                 # generic_simple_type
    | ident                                             # simple_type
    ;

class_type:
    'abstract' 'class'?
    | 'class'
    | 'interface' 'class'?
    ;

item_list:
    (ident NEWLINE)+
    ;

enum_declaration:
    'enum' ident ('{' NEWLINE
    item_list?
    '}' )?
    ;

CONNECTOR:
    '--'
    | '..'
    | '-->'
    | '<--'
    | '--*'
    | '*--'
    | '--o'
    | 'o--'
    | '<|--'
    | '--|>'
    | '..|>'
    | '<|..'
    | '*-->'
    | '<--*'
    | 'o-->'
    | '<--o'
    | '-'
    | '.'
    | '->'
    | '<-'
    | '-*'
    | '*-'
    | '-o'
    | 'o-'
    | '<|-'
    | '-|>'
    | '.|>'
    | '<|.'
    | '*->'
    | '<-*'
    | 'o->'
    | '<-o'
    ;

NEWPAGE : 'newpage' -> channel(HIDDEN)
    ;

NEWLINE  :   [\r\n];

IDENT : NONDIGIT ( DIGIT | NONDIGIT )*;

// Tokens pour les crochets de tableau (doivent être avant CONNECTOR pour éviter l'ambiguïté)
LBRACKET : '[' ;
RBRACKET : ']' ;

COMMENT :
    ('/' '/' .*? '\n' | '/*' .*? '*/') -> channel(HIDDEN)
    ;
WS  :   [ ]+ -> skip ; // toss out whitespace

//=========================================================
// Fragments
//=========================================================
fragment NONDIGIT : [_a-zA-Z];
fragment DIGIT :  [0-9];
fragment UNSIGNED_INTEGER : DIGIT+;
