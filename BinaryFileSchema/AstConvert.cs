using System;
using System.Collections.Generic;
using System.Globalization;
using BinaryFileSchemaParser;
using Peg.Base;
using System.Runtime.Serialization;

namespace BFSchema
{
	public class AstConvert
	{
		BinaryFileSchema schema;
		string source;

		public static EBinaryFileSchemaParser GetNodeId(PegNode node)
		{
			return (EBinaryFileSchemaParser)node.id_;
		}
		public string GetNodeText(PegNode node)
		{
			return source.Substring(node.match_.posBeg_, node.match_.Length).Trim();
		}
		public AstConvert(BinaryFileSchema schema, string source)
		{
            this.schema = schema;
			this.source = source;
		}
        public AstConvert(string source)
        {
            this.schema = new BinaryFileSchema();
            this.source = source;
        }

		public BinaryFileSchema GetBFSTree(PegNode rootnode)
		{
			//First pass below root. Expecting datablocks and byteOrder fields.
			PegNode node = rootnode.child_;
			do
			{
				PegNode field = node;
				EBinaryFileSchemaParser field_id = GetNodeId(field);
				bool isformat = false;
				BfsSourceRange formatrange = GetSourceRange(field);
				BfsSourceRange blocktyperange;

				//BYTEORDER
				if (GetNodeId(field) == EBinaryFileSchemaParser.byteorder)
				{
					BfsByteOrder byteorder = new BfsByteOrder();
					StoreSourceRange(field, byteorder);
					if ( GetNodeId(field.child_) == EBinaryFileSchemaParser.littleendian)
						byteorder.ByteOrder = BfsByteOrderEnum.LittleEndian;
					else if (GetNodeId(field.child_) == EBinaryFileSchemaParser.bigendian)
						byteorder.ByteOrder = BfsByteOrderEnum.BigEndian;
					else
						byteorder.ByteOrder = BfsByteOrderEnum.LanguageDefault;
					schema.ByteOrder = byteorder;
				}
				else
				{
					PegNode block_content = field.child_;

					//If the first node is a 'format' flag, go to next sibling
					if (GetNodeId(block_content) == EBinaryFileSchemaParser.formatspecifier)
					{
						isformat = true;
						formatrange = GetSourceRange(block_content);
						block_content = block_content.next_;
					}

					blocktyperange = GetSourceRange(block_content);
					block_content = block_content.next_;

					IBfsDataBlock block;
					switch (field_id)
					{
						//STRUCT
						case EBinaryFileSchemaParser.p_struct:
							block = new BfsStruct();
							StoreSourceRange(node, block);
							block.IsFormat = isformat;
							ConvertStructType(block_content, block as IBfsStructType);
							schema.DatablockList.Add(block);
							break;

						//ABS_OFFSET
						case EBinaryFileSchemaParser.abs_offset:
							block = new BfsAbsOffset();
							StoreSourceRange(node, block);
							block.IsFormat = isformat;
							ConvertStructType(block_content, block as IBfsStructType);
							schema.DatablockList.Add(block);
							break;

						//REL_OFFSET
						case EBinaryFileSchemaParser.rel_offset:
							block = new BfsRelOffset();
							StoreSourceRange(node, block);
							block.IsFormat = isformat;
							ConvertStructType(block_content, block as IBfsStructType);
							schema.DatablockList.Add(block);
							break;

						//ENUM
						case EBinaryFileSchemaParser.p_enum:
							block = new BfsEnum();
							StoreSourceRange(node, block);
							block.IsFormat = isformat;
							ConvertEnumType(block_content, block as BfsEnum);
							schema.DatablockList.Add(block);
							break;

						//BITFIELD
						case EBinaryFileSchemaParser.bitfield:
							block = new BfsBitfield();
							StoreSourceRange(node, block);
							block.BlockTypeSourceRange = GetSourceRange(field);
							block.IsFormat = isformat;
							ConvertBitfieldType(block_content, block as BfsBitfield);
							schema.DatablockList.Add(block);
							break;

						default:
							throw new AstConvertException("Not a data-block: " + GetNodeId(block_content));
					}

					block.BlockTypeSourceRange = blocktyperange;
					if (isformat)
						block.FormatSourceRange = formatrange;
				}
				
			}
			while ((node = node.next_) != null);

			return schema;
		}

