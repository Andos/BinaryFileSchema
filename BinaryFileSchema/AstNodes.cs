using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

[assembly: CLSCompliant(true)]
namespace BFSchema
{
	public class BinaryFileSchema
	{
		public IList<IBfsDataBlock> DatablockList { get; set; }
		public Dictionary<string, IBfsDataBlock> DataBlocks { get; set; }
		public BfsByteOrder ByteOrder { get; set; }
		public IBfsDataBlock FormatBlock { get; set; }
		public BinaryFileSchema()
		{
			DatablockList = new List<IBfsDataBlock>();
			DataBlocks = new Dictionary<string, IBfsDataBlock>();
            ByteOrder = new BfsByteOrder();
            ByteOrder.ByteOrder = BfsByteOrderEnum.LanguageDefault;
		}
        public BinaryFileSchema(string filename, IBfsErrorHandler errorHandler) : this()
        {
            StreamReader reader = new StreamReader(filename);
            BfsCompiler.ParseBfs(this,reader.ReadToEnd(), errorHandler);
        }
	}

	public class BfsByteOrder : IBfsSourceNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsByteOrderEnum ByteOrder { get; set; }
	}

	public struct BfsSourceRange
	{
		public int Begin { get; set; }
		public int Length { get; set; }
		public override bool Equals(object obj)
		{
			if(!(obj is BfsSourceRange))
				return false;
			BfsSourceRange range = (BfsSourceRange)obj;
			return Equals(range);
		}
		public bool Equals(BfsSourceRange range)
		{
			return range.Begin == Begin && range.Length == Length;
		}
		public override int GetHashCode()
		{
			return Begin ^ Length;
		}
		public static bool operator ==(BfsSourceRange a, BfsSourceRange b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(BfsSourceRange a, BfsSourceRange b)
		{
			return !a.Equals(b);
		}
		public override string ToString()
		{
			return "Begin: " + Begin + " Length: " + Length;
		}
	}

	#region Interfaces

	public interface IBfsSourceNode
	{
		BfsSourceRange SourceRange { get; set; }
	}

	public interface IBfsDataBlock : IBfsSourceNode
	{
		string Name { get; set; }
		bool IsFormat { get; set; }
		Dictionary<string, BfsLocalField> LocalFields { get; set; }
		IList<BfsLocalField> LocalFieldList { get; set; }
		int SizeInBytes { get; set; }
		BfsSourceRange BlockTypeSourceRange { get; set; }
		BfsSourceRange FormatSourceRange { get; set; }
	}

	public interface IBfsStructType : IBfsDataBlock
	{
		Dictionary<string, BfsStructField> StructFields { get; set; }
		IList<BfsStructField> StructFieldList { get; set; }
        IList<IBfsNamedField> ParseOrder { get; set; }
        string CompressionMethod { get; set; }
        BfsSourceRange CompressionRange { get; set; }
        bool IsCompressed { get; set; } //If there is a chance that the structure is compressed (direct/indirect)
	}

	public interface IBfsConsumptionType : IBfsNamedField, IBfsSourceNode{}

	public interface IBfsType : IBfsSourceNode
	{
		IBfsArray ArrayExtension { get; set; }
	}

	public interface IBfsEnumMember  : IBfsSourceNode{ }
	public interface IBfsAction  : IBfsSourceNode{ }

	public interface IBfsNamedField : IBfsSourceNode
	{
		string Name { get; set; }
		BfsPrimitiveType PrimitiveType { get; set; }
	}

	public interface IBfsArray  : IBfsSourceNode{ }

	public interface IBfsExpNode : IBfsSourceNode { }

	#endregion
	#region Enums
	public enum BfsPrimitiveTypeEnum
	{
		CallExpression = -2,
		EnumMember = -1,
		Undetermined = 0,
		Bool = 1,
		Ubyte = 2,
		Sbyte = 3,
		Ushort = 4,
		Short = 5,
		Uint = 6,
		Int = 7,
		Ulong = 8,
		Long = 9
	}
	public enum BfsByteOrderEnum { LittleEndian, BigEndian, LanguageDefault }
	public enum BfsInclusionEnum { Included, Excluded, Skipped }
	#endregion
	#region Datablocks
	public class BfsStruct : IBfsStructType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public bool IsFormat { get; set; }
		public Dictionary<string,BfsLocalField> LocalFields { get; set; }
		public Dictionary<string, BfsStructField> StructFields { get; set; }
		public IList<BfsLocalField> LocalFieldList { get; set; }
		public IList<BfsStructField> StructFieldList { get; set; }
		public BfsSourceRange BlockTypeSourceRange { get; set; }
		public BfsSourceRange FormatSourceRange { get; set; }
        public IList<IBfsNamedField> ParseOrder { get; set; }
        public string CompressionMethod { get; set; }
        public BfsSourceRange CompressionRange { get; set; }
        public bool IsCompressed { get; set; }
		public int SizeInBytes { get; set; }
		public BfsStruct()
		{
			LocalFields = new Dictionary<string,BfsLocalField>();
			StructFields = new Dictionary<string, BfsStructField>();
			LocalFieldList = new List<BfsLocalField>();
			StructFieldList = new List<BfsStructField>();
            ParseOrder = new List<IBfsNamedField>();
		}
		public override string ToString()
		{
			return "Structure: " + Name;
		}
	}

	public class BfsAbsOffset : IBfsStructType, IBfsConsumptionType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public bool IsFormat { get; set; }
		public Dictionary<string, BfsLocalField> LocalFields { get; set; }
		public Dictionary<string, BfsStructField> StructFields { get; set; }
		public IList<BfsLocalField> LocalFieldList { get; set; }
		public IList<BfsStructField> StructFieldList { get; set; }
		public BfsPrimitiveType PrimitiveType { get; set; }
		public BfsSourceRange BlockTypeSourceRange { get; set; }
		public BfsSourceRange FormatSourceRange { get; set; }
        public IList<IBfsNamedField> ParseOrder { get; set; }
        public string CompressionMethod { get; set; }
        public BfsSourceRange CompressionRange { get; set; }
        public bool IsCompressed { get; set; }
		public int SizeInBytes { get; set; }
		public BfsAbsOffset()
		{
			LocalFields = new Dictionary<string, BfsLocalField>();
			StructFields = new Dictionary<string, BfsStructField>();
			LocalFieldList = new List<BfsLocalField>();
			StructFieldList = new List<BfsStructField>();
            ParseOrder = new List<IBfsNamedField>();
		}
		public override string ToString()
		{
			return "Absolute offset: " + Name;
		}
	}

	public class BfsRelOffset : IBfsStructType, IBfsConsumptionType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public bool IsFormat { get; set; }
		public Dictionary<string, BfsLocalField> LocalFields { get; set; }
		public Dictionary<string, BfsStructField> StructFields { get; set; }
		public IList<BfsLocalField> LocalFieldList { get; set; }
		public IList<BfsStructField> StructFieldList { get; set; }
		public BfsPrimitiveType PrimitiveType { get; set; }
		public BfsSourceRange BlockTypeSourceRange { get; set; }
		public BfsSourceRange FormatSourceRange { get; set; }
        public IList<IBfsNamedField> ParseOrder { get; set; }
        public string CompressionMethod { get; set; }
        public BfsSourceRange CompressionRange { get; set; }
        public bool IsCompressed { get; set; }
		public int SizeInBytes { get; set; }
		public BfsRelOffset()
		{
			LocalFields = new Dictionary<string, BfsLocalField>();
			StructFields = new Dictionary<string, BfsStructField>();
			LocalFieldList = new List<BfsLocalField>();
			StructFieldList = new List<BfsStructField>();
            ParseOrder = new List<IBfsNamedField>();
		}
		public override string ToString()
		{
			return "Relative offset: " + Name;
		}
	}

	public class BfsEnum : IBfsDataBlock, IBfsConsumptionType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public bool IsFormat { get; set; }
		public BfsPrimitiveType PrimitiveType { get; set; }
		public Dictionary<string, BfsLocalField> LocalFields { get; set; }
		public Dictionary<string, BfsEnumFieldAlias> EnumAliases { get; set; }
		public IList<BfsLocalField> LocalFieldList { get; set; }
		public IList<BfsEnumField> EnumFields { get; set; }
		public BfsSourceRange BlockTypeSourceRange { get; set; }
		public BfsSourceRange FormatSourceRange { get; set; }
		public int SizeInBytes { get; set; }
		public BfsEnum()
		{
			LocalFields = new Dictionary<string, BfsLocalField>();
			LocalFieldList = new List<BfsLocalField>();
			EnumFields = new List<BfsEnumField>();
			EnumAliases = new Dictionary<string,BfsEnumFieldAlias>();
		}
		public override string ToString()
		{
			return "Enum: " + Name;
		}		
	}

	public class BfsBitfield : IBfsDataBlock, IBfsConsumptionType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public bool IsFormat{ get; set; }
		public Dictionary<string, BfsLocalField> LocalFields { get; set; }
		public IList<BfsLocalField> LocalFieldList { get; set; }
		public IList<BfsBitfieldField> BitFieldFields { get; set; }
		public BfsPrimitiveType PrimitiveType { get; set; }
		public BfsSourceRange BlockTypeSourceRange { get; set; }
		public BfsSourceRange FormatSourceRange { get; set; }
		public int SizeInBytes { get; set; }
		public BfsBitfield()
		{
			LocalFields = new Dictionary<string, BfsLocalField>();
			LocalFieldList = new List<BfsLocalField>();
			BitFieldFields = new List<BfsBitfieldField>();
		}
		public override string ToString()
		{
			return "Bitfield: " + Name;
		}
	}
	#endregion
	#region Types
	public class BfsUnresolvedNamedType : IBfsType
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public IBfsArray ArrayExtension { get; set; }
		public override string ToString()
		{
			return "unresolved named type: " + Name;
		}
	}

	public class BfsNamedType : IBfsType
	{
		public BfsSourceRange SourceRange { get; set; }
		public IBfsDataBlock DataBlock { get; set; }
		public IBfsArray ArrayExtension {get; set;}
		public override string ToString()
		{
			return "named type: " + DataBlock.Name;
		}
	}

	public class BfsPrimitiveType : IBfsType
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsPrimitiveTypeEnum PrimitiveType { get; set; }
		public IBfsArray ArrayExtension { get; set; }
		public override string ToString()
		{
			return "primitive type: " + PrimitiveType;
		}
	}

	public class BfsFunctionType : IBfsType
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsSourceRange ArgumentSourceRange { get; set; }
		public string FunctionName { get; set; }
		public string FunctionArgument { get; set; }
		public IBfsArray ArrayExtension { get; set; }
		public override string ToString()
		{
			return "function type: " + FunctionName + "(" + FunctionArgument + ")";
		}
	}

	public class BfsKnownArray : IBfsArray
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsExpression Expression { get; set; }
	}

	public class BfsUnknownArray : IBfsArray
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsSourceRange UntilSourceRange { get; set; }
		public IList<IBfsStopCase> StopCases { get; set; }
		public IList<BfsSourceRange> OrWords {get; set;}
		public BfsUnknownArray()
		{
			StopCases = new List<IBfsStopCase>();
			OrWords = new List<BfsSourceRange>();
		}
	}

	public interface IBfsStopCase : IBfsSourceNode
	{
		BfsInclusionEnum Inclusion { get; set; }
		BfsSourceRange InclusionSourceRange { get; set; }
	}

	public class BfsStopCaseString : IBfsStopCase
	{
		public BfsSourceRange SourceRange { get; set; }
		public string StopString { get; set; }
		public BfsInclusionEnum Inclusion { get; set; }
		public BfsSourceRange InclusionSourceRange { get; set; }
		public BfsSourceRange ConsumptionSourceRange { get; set; }
	}

	public class BfsStopCaseHex : IBfsStopCase
	{
		public BfsSourceRange SourceRange { get; set; }
		public string HexString { get; set; }
		public BfsInclusionEnum Inclusion { get; set; }
		public BfsSourceRange InclusionSourceRange { get; set; }
		public BfsSourceRange ConsumptionSourceRange { get; set; }
	}

	public class BfsStopCaseEndOfFile : IBfsStopCase
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsInclusionEnum Inclusion { get; set; }
		public BfsSourceRange InclusionSourceRange { get; set; }
		public BfsSourceRange ConsumptionSourceRange { get; set; }
	}

	#endregion
	#region Datablock members
	public class BfsLocalField : IBfsNamedField
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public BfsPrimitiveType PrimitiveType { get; set; }
		public BfsExpression AssignmentExpression { get; set; }
        public override string ToString()
        {
            return Name;
        }
	}

	public class BfsStructField : IBfsNamedField
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Name { get; set; }
		public IBfsType FieldType { get; set; }
		public bool Skip { get; set; }
		public BfsPrimitiveType PrimitiveType
		{
			get
			{
                if (FieldType is BfsPrimitiveType)
                    return FieldType as BfsPrimitiveType;
                else if (FieldType is BfsNamedType && (FieldType as BfsNamedType).DataBlock is IBfsConsumptionType)
                    return ((FieldType as BfsNamedType).DataBlock as IBfsConsumptionType).PrimitiveType;
                //else if (FieldType is BfsFunctionType)
                //    return new BfsPrimitiveType { PrimitiveType = BfsPrimitiveTypeEnum.CallExpression }; TODO

                //Otherwise the type is undetermined
				return new BfsPrimitiveType(){ PrimitiveType = BfsPrimitiveTypeEnum.Undetermined };
			}
			set { }
		}
		public BfsExpression Conditional { get; set; }
		public BfsSourceRange ConditionalSourceRange { get; set; }
		public BfsSourceRange SkipSourceRange { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}

	public class BfsEnumField : IBfsSourceNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public IBfsEnumMember EnumMatch { get; set; }
		public string Alias { get; set; }
		public BfsSourceRange AliasSourceRange { get; set; }
		public IList<IBfsAction> Actions { get; set; }
		public BfsEnumField()
		{
			Actions = new List<IBfsAction>();
		}
		public override string ToString()
		{
			return ((Alias != null) ? Alias : "<none>") + "    " + EnumMatch.ToString();
		}
	}

	public class BfsEnumFieldAlias
	{
		public string Name { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}

	public class BfsBitfieldField : IBfsSourceNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public long BitNumber { get; set; }
        public string Name { get; set; }
		public IList<IBfsAction> Actions { get; set; }
		public BfsBitfieldField()
		{
			Actions = new List<IBfsAction>();
		}
		public override string ToString()
		{
			return "Bit: " + BitNumber + ((Name != null) ? Name : "");
		}
	}
	#endregion
	#region Expression
	public class BfsExpression : IBfsSourceNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsPrimitiveTypeEnum PrimitiveType { get { return ExpressionGroup.PrimitiveType; } }
		public BfsExpGroup ExpressionGroup { get; set; }
		public HashSet<BfsExpressionVariable> DependantVariables {get; set; }
		public BfsExpression()
		{
			DependantVariables = new HashSet<BfsExpressionVariable>();
		}
		public override string ToString()
		{
			return ExpressionGroup.ToString();
		}
	}
	
	public class BfsExpGroup : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsPrimitiveTypeEnum PrimitiveType { get; set; }
		public IList<IBfsExpNode> Members { get; set; }
		public BfsExpGroupOperatorLevel OperatorPrecedenceLevel {get;set;}
		public BfsExpGroup()
		{
			Members = new List<IBfsExpNode>();
		}
		public override string ToString()
		{
			string res = string.Empty;
			foreach (IBfsExpNode node in Members)
				res += node.ToString();
			return "(" + res + ")";
		}
	}

	public enum BfsExpGroupOperatorLevel
	{ 
		Undetermined,
		Logical,
		Bitwise,
		Comparisson,
		Shift,
		Sum,
		Product
	}

	public class BfsExpressionVariable : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public IList<IBfsNamedField> NameHierarchy { get; set; }
		public IBfsNamedField LastField {
            get { return NameHierarchy[NameHierarchy.Count - 1]; }
            set { NameHierarchy[NameHierarchy.Count - 1] = value; }
        }
		public BfsPrimitiveTypeEnum PrimitiveType { get { return LastField.PrimitiveType.PrimitiveType;} }
		public BfsExpressionVariable()
		{
			NameHierarchy = new List<IBfsNamedField>();
		}
		public override string ToString()
		{
			string res = "";
			foreach (IBfsNamedField f in NameHierarchy)
				res += f.Name + ".";
			return res.TrimEnd(".".ToCharArray());
		}
        public override bool Equals(object obj)
        {
            if (!(obj is BfsExpressionVariable))
                return false;

            BfsExpressionVariable other = obj as BfsExpressionVariable;
            if (this.NameHierarchy.Count != other.NameHierarchy.Count)
                return false;

            for (int i = 0; i < this.NameHierarchy.Count; i++)
                if (this.NameHierarchy[i].Name != other.NameHierarchy[i].Name ||
                    this.NameHierarchy[i].PrimitiveType.PrimitiveType != other.NameHierarchy[i].PrimitiveType.PrimitiveType)
                    return false;

                return true;
        }
        public override int GetHashCode()
        {
            int hashcode = 0;
            for (int i = 0; i < this.NameHierarchy.Count; i++)
            {
                hashcode += NameHierarchy[i].Name.GetHashCode();
                hashcode += NameHierarchy[i].PrimitiveType.PrimitiveType.GetHashCode();
            }
            return hashcode;
        }
	}

	//This will be replaced by a BfsExpressionVariable in the AST conversion.
	public class BfsUnresolvedVariableExp : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public IList<string> NameHierarchy { get; set; }
		public string LastField { get { return NameHierarchy[NameHierarchy.Count - 1]; } }
		public BfsUnresolvedVariableExp()
		{
			NameHierarchy = new List<string>();
		}
		public override string ToString()
		{
			string res = "";
			foreach (string s in NameHierarchy)
				res += s + ".";
			return res.TrimEnd(".".ToCharArray());
		}
	}

	public class BfsEnumAliasExp : IBfsNamedField, IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsEnumFieldAlias Alias { get; set; }
        public BfsEnum EnumBlock { get; set; }
		public string Name { get { return Alias.Name; } set { Alias.Name = value; } }
		public BfsPrimitiveType PrimitiveType
		{
			get { return new BfsPrimitiveType() { PrimitiveType = BfsPrimitiveTypeEnum.Undetermined }; }
			set { }
		}

	}

	//A potential BfsEnumAliasExp which must be replaced. Otherwise no variable exists with that name.
	public class BfsExpressionUnknown : IBfsNamedField, IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsPrimitiveType PrimitiveType
		{
			get
			{
				BfsPrimitiveType type = new BfsPrimitiveType();
				type.PrimitiveType = BfsPrimitiveTypeEnum.Undetermined;
				return type;
			}
			set { }
		}
		public string Name { get; set; }
	}


	public class BfsNumberExp : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public long Value { get; set; }
		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
	}
	public class BfsBooleanExp : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public bool Value { get; set; }
		public override string ToString()
		{
			return Value ? "true" : "false";
		}
	}
	public class BfsValueExp : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
	}
	public class BfsCallExp : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsSourceRange NameSourceRange { get; set; }
		public BfsSourceRange ArgumentSourceRange { get; set; }
		public int SizeInBytes{ get; set; }
		public string FunctionName { get; set; }
		public string FunctionArgument { get; set; }
		public override string ToString()
		{
			return FunctionName + "(" + FunctionArgument + ")";
		}
	}
	public class BfsOperator : IBfsExpNode
	{
		public BfsSourceRange SourceRange { get; set; }
		public string Operator { get; set; }
		public BfsExpGroupOperatorLevel PrecendenceLevel
		{
			get
			{
				List<string> logical = new List<string>(new string[] { "&&", "||" });
				List<string> bitwise = new List<string>(new string[] { "^", "&", "|" });
				List<string> comp = new List<string>(new string[] { "!=", "==", "<=", ">=", "<", ">" });
				List<string> shift = new List<string>(new string[] { ">>", "<<" });
				List<string> sum = new List<string>(new string[] { "+", "-" });
				List<string> prod = new List<string>(new string[] { "*", "/", "%" });
				if( sum.Contains( Operator ))
					return BfsExpGroupOperatorLevel.Sum;
				if (prod.Contains(Operator))
					return BfsExpGroupOperatorLevel.Product;
				if (comp.Contains(Operator))
					return BfsExpGroupOperatorLevel.Comparisson;
				if (logical.Contains(Operator))
					return BfsExpGroupOperatorLevel.Logical;
				if (bitwise.Contains(Operator))
					return BfsExpGroupOperatorLevel.Bitwise;
				if (shift.Contains(Operator))
					return BfsExpGroupOperatorLevel.Shift;
				else return BfsExpGroupOperatorLevel.Undetermined;
			}
		}
		public override string ToString()
		{
			return Operator;
		}
	}
	#endregion
	#region Enum stuff
	public class BfsEnumValue : IBfsEnumMember
	{
		public BfsSourceRange SourceRange { get; set; }
		public long Value {get; set;}
		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
	}

	public class BfsEnumRange : IBfsEnumMember
	{
		public BfsSourceRange SourceRange { get; set; }
		public long StartValue { get; set; }
		public BfsInclusionEnum StartInclusion { get; set; }
		public long EndValue { get; set; }
		public BfsInclusionEnum EndInclusion { get; set; }
		public override string ToString()
		{
			string ret = ((StartInclusion == BfsInclusionEnum.Included) ? "[" : "]");
			ret += StartValue + ".." + EndValue;
			ret += ((EndInclusion == BfsInclusionEnum.Included) ? "]" : "[");
			return ret;
		}
	}

	public class BfsEnumElse : IBfsEnumMember
	{
		public BfsSourceRange SourceRange { get; set; }
		public override string ToString()
		{
			return "else";
		}
	}
	#endregion
	#region Actions
	public class BfsActionOutput : IBfsAction
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsSourceRange ArgumentSourceRange { get; set; }
		public string FunctionName { get; set; }
		public string Argument { get; set; }
	}

	public class BfsActionUnresolvedAssignment : IBfsAction
	{
		public BfsSourceRange SourceRange { get; set; }
		public string UnresolvedVariableName { get; set; }
		public BfsExpression Expression { get; set; }
	}

	public class BfsActionAssignment : IBfsAction
	{
		public BfsSourceRange SourceRange { get; set; }
		public BfsLocalField LocalVariable { get; set; }
		public BfsExpression Expression { get; set; }
	}
	#endregion


}
