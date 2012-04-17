/* created on 11-04-2010 02:20:36 from peg generator V1.0 using 'Test' as input*/

using Peg.Base;
using System;
using System.IO;
using System.Text;
namespace BinaryFileSchemaParser
{
      
      public enum EBinaryFileSchemaParser{bfschema= 1, S= 2, whitespace= 3, comment= 4, singleline_comment= 5, 
                                           multiline_comment= 6, valid_item_name= 7, byteorder= 8, littleendian= 9, 
                                           bigendian= 10, skip= 11, conditional= 12, formatspecifier= 13, 
                                           datablocks= 14, datablock= 15, field= 16, localfield= 17, localassignment= 18, 
                                           type= 19, primitivetype= 20, namedtype= 21, functiontype= 22, 
                                           functionname= 23, action_list= 24, actions= 25, action= 26, assignment= 27, 
                                           output= 28, blockname= 29, p_string= 30, hex= 31, hexbyte= 32, 
                                           EOF= 33, fieldname= 34, arrayknown= 35, arrayunknown= 36, stoplist= 37, 
                                           or_keyword= 38, stopcase= 39, stopword= 40, inclusion= 41, p_struct= 42, 
                                           rel_offset= 43, abs_offset= 44, p_enum= 45, bitfield= 46, compression= 47, 
                                           compressionmethod= 48, enumfield= 49, p_else= 50, enumrange= 51, 
                                           enumname= 52, nameoractions= 53, bitname= 54, bit= 55, expression= 56, 
                                           logical= 57, bitwise= 58, comp= 59, shift= 60, sum= 61, prod= 62, 
                                           value= 63, call= 64, number= 65, invoke_name= 66, named_value= 67, 
                                           namelist= 68, varname= 69};
      public class BinaryFileSchemaParser : PegCharParser 
      {
        