		private static void StoreSourceRange(PegNode node, IBfsSourceNode source_node)
		{
			BfsSourceRange range = new BfsSourceRange();
			range.Begin = node.match_.posBeg_;
			range.Length = node.match_.Length;
			source_node.SourceRange = range;
		}

		private static BfsSourceRange GetSourceRange(PegNode node)
		{
			BfsSourceRange range = new BfsSourceRange();
			range.Begin = node.match_.posBeg_;
			range.Length = node.match_.Length;
			return range;
		}

		private void ConvertLocalField(PegNode node, IBfsDataBlock block)
		{
			BfsLocalField localfield = new BfsLocalField();
			StoreSourceRange(node, localfield);
			localfield.Name = GetNodeText(node.child_);
			localfield.PrimitiveType = ConvertPrimitiveType(node.child_.next_);
			
			node = node.child_.next_.next_;
			if (node != null && GetNodeId(node) == EBinaryFileSchemaParser.expression)
				localfield.AssignmentExpression = ConvertExpression(node);

			block.LocalFieldList.Add(localfield);
		}

		public void ConvertStructType(PegNode blocknamenode, IBfsStructType block)
		{
			if (block == null)
				throw new AstConvertException("Type wasn't struct-like");

            block.SourceRange = GetSourceRange(blocknamenode);
            block.Name = GetNodeText(blocknamenode);

            PegNode nextNode = blocknamenode.next_;

            //If the structure has a Compression Method defined
            if ( nextNode != null && GetNodeId(nextNode) == EBinaryFileSchemaParser.compressionmethod)
            {
                block.CompressionMethod = GetNodeText(nextNode);
                block.CompressionRange = GetSourceRange(nextNode);
                nextNode = nextNode.next_;
            }

            //If a primitive type is present, then it is a comsumeable struct type
            if (nextNode != null && GetNodeId(nextNode) == EBinaryFileSchemaParser.primitivetype)
			{
				IBfsConsumptionType special = block as IBfsConsumptionType;
                special.PrimitiveType = ConvertPrimitiveType(nextNode);
                nextNode = nextNode.next_;
			}

			//For each of the fields the struct-type contains
            for(PegNode field = nextNode; field != null; field = field.next_)
			{
				if (GetNodeId(field) == EBinaryFileSchemaParser.field)
				{
					BfsStructField fielditem = new BfsStructField();
					fielditem.SourceRange = GetSourceRange(field);
					ConvertStructField(field, fielditem);
					block.StructFieldList.Add(fielditem);
				}
				else
					if (GetNodeId(field) == EBinaryFileSchemaParser.localfield)
						ConvertLocalField(field, block);
					else
						throw new AstConvertException("Node wasn't a field: " + GetNodeId(field));
			}
		}

		private void ConvertStructField(PegNode fieldNode, BfsStructField field)
		{
			PegNode node = fieldNode.child_;

			if (GetNodeId(node) == EBinaryFileSchemaParser.skip)
			{
				field.Skip = true;
				field.SkipSourceRange = GetSourceRange(node);
				node = node.next_;
			}

			if (GetNodeId(node) == EBinaryFileSchemaParser.conditional)
			{
				field.Conditional = ConvertExpression(node.child_);
				field.ConditionalSourceRange = GetSourceRange(node);
				node = node.next_;
			}

			if (GetNodeId(node) == EBinaryFileSchemaParser.fieldname)
			{
				field.Name = GetNodeText(node);
				node = node.next_;
			}
			//else throw new AstConvertException("Expected a fieldname node");

			if (GetNodeId(node) == EBinaryFileSchemaParser.type)
				field.FieldType = ConvertType(node);
                
			//else throw new AstConvertException("Type expected");
		}

		private BfsPrimitiveType ConvertPrimitiveType( PegNode node )
		{
			string name = GetNodeText(node);
			BfsPrimitiveType type = new BfsPrimitiveType();
			type.SourceRange = GetSourceRange(node);

			switch (name)
			{
				case "bool": type.PrimitiveType = BfsPrimitiveTypeEnum.Bool; break;
				case "sbyte": type.PrimitiveType = BfsPrimitiveTypeEnum.Sbyte; break;
				case "ubyte": type.PrimitiveType = BfsPrimitiveTypeEnum.Ubyte; break;
				case "short": type.PrimitiveType = BfsPrimitiveTypeEnum.Short; break;
				case "ushort": type.PrimitiveType = BfsPrimitiveTypeEnum.Ushort; break;
				case "int": type.PrimitiveType = BfsPrimitiveTypeEnum.Int; break;
				case "uint": type.PrimitiveType = BfsPrimitiveTypeEnum.Uint; break;
				case "long": type.PrimitiveType = BfsPrimitiveTypeEnum.Long; break;
				case "ulong": type.PrimitiveType = BfsPrimitiveTypeEnum.Ulong; break;
				default: type.PrimitiveType = BfsPrimitiveTypeEnum.Undetermined; break;
			}
			return type;
		}

