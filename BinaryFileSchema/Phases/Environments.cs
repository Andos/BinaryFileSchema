using System.Collections.Generic;

namespace BFSchema
{
	class Environments : IPhase
	{
		public void Check(BinaryFileSchema schema)
		{
			bool formatfound = false;
			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				bool elsefound = false;

                //Check that only one block is declared 'format'.
				if (block.IsFormat && formatfound == false)
				{
					formatfound = true;
					schema.FormatBlock = block;
				}
				else if (block.IsFormat && formatfound == true)
					BfsCompiler.ReportError(block.SourceRange,
						"Only one 'format' specifier is allowed per schema");

				//Check that no two blocks have the same name and build map over types.
				if (schema.DataBlocks.ContainsKey(block.Name))
					BfsCompiler.ReportError(block.SourceRange,
						"Duplicate datablock name not allowed: '" + block.Name + "'");
				else
					schema.DataBlocks.Add(block.Name, block);

				//Build map over local variables
				foreach (BfsLocalField localfield in block.LocalFieldList)
					if (localfield.Name == "value")
						BfsCompiler.ReportError(localfield.SourceRange,
							"'value' is a reserved name for use in expressions in consumable types.");
                    else if (block.LocalFields.ContainsKey(localfield.Name))
                        BfsCompiler.ReportError(localfield.SourceRange,
                            "Duplicate local variable name not allowed: '" + localfield.Name + "'");
                    else
                        block.LocalFields.Add(localfield.Name, localfield);

				//Check that no two struct fields in the same block have the same name.
				if (block is IBfsStructType)
				{
					IBfsStructType structblock = block as IBfsStructType;
					foreach (BfsStructField field in structblock.StructFieldList)
					{
						if (field.Name == "value")
							BfsCompiler.ReportError(field.SourceRange,
								"'value' is a reserved name for use in expressions in consumable types.");
                        else if (structblock.StructFields.ContainsKey(field.Name))
                            BfsCompiler.ReportError(field.SourceRange,
                                "Duplicate field name not allowed: '" + field.Name + "'");
                        else
                            structblock.StructFields.Add(field.Name, field);
					}
					//Check that no local variables have the same name
					foreach (BfsStructField field in structblock.StructFieldList)
						if (structblock.LocalFields.ContainsKey(field.Name))
							BfsCompiler.ReportError(field.SourceRange, "Name clash with local variable: '" + field.Name + "'");

                    //Check that only supported compressions methods are defined
                    if (structblock.CompressionMethod != null)
                        if(structblock.CompressionMethod != "GZip" && structblock.CompressionMethod != "Deflate")
                            BfsCompiler.ReportError(structblock.CompressionRange,
                                "Unknown compression method: '" + structblock.CompressionMethod + "'. Expected 'GZip' or 'Deflate'.");
				}

				if (block is BfsEnum)
				{
					BfsEnum enumblock = block as BfsEnum;
					enumblock.SizeInBytes = GetSizeOfPrimitiveType(enumblock.PrimitiveType);
					HashSet<long> enumvalues = new HashSet<long>();
					HashSet<BfsEnumRange> enumranges = new HashSet<BfsEnumRange>();
					foreach (BfsEnumField field in enumblock.EnumFields)
					{
						//Check that there is only one 'else'.
						bool isElse = (field.EnumMatch is BfsEnumElse);
						if (isElse && elsefound == false)
							elsefound = true;
						else if (isElse && elsefound == true)
							BfsCompiler.ReportError(field.SourceRange, "Only one 'else' event allowed");

						//Building map from name to enum field aliases
						if (field.Alias != null && !enumblock.EnumAliases.ContainsKey(field.Alias))
						{
							BfsEnumFieldAlias alias = new BfsEnumFieldAlias();
							alias.Name = field.Alias;
							enumblock.EnumAliases.Add(field.Alias, alias);
						}

						//Check that no numbers or ranges intersect.
						if (field.EnumMatch is BfsEnumValue)
						{
							BfsEnumValue e_val = (field.EnumMatch as BfsEnumValue);
							foreach (int val in enumvalues)
								if (e_val.Value == val)
									BfsCompiler.ReportError(field.SourceRange, "Value already defined: '" + val + "'");

							foreach (BfsEnumRange range in enumranges)
								CheckValueOnRange(e_val.Value, range, field);

							enumvalues.Add(e_val.Value);
						}
						if (field.EnumMatch is BfsEnumRange)
						{
							BfsEnumRange range = field.EnumMatch as BfsEnumRange;

							if (range.EndValue <= range.StartValue)
								BfsCompiler.ReportError(range.SourceRange, "End-value must be larger than start-value in the range");

							long actualRange = range.EndValue - range.StartValue + 1;
							if (range.StartInclusion == BfsInclusionEnum.Excluded)
								actualRange--;
							if (range.EndInclusion == BfsInclusionEnum.Excluded)
								actualRange--;

							if (actualRange == 0)
								BfsCompiler.ReportError(range.SourceRange, "Range is empty because of the inclusions brackets: '" + range + "'");

							if (actualRange == 1)
								BfsCompiler.ReportWarning(range.SourceRange, "Range is of length 1, why not use a single value instead?: '" + range + "'");

							foreach (int val in enumvalues)
								CheckValueOnRange(val, range, field);

							foreach (BfsEnumRange rb in enumranges)
								CheckRangeOnRange(range, rb, field);

							enumranges.Add(range);
						}
					}
				}

				
                HashSet<long> bits = new HashSet<long>();
                HashSet<string> bitnames = new HashSet<string>();
				if (block is BfsBitfield)
				{
					BfsBitfield bitfield = block as BfsBitfield;
					bitfield.SizeInBytes = GetSizeOfPrimitiveType(bitfield.PrimitiveType);
					foreach (BfsBitfieldField field in bitfield.BitFieldFields)
					{
                        //Check that no bit-indexes are listed twice.
						if (bits.Contains(field.BitNumber))
							BfsCompiler.ReportError(field.SourceRange, "Bit may only be listed once: '" + field.BitNumber + "'");
						else
							bits.Add(field.BitNumber);

                        //Check that no bit-names are listed twice.
                        if (field.Name != null && bitnames.Contains(field.Name))
                            BfsCompiler.ReportError(field.SourceRange, "Two identical bitnames found: '" + field.Name + "'");
                        else
                            if(field.Name != null)
                                bitnames.Add(field.Name);

						//Check that the bit-indexes doesn't exeed the size of the primitive type.
						long sizeoftype = GetSizeOfPrimitiveType(bitfield.PrimitiveType) * 8 - 1;
						if (field.BitNumber > sizeoftype)
							BfsCompiler.ReportError(field.SourceRange,
								"Bit-index exceeds the boundary of the primitive type: '" + field.BitNumber + "' > " + sizeoftype);
					}
				}
			}
            if (schema.FormatBlock == null)
                BfsCompiler.ReportError("No 'format' block specified. Schema needs an entry point.");

		}

