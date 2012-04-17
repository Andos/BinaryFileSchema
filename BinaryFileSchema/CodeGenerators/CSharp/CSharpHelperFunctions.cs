using System.Collections.Generic;
using System.Text;

namespace BFSchema.CodeGenerators.CSharp
{
    public class CSHelper
    {
        public static string TypeMap(IBfsType type)
        {
            if (type is BfsNamedType)
                return (type as BfsNamedType).DataBlock.Name;

            if (type is BfsPrimitiveType)
            {
                string typeString = (type as BfsPrimitiveType).PrimitiveType.ToString();
                switch (typeString)
                {
                    case "Ubyte":
                        return "Byte";
                    case "Sbyte":
                        return "Byte";
                    default:
                        return typeString;
                }
            }
            else return "/* TODO!!! */";
        }

        public static string ReadMap(BfsPrimitiveType primitiveType)
        {
            switch (primitiveType.PrimitiveType.ToString())
            {
                case "Ubyte":
                    return "Byte";
                case "Sbyte":
                    return "SByte";
                case "Short":
                    return "Int16";
                case "Ushort":
                    return "UInt16";
                case "Int":
                    return "Int32";
                case "Uint":
                    return "UInt32";
                case "Long":
                    return "Int64";
                case "Ulong":
                    return "UInt64";
                default:
                    return primitiveType.PrimitiveType.ToString();
            }
        }

        //Output C# code for an expression
        public static string MakeExpression(BfsExpression expression, IBfsDataBlock owner)
        {
            string exp = VisitExpGroup(expression.ExpressionGroup, owner);

            //Trim unnessecary parenthesis (BROKEN! not sure the parenthethis are matching)
            /*while (exp.StartsWith("(") && exp.EndsWith(")"))
                exp = exp.Substring(1, exp.Length - 2);*/

            return exp.Trim();
        }

        //Recursive visit of expression groups
        public static string VisitExpGroup(BfsExpGroup group, IBfsDataBlock owner)
        {
            StringBuilder b = new StringBuilder();
            b.Append("(");

            for (int i = 0; i < group.Members.Count; i++)
            {
                IBfsExpNode node = group.Members[i];

                if (node is BfsExpGroup)
                    b.Append(VisitExpGroup(node as BfsExpGroup, owner));
                else if (node is BfsExpressionVariable)
                {
                    BfsExpressionVariable expVar = node as BfsExpressionVariable;
                    b.Append(owner.Name.ToLower());

                    foreach (IBfsNamedField namedField in expVar.NameHierarchy)
                        b.Append("." + namedField.Name);

                    //Only append this last reading into the enum type if the value is being compared against an EnumAliasExp
                    if ((group.Members.Count > i + 2 && group.Members[i + 2] is BfsEnumAliasExp)
                        || (i >= 2 && group.Members[i - 2] is BfsEnumAliasExp))
                        b.Append("." + expVar.NameHierarchy[expVar.NameHierarchy.Count - 1]);
                }
                else if (node is BfsEnumAliasExp)
                {
                    //TODO
                    BfsEnumAliasExp enumAliasExp = node as BfsEnumAliasExp;
                    b.Append(enumAliasExp.EnumBlock.Name + "." + enumAliasExp.EnumBlock.Name + "Enum." + enumAliasExp.Alias.Name);
                }
                else if (node is BfsLocalField)
                {
                    BfsLocalField localField = node as BfsLocalField;
                    b.Append(localField.Name);
                }
                else
                    b.Append(" " + node + " ");
            }
            b.Append(")");
            return b.ToString();
        }

    }



    public class CodeClass
    {
        public enum ClassTypeEnum { Class, Struct }
        public CodeClass(string name) { Name = name; ClassType = ClassTypeEnum.Class; }
        public string Name { get; set; }
        public List<CodeMethod> CodeMethods { get { return codeMethods; } }
        public List<string> CodeFields { get { return codeFields; } }
        public ClassTypeEnum ClassType { get; set; }

        List<CodeMethod> codeMethods = new List<CodeMethod>();
        List<string> codeFields = new List<string>();
     

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("\tpublic " + ClassType.ToString().ToLower() + " " + Name);
            b.AppendLine("\t{");