		private IBfsType ConvertType(PegNode node)
		{
			node = node.child_;
			IBfsType type = null;

			switch (GetNodeId(node))
			{ 
				case EBinaryFileSchemaParser.primitivetype:
					type = ConvertPrimitiveType(node);
					break;

				case EBinaryFileSchemaParser.namedtype:
					BfsUnresolvedNamedType namedtype = new BfsUnresolvedNamedType();
                    namedtype.SourceRange = GetSourceRange(node);
					namedtype.Name = GetNodeText(node);
					type = namedtype;
					break;

				case EBinaryFileSchemaParser.functiontype:
					BfsFunctionType functiontype = new BfsFunctionType();
                    functiontype.SourceRange = GetSourceRange(node);
					functiontype.ArgumentSourceRange = GetSourceRange(node.child_.next_);
					functiontype.FunctionName = GetNodeText(node.child_);
					functiontype.FunctionArgument = GetNodeText(node.child_.next_);
                    functiontype.FunctionArgument = functiontype.FunctionArgument.Substring(1, functiontype.FunctionArgument.Length - 2);
					type = functiontype;
					break;
			}

			//Array extension?
			if (node.next_ != null )
				type.ArrayExtension = ConvertArrayExtension(node.next_);

			return type;
		}

		private IBfsArray ConvertArrayExtension(PegNode node)
		{
			EBinaryFileSchemaParser type = GetNodeId(node);

			if (type != EBinaryFileSchemaParser.arrayknown && type != EBinaryFileSchemaParser.arrayunknown)
				throw new AstConvertException("Must be an array extension! : " + GetNodeText(node));

			//Known sized array extensions
			if (type == EBinaryFileSchemaParser.arrayknown)
			{
				BfsKnownArray knownarray = new BfsKnownArray();
				StoreSourceRange(node, knownarray);
				knownarray.Expression = ConvertExpression(node.child_);

				return knownarray;
			}
			//Unknown sized
			else
			{
				BfsUnknownArray unknownarray = new BfsUnknownArray();
				StoreSourceRange(node, unknownarray);
				unknownarray.UntilSourceRange = GetSourceRange(node.child_);
				for (PegNode stopcasenode = node.child_.next_; stopcasenode != null; stopcasenode = stopcasenode.next_)
				{
					IBfsStopCase stopcase = null;

					if (GetNodeId(stopcasenode) == EBinaryFileSchemaParser.stopcase)
					{
						switch( GetNodeId(stopcasenode.child_) )
						{
							case EBinaryFileSchemaParser.p_string:
								BfsStopCaseString stopstring = new BfsStopCaseString();
								stopstring.StopString = GetNodeText(stopcasenode.child_);
								stopcase = stopstring;
								break;
							case EBinaryFileSchemaParser.EOF:
								stopcase = new BfsStopCaseEndOfFile();
								break;
							case EBinaryFileSchemaParser.hex:
								BfsStopCaseHex stophex = new BfsStopCaseHex();
								stophex.HexString = GetNodeText(stopcasenode.child_);
								stopcase = stophex;
								break;
							default:
								throw new Exception("Unknown stopcase type : " + GetNodeId(stopcasenode.child_));
						}
                        stopcase.Inclusion = BfsInclusionEnum.Skipped;
						stopcase.SourceRange = GetSourceRange(stopcasenode.child_);

						//Inclusion and consumption
						for (PegNode bnode = stopcasenode.child_.next_; bnode != null; bnode = bnode.next_)
						{
							if (GetNodeId(bnode) == EBinaryFileSchemaParser.inclusion)
							{
								stopcase.InclusionSourceRange = GetSourceRange(bnode);

								if (GetNodeText(bnode) == "included")
									stopcase.Inclusion = BfsInclusionEnum.Included;
                                else if (GetNodeText(bnode) == "excluded")
									stopcase.Inclusion = BfsInclusionEnum.Excluded;
                                else if (GetNodeText(bnode) == "skipped")
                                    stopcase.Inclusion = BfsInclusionEnum.Skipped;
							}
						}
						unknownarray.StopCases.Add(stopcase);
					}
					else if (GetNodeId(stopcasenode) == EBinaryFileSchemaParser.or_keyword)
						unknownarray.OrWords.Add(GetSourceRange(stopcasenode));
				}

				return unknownarray;
			}
		}