		private static void CheckValueOnRange(long val, BfsEnumRange range, BfsEnumField field)
		{
			if ( val < range.StartValue || val > range.EndValue)
				return;

			if (val > range.StartValue && val < range.EndValue)
				BfsCompiler.ReportError(field.SourceRange, "Value intersecting range is not allowed: '" + range + "'");

			if (range.StartInclusion == BfsInclusionEnum.Included && val == range.StartValue
				|| range.EndInclusion == BfsInclusionEnum.Included && val == range.EndValue)
				BfsCompiler.ReportError(field.SourceRange, "Value intersecting range is not allowed. Check the inclusion brackets: '" + range + "'");
		}

		private static void CheckRangeOnRange(BfsEnumRange ra, BfsEnumRange rb, BfsEnumField field)
		{
			if (ra.StartValue > rb.EndValue || rb.StartValue > ra.EndValue)
				return;

			if( ra.EndValue == rb.StartValue &&
				(ra.EndInclusion == BfsInclusionEnum.Excluded || rb.StartInclusion == BfsInclusionEnum.Excluded ))
				return;

			if( rb.EndValue == ra.StartValue &&
				(rb.EndInclusion == BfsInclusionEnum.Excluded || ra.StartInclusion == BfsInclusionEnum.Excluded ))
				return;

			BfsCompiler.ReportError(field.SourceRange, "Intersecting ranges not allowed: '" + ra + "' and '" + rb + "'");
		}

		public static int GetSizeOfPrimitiveType( BfsPrimitiveType primitiveType )
		{
			switch (primitiveType.PrimitiveType)
			{
				case BfsPrimitiveTypeEnum.Bool: return 1;
				case BfsPrimitiveTypeEnum.Sbyte: return 1;
				case BfsPrimitiveTypeEnum.Ubyte: return 1;
				case BfsPrimitiveTypeEnum.Short: return 2;
				case BfsPrimitiveTypeEnum.Ushort: return 2;
				case BfsPrimitiveTypeEnum.Int: return 4;
				case BfsPrimitiveTypeEnum.Uint: return 4;
				case BfsPrimitiveTypeEnum.Long: return 4;
				case BfsPrimitiveTypeEnum.Ulong: return 4;
				case BfsPrimitiveTypeEnum.CallExpression:
					BfsCompiler.ReportMessage("Function type...");
					return 1;
			}
			return 1;
		}

	}


}