         #region Input Properties
        public static EncodingClass encodingClass = EncodingClass.unicode;
        public static UnicodeDetection unicodeDetection = UnicodeDetection.FirstCharIsAscii;
        #endregion Input Properties
        #region Constructors
        public BinaryFileSchemaParser()
            : base()
        {
            
        }
        public BinaryFileSchemaParser(string src,TextWriter FerrOut)
			: base(src,FerrOut)
        {
            
        }
        #endregion Constructors
        #region Overrides
        public override string GetRuleNameFromId(int id)
        {
            try
            {
                   EBinaryFileSchemaParser ruleEnum = (EBinaryFileSchemaParser)id;
                    string s= ruleEnum.ToString();
                    int val;
                    if( int.TryParse(s,out val) ){
                        return base.GetRuleNameFromId(id);
                    }else{
                        return s;
                    }
            }
            catch (Exception)
            {
                return base.GetRuleNameFromId(id);
            }
        }
        public override void GetProperties(out EncodingClass encoding, out UnicodeDetection detection)
        {
            encoding = encodingClass;
            detection = unicodeDetection;
        } 
        #endregion Overrides
		#region Grammar Rules
        public bool bfschema()    /*^^bfschema:				S byteorder? S datablocks S (!./FATAL<"Expected end of file">);*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.bfschema,()=>
                And(()=>  
                     S()
                  && Option(()=> byteorder() )
                  && S()
                  && datablocks()
                  && S()
                  && (    Not(()=> Any() ) || Fatal("Expected end of file")) ) );
		}
        public bool S()    /*S:						(whitespace / comment)*;*/
        {

           return OptRepeat(()=>     whitespace() || comment() );
		}
        public bool whitespace()    /*whitespace:				[ \t\r\n];*/
        {

           return OneOf(" \t\r\n");
		}
        public bool comment()    /*comment:				singleline_comment / multiline_comment;*/
        {

           return     singleline_comment() || multiline_comment();
		}
        public bool singleline_comment()    /*singleline_comment:		'//' [^#x000D#x000A#x0085#x2028#x2029]*;*/
        {

           return And(()=>  
                     Char('/','/')
                  && OptRepeat(()=>    
                      NotOneOf("\u000d\u000a\u0085\u2028\u2029") ) );
		}
        public bool multiline_comment()    /*multiline_comment:		'/*' ( !'* /' . )* '* /';*/
        {

           return And(()=>  
                     Char('/','*')
                  && OptRepeat(()=>    
                      And(()=>    Not(()=> Char('*','/') ) && Any() ) )
                  && Char('*','/') );
		}
        public bool valid_item_name()    /*valid_item_name:		[_a-zA-Z][_a-zA-Z0-9]*;*/
        {

           return And(()=>  
                     (In('a','z', 'A','Z')||OneOf("_"))
                  && OptRepeat(()=>    
                      (In('a','z', 'A','Z', '0','9')||OneOf("_")) ) );
		}
        public bool byteorder()    /*^^byteorder:			'byteOrdering' S @'::' S ( littleendian / bigendian / FATAL<"'little-endian' or 'big-endian' expected."> ) S @';';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.byteorder,()=>
                And(()=>  
                     Char("byteOrdering")
                  && S()
                  && (    Char(':',':') || Fatal("<<'::'>> expected"))
                  && S()
                  && (    
                         littleendian()
                      || bigendian()
                      || Fatal("'little-endian' or 'big-endian' expected."))
                  && S()
                  && (    Char(';') || Fatal("<<';'>> expected")) ) );
		}
        public bool littleendian()    /*^^littleendian:			'little-endian';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.littleendian,()=>
                Char("little-endian") );
		}
        public bool bigendian()    /*^^bigendian:			'big-endian';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.bigendian,()=>
                Char("big-endian") );
		}
        public bool skip()    /*^^skip:					'skip' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.skip,()=>
                And(()=>    Char('s','k','i','p') && S() ) );
		}
        public bool conditional()    /*^^conditional:			'if' S '(' S expression S ')' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.conditional,()=>
                And(()=>  
                     Char('i','f')
                  && S()
                  && Char('(')
                  && S()
                  && expression()
                  && S()
                  && Char(')')
                  && S() ) );
		}
        public bool formatspecifier()    /*^^formatspecifier:		'format';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.formatspecifier,()=>
                Char('f','o','r','m','a','t') );
		}
        public bool datablocks()    /*datablocks:				(datablock S)+ / FATAL<"Datablock expected">;*/
        {

           return   
                     PlusRepeat(()=> And(()=>    datablock() && S() ) )
                  || Fatal("Datablock expected");
		}
        public bool datablock()    /*datablock:				(struct / rel_offset / abs_offset / enum / bitfield) S;*/
        {

           return And(()=>  
                     (    
                         p_struct()
                      || rel_offset()
                      || abs_offset()
                      || p_enum()
                      || bitfield())
                  && S() );
		}
        public bool field()    /*^^field:				skip? conditional? fieldname S '::' S type @';';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.field,()=>
                And(()=>  
                     Option(()=> skip() )
                  && Option(()=> conditional() )
                  && fieldname()
                  && S()
                  && Char(':',':')
                  && S()
                  && type()
                  && (    Char(';') || Fatal("<<';'>> expected")) ) );
		}
        public bool localfield()    /*^^localfield:			'local' S @fieldname S @'::' S @primitivetype S localassignment? @';';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.localfield,()=>
                And(()=>  
                     Char('l','o','c','a','l')
                  && S()
                  && (    fieldname() || Fatal("<<fieldname>> expected"))
                  && S()
                  && (    Char(':',':') || Fatal("<<'::'>> expected"))
                  && S()
                  && (    
                         primitivetype()
                      || Fatal("<<primitivetype>> expected"))
                  && S()
                  && Option(()=> localassignment() )
                  && (    Char(';') || Fatal("<<';'>> expected")) ) );
		}
        public bool localassignment()    /*localassignment: 		'=' S expression S;*/
        {

           return And(()=>    Char('=') && S() && expression() && S() );
		}
        public bool type()    /*^^type:					(primitivetype / functiontype / namedtype / FATAL<"primitive, named or function type expected">) S (arrayknown / arrayunknown)? S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.type,()=>
                And(()=>  
                     (    
                         primitivetype()
                      || functiontype()
                      || namedtype()
                      || Fatal("primitive, named or function type expected"))
                  && S()
                  && Option(()=>     arrayknown() || arrayunknown() )
                  && S() ) );
		}
        public bool primitivetype()    /*^^primitivetype:		'bool' / 'sbyte' / 'ubyte' / 'short' / 'ushort' / 'int' / 'uint' / 'long' / 'ulong';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.primitivetype,()=>
                OneOfLiterals(optimizedLiterals0) );
		}
        public bool namedtype()    /*^^namedtype:			valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.namedtype,()=>
                valid_item_name() );
		}
        public bool functiontype()    /*^^functiontype:			functionname S '(' S @string S ')' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.functiontype,()=>
                And(()=>  
                     functionname()
                  && S()
                  && Char('(')
                  && S()
                  && (    p_string() || Fatal("<<string>> expected"))
                  && S()
                  && Char(')')
                  && S() ) );
		}
        public bool functionname()    /*^^functionname:			valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.functionname,()=>
                valid_item_name() );
		}
        public bool action_list()    /*^^action_list:			'[' S (actions / FATAL<"Empty action list">) S @']' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.action_list,()=>
                And(()=>  
                     Char('[')
                  && S()
                  && (    actions() || Fatal("Empty action list"))
                  && S()
                  && (    Char(']') || Fatal("<<']'>> expected"))
                  && S() ) );
		}
        public bool actions()    /*actions:				action (',' S actions )?;*/
        {

           return And(()=>  
                     action()
                  && Option(()=>    
                      And(()=>    Char(',') && S() && actions() ) ) );
		}
        public bool action()    /*action:					assignment / output;*/
        {

           return     assignment() || output();
		}
        public bool assignment()    /*^^assignment:			varname S '=' S @expression S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.assignment,()=>
                And(()=>  
                     varname()
                  && S()
                  && Char('=')
                  && S()
                  && (    expression() || Fatal("<<expression>> expected"))
                  && S() ) );
		}
        public bool output()    /*^^output:				^('warning'/'error'/'debug') S @'(' S @string S @')' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.output,()=>
                And(()=>  
                     TreeChars(()=>    
                            
                               Char('w','a','r','n','i','n','g')
                            || Char('e','r','r','o','r')
                            || Char('d','e','b','u','g') )
                  && S()
                  && (    Char('(') || Fatal("<<'('>> expected"))
                  && S()
                  && (    p_string() || Fatal("<<string>> expected"))
                  && S()
                  && (    Char(')') || Fatal("<<')'>> expected"))
                  && S() ) );
		}
        public bool blockname()    /*^^blockname:			valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.blockname,()=>
                valid_item_name() );
		}
        public bool p_string()    /*^^string:				'"' ( !'"' . )* '"';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.p_string,()=>
                And(()=>  
                     Char('"')
                  && OptRepeat(()=>    
                      And(()=>    Not(()=> Char('"') ) && Any() ) )
                  && Char('"') ) );
		}
        public bool hex()    /*^^hex:					'0x' hexbyte+;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.hex,()=>
                And(()=>    Char('0','x') && PlusRepeat(()=> hexbyte() ) ) );
		}
        public bool hexbyte()    /*hexbyte:				[0-9A-Fa-f] ([0-9A-Fa-f] / FATAL<"Two hex chars required per byte.">);*/
        {

           return And(()=>  
                     In('0','9', 'A','F', 'a','f')
                  && (    
                         In('0','9', 'A','F', 'a','f')
                      || Fatal("Two hex chars required per byte.")) );
		}
        public bool EOF()    /*^^EOF:					'EOF';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.EOF,()=>
                Char('E','O','F') );
		}
        public bool fieldname()    /*^^fieldname:			valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.fieldname,()=>
                valid_item_name() );
		}
        public bool arrayknown()    /*^^arrayknown:			'[' expression @']';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.arrayknown,()=>
                And(()=>  
                     Char('[')
                  && expression()
                  && (    Char(']') || Fatal("<<']'>> expected")) ) );
		}
        public bool arrayunknown()    /*^^arrayunknown:			'[' S ']' S ^'until' S stoplist S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.arrayunknown,()=>
                And(()=>  
                     Char('[')
                  && S()
                  && Char(']')
                  && S()
                  && TreeChars(()=> Char('u','n','t','i','l') )
                  && S()
                  && stoplist()
                  && S() ) );
		}
        public bool stoplist()    /*stoplist:				stopcase S (or_keyword S stoplist S )? S;*/
        {

           return And(()=>  
                     stopcase()
                  && S()
                  && Option(()=>    
                      And(()=>    or_keyword() && S() && stoplist() && S() ) )
                  && S() );
		}
        public bool or_keyword()    /*^or_keyword:			'or';*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.or_keyword,()=>
                Char('o','r') );
		}
        public bool stopcase()    /*^^stopcase:				( stopword S inclusion? S) / EOF / FATAL<"Hex, string or EOF expected">;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.stopcase,()=>
                  
                     And(()=>    
                         stopword()
                      && S()
                      && Option(()=> inclusion() )
                      && S() )
                  || EOF()
                  || Fatal("Hex, string or EOF expected") );
		}
        public bool stopword()    /*stopword:				string / hex;*/
        {

           return     p_string() || hex();
		}
        public bool inclusion()    /*^^inclusion:			'included' / 'excluded' / 'skipped';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.inclusion,()=>
                  
                     Char("included")
                  || Char("excluded")
                  || Char('s','k','i','p','p','e','d') );
		}
        public bool p_struct()    /*^^struct:				formatspecifier? S ^'struct'     S @blockname S compression? S						   @'{' S (localfield S)* (field S)+ @'}' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.p_struct,()=>
                And(()=>  
                     Option(()=> formatspecifier() )
                  && S()
                  && TreeChars(()=> Char('s','t','r','u','c','t') )
                  && S()
                  && (    blockname() || Fatal("<<blockname>> expected"))
                  && S()
                  && Option(()=> compression() )
                  && S()
                  && (    Char('{') || Fatal("<<'{'>> expected"))
                  && S()
                  && OptRepeat(()=> And(()=>    localfield() && S() ) )
                  && PlusRepeat(()=> And(()=>    field() && S() ) )
                  && (    Char('}') || Fatal("<<'}'>> expected"))
                  && S() ) );
		}
        public bool rel_offset()    /*^^rel_offset:			formatspecifier? S ^'rel_offset' S @blockname S compression? S '::' S @primitivetype S @'{' S (localfield S)* (field S)+ @'}' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.rel_offset,()=>
                And(()=>  
                     Option(()=> formatspecifier() )
                  && S()
                  && TreeChars(()=> Char("rel_offset") )
                  && S()
                  && (    blockname() || Fatal("<<blockname>> expected"))
                  && S()
                  && Option(()=> compression() )
                  && S()
                  && Char(':',':')
                  && S()
                  && (    
                         primitivetype()
                      || Fatal("<<primitivetype>> expected"))
                  && S()
                  && (    Char('{') || Fatal("<<'{'>> expected"))
                  && S()
                  && OptRepeat(()=> And(()=>    localfield() && S() ) )
                  && PlusRepeat(()=> And(()=>    field() && S() ) )
                  && (    Char('}') || Fatal("<<'}'>> expected"))
                  && S() ) );
		}
        public bool abs_offset()    /*^^abs_offset:			formatspecifier? S ^'abs_offset' S @blockname S compression? S '::' S @primitivetype S @'{' S (localfield S)* (field S)+ @'}' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.abs_offset,()=>
                And(()=>  
                     Option(()=> formatspecifier() )
                  && S()
                  && TreeChars(()=> Char("abs_offset") )
                  && S()
                  && (    blockname() || Fatal("<<blockname>> expected"))
                  && S()
                  && Option(()=> compression() )
                  && S()
                  && Char(':',':')
                  && S()
                  && (    
                         primitivetype()
                      || Fatal("<<primitivetype>> expected"))
                  && S()
                  && (    Char('{') || Fatal("<<'{'>> expected"))
                  && S()
                  && OptRepeat(()=> And(()=>    localfield() && S() ) )
                  && PlusRepeat(()=> And(()=>    field() && S() ) )
                  && (    Char('}') || Fatal("<<'}'>> expected"))
                  && S() ) );
		}
        public bool p_enum()    /*^^enum:					formatspecifier? S ^'enum'       S @blockname S '::' S @primitivetype S @'{' S (localfield S)* (enumfield S)+ @'}' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.p_enum,()=>
                And(()=>  
                     Option(()=> formatspecifier() )
                  && S()
                  && TreeChars(()=> Char('e','n','u','m') )
                  && S()
                  && (    blockname() || Fatal("<<blockname>> expected"))
                  && S()
                  && Char(':',':')
                  && S()
                  && (    
                         primitivetype()
                      || Fatal("<<primitivetype>> expected"))
                  && S()
                  && (    Char('{') || Fatal("<<'{'>> expected"))
                  && S()
                  && OptRepeat(()=> And(()=>    localfield() && S() ) )
                  && PlusRepeat(()=> And(()=>    enumfield() && S() ) )
                  && (    Char('}') || Fatal("<<'}'>> expected"))
                  && S() ) );
		}
        public bool bitfield()    /*^^bitfield:				formatspecifier? S ^'bitfield'   S @blockname S '::' S @primitivetype S @'{' S (localfield S)* (bit S)+ @'}' S;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.bitfield,()=>
                And(()=>  
                     Option(()=> formatspecifier() )
                  && S()
                  && TreeChars(()=> Char("bitfield") )
                  && S()
                  && (    blockname() || Fatal("<<blockname>> expected"))
                  && S()
                  && Char(':',':')
                  && S()
                  && (    
                         primitivetype()
                      || Fatal("<<primitivetype>> expected"))
                  && S()
                  && (    Char('{') || Fatal("<<'{'>> expected"))
                  && S()
                  && OptRepeat(()=> And(()=>    localfield() && S() ) )
                  && PlusRepeat(()=> And(()=>    bit() && S() ) )
                  && (    Char('}') || Fatal("<<'}'>> expected"))
                  && S() ) );
		}
        public bool compression()    /*compression:			'<' S compressionmethod S '>';*/
        {

           return And(()=>  
                     Char('<')
                  && S()
                  && compressionmethod()
                  && S()
                  && Char('>') );
		}
        public bool compressionmethod()    /*^^compressionmethod:	valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.compressionmethod,()=>
                valid_item_name() );
		}
        public bool enumfield()    /*^^enumfield:			( number / enumrange / else ) S nameoractions? @';';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.enumfield,()=>
                And(()=>  
                     (    number() || enumrange() || p_else())
                  && S()
                  && Option(()=> nameoractions() )
                  && (    Char(';') || Fatal("<<';'>> expected")) ) );
		}
        public bool p_else()    /*^^else:					'else';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.p_else,()=>
                Char('e','l','s','e') );
		}
        public bool enumrange()    /*^^enumrange:			^('['/']') S @number S @'..' S @number S ^('['/']');*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.enumrange,()=>
                And(()=>  
                     TreeChars(()=>     Char('[') || Char(']') )
                  && S()
                  && (    number() || Fatal("<<number>> expected"))
                  && S()
                  && (    Char('.','.') || Fatal("<<'..'>> expected"))
                  && S()
                  && (    number() || Fatal("<<number>> expected"))
                  && S()
                  && TreeChars(()=>     Char('[') || Char(']') ) ) );
		}
        public bool enumname()    /*^^enumname:				valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.enumname,()=>
                valid_item_name() );
		}
        public bool nameoractions()    /*nameoractions:			':' S enumname? S action_list? S;*/
        {

           return And(()=>  
                     Char(':')
                  && S()
                  && Option(()=> enumname() )
                  && S()
                  && Option(()=> action_list() )
                  && S() );
		}
        public bool bitname()    /*^^bitname:				valid_item_name;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.bitname,()=>
                valid_item_name() );
		}
        public bool bit()    /*^^bit:					number S (':' S bitname? S action_list?)? @';';*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.bit,()=>
                And(()=>  
                     number()
                  && S()
                  && Option(()=>    
                      And(()=>      
                               Char(':')
                            && S()
                            && Option(()=> bitname() )
                            && S()
                            && Option(()=> action_list() ) ) )
                  && (    Char(';') || Fatal("<<';'>> expected")) ) );
		}
        public bool expression()    /*^^expression:			S logical;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.expression,()=>
                And(()=>    S() && logical() ) );
		}
        public bool logical()    /*^logical:				bitwise (^('&&'/'||') S @bitwise)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.logical,()=>
                And(()=>  
                     bitwise()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=>     Char('&','&') || Char('|','|') )
                            && S()
                            && (    bitwise() || Fatal("<<bitwise>> expected")) ) ) ) );
		}
        public bool bitwise()    /*^bitwise:				comp (^('^'/'&'/ ('|' !'|')) S @comp)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.bitwise,()=>
                And(()=>  
                     comp()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=>        
                                              
                                                 Char('^')
                                              || Char('&')
                                              || And(()=>    Char('|') && Not(()=> Char('|') ) ) )
                            && S()
                            && (    comp() || Fatal("<<comp>> expected")) ) ) ) );
		}
        public bool comp()    /*^comp:					shift (^('!='/'=='/'<='/'>='/'<'/'>') S @shift)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.comp,()=>
                And(()=>  
                     shift()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=>        
                                              
                                                 Char('!','=')
                                              || Char('=','=')
                                              || Char('<','=')
                                              || Char('>','=')
                                              || Char('<')
                                              || Char('>') )
                            && S()
                            && (    shift() || Fatal("<<shift>> expected")) ) ) ) );
		}
        public bool shift()    /*^shift:					sum (^('>>'/'<<') S @sum)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.shift,()=>
                And(()=>  
                     sum()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=>     Char('>','>') || Char('<','<') )
                            && S()
                            && (    sum() || Fatal("<<sum>> expected")) ) ) ) );
		}
        public bool sum()    /*^sum:					prod (^[+-] S @prod)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.sum,()=>
                And(()=>  
                     prod()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=> OneOf("+-") )
                            && S()
                            && (    prod() || Fatal("<<prod>> expected")) ) ) ) );
		}
        public bool prod()    /*^prod:					value (^[* /%] S @value)*;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.prod,()=>
                And(()=>  
                     value()
                  && OptRepeat(()=>    
                      And(()=>      
                               TreeChars(()=> OneOf("*/%") )
                            && S()
                            && (    value() || Fatal("<<value>> expected")) ) ) ) );
		}
        public bool value()    /*value:					( number / '(' S logical S @')' S / call / named_value ) S;*/
        {

           return And(()=>  
                     (    
                         number()
                      || And(()=>      
                               Char('(')
                            && S()
                            && logical()
                            && S()
                            && (    Char(')') || Fatal("<<')'>> expected"))
                            && S() )
                      || call()
                      || named_value())
                  && S() );
		}
        public bool call()    /*^call:					invoke_name S '(' S @(^valid_item_name) S @')' S;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.call,()=>
                And(()=>  
                     invoke_name()
                  && S()
                  && Char('(')
                  && S()
                  && (    
                         TreeChars(()=> valid_item_name() )
                      || Fatal("<<(^valid_item_name)>> expected"))
                  && S()
                  && (    Char(')') || Fatal("<<')'>> expected"))
                  && S() ) );
		}
        public bool number()    /*^number:				[0-9]+;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.number,()=>
                PlusRepeat(()=> In('0','9') ) );
		}
        public bool invoke_name()    /*^invoke_name:			valid_item_name;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.invoke_name,()=>
                valid_item_name() );
		}
        public bool named_value()    /*^^named_value:			namelist;*/
        {

           return TreeNT((int)EBinaryFileSchemaParser.named_value,()=>
                namelist() );
		}
        public bool namelist()    /*namelist:				varname ('.' namelist )?;*/
        {

           return And(()=>  
                     varname()
                  && Option(()=> And(()=>    Char('.') && namelist() ) ) );
		}
        public bool varname()    /*^varname:				valid_item_name;*/
        {

           return TreeAST((int)EBinaryFileSchemaParser.varname,()=>
                valid_item_name() );
		}
		#endregion Grammar Rules

        #region Optimization Data 
        
        internal static OptimizedLiterals optimizedLiterals0;
        
        static BinaryFileSchemaParser()
        {
            
            {
               string[] literals=
               { "bool","sbyte","ubyte","short","ushort","int","uint","long",
                  "ulong" };
               optimizedLiterals0= new OptimizedLiterals(literals);
            }

            
        }
        #endregion Optimization Data 
           }
}