		private BfsExpression ConvertExpression(PegNode node)
		{
			BfsExpression exp = new BfsExpression();
			exp.ExpressionGroup = new BfsExpGroup();
			exp.SourceRange = GetSourceRange(node);
			exp.ExpressionGroup.SourceRange = exp.SourceRange;
			ConvertExpNodes(exp.ExpressionGroup, node);
			return exp;
		}

		private void ConvertExpNodes( BfsExpGroup group, PegNode node )
		{
			HashSet<EBinaryFileSchemaParser> groupnodes = new HashSet<EBinaryFileSchemaParser>(
				new EBinaryFileSchemaParser[] { EBinaryFileSchemaParser.logical, EBinaryFileSchemaParser.bitwise,
					EBinaryFileSchemaParser.comp, EBinaryFileSchemaParser.shift, EBinaryFileSchemaParser.sum, EBinaryFileSchemaParser.prod });

			for (PegNode startnode = node.child_; startnode != null; startnode = startnode.next_)
			{
				//If the node is an operator: logical, bitwise, comparisson, bitshift, sum or product
				if (groupnodes.Contains(GetNodeId(startnode)))
				{
					//Recursively generate subgroups
					BfsExpGroup subgroup = new BfsExpGroup();
					subgroup.SourceRange = GetSourceRange(startnode);
					ConvertExpNodes(subgroup, startnode);
					group.Members.Add(subgroup);
				}
				else
				{
					switch (GetNodeId(startnode))
					{
						case EBinaryFileSchemaParser.number:
							BfsNumberExp number = new BfsNumberExp();
							StoreSourceRange(startnode, number);
							number.Value = long.Parse( GetNodeText(startnode), CultureInfo.InvariantCulture );
							group.Members.Add( number );
							break;
						case EBinaryFileSchemaParser.named_value:
							BfsUnresolvedVariableExp var = new BfsUnresolvedVariableExp();
							StoreSourceRange(startnode, var);
							for (PegNode v = startnode.child_; v != null; v = v.next_)
								var.NameHierarchy.Add(GetNodeText(v));
							group.Members.Add(var);
							break;
						case EBinaryFileSchemaParser.call:
							BfsCallExp call = new BfsCallExp();
							call.SourceRange = GetSourceRange(startnode);
							call.FunctionName = GetNodeText(startnode.child_);
							call.NameSourceRange = GetSourceRange(startnode.child_);
							call.FunctionArgument = GetNodeText(startnode.child_.next_);
							call.ArgumentSourceRange = GetSourceRange(startnode.child_.next_);
							group.Members.Add(call);
							break;
						//Operators
						default:
							BfsOperator op = new BfsOperator();
							StoreSourceRange(startnode, op);
							op.Operator = GetNodeText(startnode);
							group.Members.Add(op);
							break;
					}
				}
			}
		}

		private void ConvertEnumType(PegNode node, BfsEnum block)
		{
			if (block == null)
				throw new AstConvertException("Type wasn't an enum");

			StoreSourceRange(node, block);

			block.Name = GetNodeText(node);
			block.PrimitiveType = ConvertPrimitiveType(node.next_);
			
			//For all enum fields contained in the enum (values,ranges and 'else')
			for (PegNode enumfield = node.next_.next_; enumfield != null; enumfield = enumfield.next_)
			{
				if (GetNodeId(enumfield) == EBinaryFileSchemaParser.enumfield)
					ConvertEnumField(enumfield, block);
				else if (GetNodeId(enumfield) == EBinaryFileSchemaParser.localfield)
					ConvertLocalField(enumfield, block);
				else
					throw new AstConvertException("Not an enum field!");
			}
		}