            foreach (string line in codeFields)
                b.AppendLine("\t\tpublic " + line);
            b.AppendLine();

            foreach (CodeMethod cm in codeMethods)
                b.AppendLine(cm.ToString());

            b.AppendLine("\t}");
            b.AppendLine();
            return b.ToString();
        }
    }

    public class CodeMethod
    {
        public CodeMethod(string name) { Name = name; }
        public string Name { get; set; }
        public List<string> CodeLines { get { return codeLines; } }
        List<string> codeLines = new List<string>();

		public void AddSplitLines(string lines)
		{
			string [] lineArray = lines.Split(new char[] { '\n', '\r'});
			foreach (string str in lineArray)
				if (str.Trim() != string.Empty)
					codeLines.Add(str);
		}

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            string staticString = (Name.StartsWith("override")) ? "" : "static ";

            b.AppendLine("\t\tpublic " + staticString + Name);
            b.AppendLine("\t\t{");

            foreach (string cl in codeLines)
                b.AppendLine("\t\t\t" + cl);

            b.AppendLine("\t\t}");
            return b.ToString();
        }
    }


    public class IfThenElseSequence
    {
        public List<IfBlock> IfBlocks { get { return ifBlocks; } }
        List<IfBlock> ifBlocks = new List<IfBlock>();
        public IfBlock ElseBlock { get; set; }
		public string Indent { get; set; }
        public string ErrorValue { get; set; }

		public IfThenElseSequence(string indent, string errorValue)
		{
			Indent = indent;
            ErrorValue = errorValue;
            Indent = string.Empty;
            ElseBlock = new IfBlock("");
            if(errorValue != null && errorValue != string.Empty)
                ElseBlock.CodeLines.Add("throw new Exception(\"Enum did not match! "+ errorValue +"'.\");");
            else
                ElseBlock.CodeLines.Add("throw new Exception(\"Enum did not match!\");");
		}

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
			List<IfBlock> tmp_ifblocks = new List<IfBlock>(ifBlocks);

			//Makes a list over the empty clauses to put them last (merged)
			List<IfBlock> emptyIfBlocks = new List<IfBlock>();
			foreach (IfBlock ifblock in ifBlocks)
				if (ifblock.CodeLines.Count == 0)
					emptyIfBlocks.Add(ifblock);
			//Deletes them from the other list and compresses them into the else-clause
			ElseBlock.Sentence = string.Empty;

			for( int i = 0; i<emptyIfBlocks.Count; i++)
			{
				IfBlock ifblock = emptyIfBlocks[i];
				tmp_ifblocks.Remove(ifblock);
				if (i != 0)
					ElseBlock.Sentence += " && ";
				ElseBlock.Sentence += "!(" + ifblock.Sentence + ")";
			}

			tmp_ifblocks.Add(ElseBlock);

			//Finally output the chain:
			for (int i = 0; i < tmp_ifblocks.Count; i++)
			{
                IfBlock ifblock = tmp_ifblocks[i];

                if (i != 0)
                {
                    b.AppendLine(Indent + "else");
                    ifblock.Indent = Indent + "\t";
                }
                else
                    ifblock.Indent = Indent;

				b.AppendLine(ifblock.ToString());
			}

            return b.ToString();
        }
    }

    public class IfBlock
    {
        public IfBlock(string sentence) { Sentence = sentence; }
        public string Sentence { get; set; }
        public List<string> CodeLines { get { return codeLines; } }
        List<string> codeLines = new List<string>();
		public string Indent { get; set; }

		public IfBlock()
		{
			Indent = string.Empty;
		}

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

			if (Sentence != null && Sentence != string.Empty)
				b.AppendLine(Indent + "if( " + Sentence + " )");

			if (codeLines.Count > 1)
				b.AppendLine(Indent + "{");

			foreach(string codeLine in codeLines)
				b.AppendLine(Indent + " \t" + codeLine);

			if (codeLines.Count > 1)
				b.AppendLine(Indent + "}");
            return b.ToString();
        }
    }

}