		private void ConvertEnumField(PegNode node, BfsEnum block)
		{
			BfsEnumField field = new BfsEnumField();
			StoreSourceRange(node, field);
			for (PegNode enode = node.child_; enode != null; enode = enode.next_)
			{
				switch (GetNodeId(enode))
				{
					case EBinaryFileSchemaParser.number:
						BfsEnumValue enumval = new BfsEnumValue();
						StoreSourceRange(enode, enumval);
						enumval.Value = long.Parse(GetNodeText(enode), CultureInfo.InvariantCulture);
						field.EnumMatch = enumval;
						break;

					case EBinaryFileSchemaParser.enumrange:
						BfsEnumRange enumrange = new BfsEnumRange();
						StoreSourceRange(enode, enumrange);
						enumrange.StartValue = long.Parse(GetNodeText(enode.child_.next_), CultureInfo.InvariantCulture);
						enumrange.EndValue = long.Parse(GetNodeText(enode.child_.next_.next_), CultureInfo.InvariantCulture);
						enumrange.StartInclusion =
							(GetNodeText(enode.child_) == "[") ? BfsInclusionEnum.Included : BfsInclusionEnum.Excluded;
						enumrange.EndInclusion =
							(GetNodeText(enode.child_.next_.next_.next_) == "]") ? BfsInclusionEnum.Included : BfsInclusionEnum.Excluded;
						field.EnumMatch = enumrange;
						break;

					case EBinaryFileSchemaParser.p_else:
						field.EnumMatch = new BfsEnumElse();
						StoreSourceRange(enode, field.EnumMatch);
						break;

					case EBinaryFileSchemaParser.action_list:
						ConvertActionList(enode, field.Actions);
						break;

					case EBinaryFileSchemaParser.enumname:
						field.Alias = GetNodeText(enode);
						field.AliasSourceRange = GetSourceRange(enode);
						break;
				}
			}
			block.EnumFields.Add(field);
		}

		private void ConvertActionList(PegNode node, IList<IBfsAction> actions)
		{
			if (GetNodeId(node) != EBinaryFileSchemaParser.action_list)
				throw new AstConvertException("Wasn't an action list node!");

			for (node = node.child_; node != null; node = node.next_)
			{ 
				switch(GetNodeId(node))
				{
					case EBinaryFileSchemaParser.output:
						BfsActionOutput output = new BfsActionOutput();
						StoreSourceRange(node, output);

						BfsSourceRange range = new BfsSourceRange();
						range.Begin = node.child_.next_.match_.posBeg_;
						range.Length = node.child_.next_.match_.Length;
						output.ArgumentSourceRange = range;

						output.FunctionName = GetNodeText(node.child_);
						output.Argument = GetNodeText(node.child_.next_);
						actions.Add(output);
						break;

					case EBinaryFileSchemaParser.assignment:
						BfsActionUnresolvedAssignment assignment = new BfsActionUnresolvedAssignment();
						StoreSourceRange(node, assignment);
						assignment.UnresolvedVariableName = GetNodeText(node.child_);
						assignment.Expression = ConvertExpression(node.child_.next_);
						actions.Add(assignment);
						break;
				}
			}
		}

		private void ConvertBitfieldType(PegNode node, BfsBitfield block)
		{
			block.Name = GetNodeText(node);
			block.PrimitiveType = ConvertPrimitiveType(node.next_);
			StoreSourceRange(node, block);
		
			for (PegNode bit = node.next_.next_; bit != null; bit = bit.next_)
			{
				if (GetNodeId(bit) == EBinaryFileSchemaParser.bit)
					ConvertBitfieldField(bit, block);
				else if (GetNodeId(bit) == EBinaryFileSchemaParser.localfield)
					ConvertLocalField(bit, block);
				else
					throw new AstConvertException("Not an enum field: " + GetNodeId(bit));
		
			}
		}

		private void ConvertBitfieldField(PegNode node, BfsBitfield block)
		{
			BfsBitfieldField bitff = new BfsBitfieldField();
			StoreSourceRange(node.child_, bitff);

            for (PegNode n = node.child_; n != null; n = n.next_)
            {
                if (GetNodeId(n) == EBinaryFileSchemaParser.number)
                    bitff.BitNumber = long.Parse(GetNodeText(n), CultureInfo.InvariantCulture);
                else if (GetNodeId(n) == EBinaryFileSchemaParser.bitname)
                    bitff.Name = GetNodeText(n);
                else if (GetNodeId(n) == EBinaryFileSchemaParser.action_list)
                    ConvertActionList(n, bitff.Actions);
            }
            block.BitFieldFields.Add(bitff);
		}

	}

	[SerializableAttribute]
	public class AstConvertException : Exception
	{
		public AstConvertException() : base(){}
		public AstConvertException(string msg) : base(msg) { }
		public AstConvertException(string msg, Exception ex) : base(msg, ex) { }
		private AstConvertException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
