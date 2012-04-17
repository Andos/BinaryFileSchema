using System.Collections.Generic;
using System.Text;

namespace BFSchema.CodeGenerators.CSharp
{
	public class CSharpGenerator : CodeGenerator
	{
        string FormatName = string.Empty;
        CodeClass rootParser = null;
        

		public string GenerateCode(BinaryFileSchema schema)
		{
			StringBuilder b = new StringBuilder();
            List<CodeClass> codeClasses = new List<CodeClass>();
            FormatName = schema.FormatBlock.Name;

			b.AppendLine("using System;");
			b.AppendLine("using System.Collections.Generic;");
			b.AppendLine("using System.IO;");
			b.AppendLine("using System.Text;");
            b.AppendLine("namespace " + schema.FormatBlock.Name);
            b.AppendLine("{");

            CodeClass mainClass = new CodeClass(schema.FormatBlock.Name + "Parser");
            codeClasses.Add(mainClass);
            rootParser = mainClass;

            CodeMethod mainMethod = new CodeMethod("void Main(string[] args)");
            mainMethod.CodeLines.Add("try{");
            mainMethod.CodeLines.Add("\t" + schema.FormatBlock.Name + " ps = " + schema.FormatBlock.Name + "Parser.ReadFile(args[0]);");

            mainMethod.CodeLines.Add("\tConsole.WriteLine(ps.ToString());");
            
            mainMethod.CodeLines.Add("}");
            mainMethod.CodeLines.Add("catch(Exception e){");
            mainMethod.CodeLines.Add("\tConsole.WriteLine(e.Message);");
            mainMethod.CodeLines.Add("\tConsole.WriteLine(e.StackTrace);");
            mainMethod.CodeLines.Add("}");
            mainClass.CodeMethods.Add(mainMethod);

            CodeMethod readFile = new CodeMethod( schema.FormatBlock.Name + " ReadFile(string filename)");
            mainClass.CodeMethods.Add(readFile);

			readFile.CodeLines.Add("FileStream fstream = new FileStream(filename, FileMode.Open);");
            readFile.CodeLines.Add("BinaryReader reader = new BinaryReader(fstream);");
            string endianness = ((schema.ByteOrder.ByteOrder == BfsByteOrderEnum.BigEndian) ? BfsByteOrderEnum.BigEndian : BfsByteOrderEnum.LittleEndian).ToString();
            readFile.CodeLines.Add("BfsBinaryReader bfsReader = new BfsBinaryReader(reader, BfsBinaryReader.Endianness." + endianness + ");");
            readFile.CodeLines.Add("return " + schema.FormatBlock.Name + ".Read(bfsReader);");

            foreach (IBfsDataBlock dataBlock in schema.DatablockList)
            {
                CodeClass newClass = MakeClass(dataBlock);
                if(newClass != null)
                    codeClasses.Add(newClass);
            }

            //Output the classes
            foreach (CodeClass cc in codeClasses)
                b.Append(cc.ToString());

            b.AppendLine("}");
			return b.ToString();
		}

		public override string ToString()
		{
			return "C#";
		}

        public CodeClass MakeClass(IBfsDataBlock block)
        {
            //Append all local fields no matter the type
            CodeClass codeClass = new CodeClass(block.Name);
            foreach (BfsLocalField localField in block.LocalFieldList)
                codeClass.CodeFields.Add( localField.PrimitiveType.PrimitiveType.ToString().ToLower() + " " + localField.Name + ";");

            if (block is BfsStruct)
            { 
                BfsStruct dataStruct = block as BfsStruct;
                MakeStruct(codeClass, dataStruct);
                return codeClass;
            }
            else if (block is BfsEnum)
            {
                BfsEnum dataEnum = block as BfsEnum;
                MakeEnum(codeClass, dataEnum);
                return codeClass;
            }
            else if (block is BfsBitfield)
            {
                BfsBitfield dataField = block as BfsBitfield;
                MakeBitfield(codeClass, dataField);
                return codeClass;
            }
            else if (block is BfsAbsOffset || block is BfsRelOffset)
            {
                BfsCompiler.ReportError(block.BlockTypeSourceRange,"Not yet implemented: Support for abs_offset or rel_offset!");
            }

            return null;
        }

        public void MakeReadStruct(BfsStruct dataStruct, CodeMethod codeMethod)
        {
            //Wrap the binary reader class into a compression stream if nessecary
            if (dataStruct.CompressionMethod != null)
            {
                if(dataStruct.CompressionMethod == "GZip" || dataStruct.CompressionMethod == "Deflate")
                {
                    codeMethod.CodeLines.Add( dataStruct.CompressionMethod+"Stream bfs_decompress = new "+ dataStruct.CompressionMethod +"Stream(file.BaseStream, CompressionMode.Decompress);");
                    codeMethod.CodeLines.Add("file = new BfsBinaryReader(new BinaryReader(bfs_decompress), file.FileEndianness);");
                }
                else
                    BfsCompiler.ReportError("Does not know the compression mode '" + dataStruct.CompressionMethod + "'");
            }

            //The class to return
            codeMethod.CodeLines.Add( dataStruct.Name + " " + dataStruct.Name.ToLower() + " = new " + dataStruct.Name + "();");

            foreach (IBfsNamedField namedField in dataStruct.ParseOrder)
            {
                if (namedField is BfsStructField)
                {
                    BfsStructField field = namedField as BfsStructField;

                    if (field.Conditional != null)
                        codeMethod.CodeLines.Add("if (" + CSHelper.MakeExpression(field.Conditional, dataStruct) + ")");

                    if (field.FieldType.ArrayExtension == null)
                        codeMethod.CodeLines.Add(((field.Conditional != null) ? "\t" : "") + dataStruct.Name.ToLower() + "." + field.Name + " = " + ReadType(field.FieldType)+ ";");
                    else
                        if (field.FieldType.ArrayExtension is BfsKnownArray)
                        {
                            BfsKnownArray knownArray = field.FieldType.ArrayExtension as BfsKnownArray;
                            codeMethod.CodeLines.Add(dataStruct.Name.ToLower() + "." + field.Name + " = new " + CSHelper.TypeMap(field.FieldType).ToLower() + "[" + CSHelper.MakeExpression(knownArray.Expression, dataStruct) + "];");
                            codeMethod.CodeLines.Add("for ( int i=0; i<" + CSHelper.MakeExpression(knownArray.Expression, dataStruct) + "; i++)");
                            codeMethod.CodeLines.Add("\t" + dataStruct.Name.ToLower() + "." + field.Name + "[i] = " + ReadType(field.FieldType) + ";");
                        }
                        else if (field.FieldType.ArrayExtension is BfsUnknownArray)
                        {
                            BfsUnknownArray unknownArray = field.FieldType.ArrayExtension as BfsUnknownArray;
                            
                            //Make temporary copy of the list to convert all ascii strings into hex strings.
                            List<IBfsStopCase> stopcases = new List<IBfsStopCase>(unknownArray.StopCases);
                            bool stopAtEOF = false;
                            foreach (IBfsStopCase sc in stopcases)
                                if (sc is BfsStopCaseEndOfFile)
                                    stopAtEOF = true;

                            string type = CSHelper.TypeMap(field.FieldType).ToLower();
                            codeMethod.CodeLines.Add(dataStruct.Name.ToLower() + "." + field.Name + " = new List<" + type + ">();");
                            codeMethod.CodeLines.Add("StopCaseTester " + field.Name + "_tester = new StopCaseTester(file, "+stopAtEOF.ToString().ToLower()+", new StopCase[] {");
                            bool hasInclude = false, hasSkipped = false;
                            bool onlyIncluded = true, onlySkipped = true;

                            for (int i = 0; i < stopcases.Count; i++)
                            {
                                byte[] stopcase = new byte [1];
                                if (stopcases[i] is BfsStopCaseHex)
                                    stopcase = ByteArrayConverter.ConvertHexString((stopcases[i] as BfsStopCaseHex).HexString);
                                else if(stopcases[i] is BfsStopCaseString)
                                    stopcase = ByteArrayConverter.ConvertString((stopcases[i] as BfsStopCaseString).StopString);

                                StringBuilder builder = new StringBuilder("\tnew StopCase(new byte [] { ");
                                for (int x = 0; x < stopcase.Length; x++)
                                {
                                    builder.Append("0x" + stopcase[x].ToString("X"));
                                    if (x < stopcase.Length - 1)
                                        builder.Append(", ");
                                }
                                builder.Append(" }, ");
                                BfsInclusionEnum inclusion = stopcases[i].Inclusion;
                                bool included = (inclusion == BfsInclusionEnum.Included);
                                bool skipped = (inclusion == BfsInclusionEnum.Skipped);


                                builder.Append((included ? "true" : "false") + ", ");
                                //If things are consumed, they are automatically considered consumed so this is not needed.
                                builder.Append(((skipped && !included) ? "true" : "false") + ")");

                                if (stopcases[i].Inclusion == BfsInclusionEnum.Included)
                                    hasInclude = true;
                                else
                                    onlyIncluded = false;

                                if (stopcases[i].Inclusion == BfsInclusionEnum.Skipped)
                                    hasSkipped = true;
                                else
                                    onlySkipped = false;

                                if (i < stopcases.Count - 1)
                                        builder.Append(",");

                                codeMethod.CodeLines.Add(builder.ToString());
                                
                            }
                            codeMethod.CodeLines.Add("});");

                            codeMethod.CodeLines.Add("while("+field.Name + "_tester.CanContinue())");
                            codeMethod.CodeLines.Add("\t" + dataStruct.Name.ToLower() + "." + field.Name + ".Add(" + ReadType(field.FieldType) + ");");
                            
                            string indent = string.Empty;

                            if (hasInclude)
                            {
                                //The 'if'-test on the inclusion stopcase is only needed if not all cases are included
                                if (!onlyIncluded)
                                {
                                    codeMethod.CodeLines.Add("if(" + field.Name + "_tester.StoppedAtCase().isIncluded)");
                                    indent = "\t";
                                }
                                codeMethod.CodeLines.Add(indent + field.Name + ".Add(" + ReadType(field.FieldType) + ");");
                            }
                            indent = string.Empty;
                            
                            if (hasSkipped)
                            {
                                if (!onlySkipped)
                                {
                                    codeMethod.CodeLines.Add("if(" + field.Name + "_tester.StoppedAtCase().isSkipped)");
                                    indent = "\t";
                                }
                                
                                codeMethod.CodeLines.Add(indent + "file.SkipBytes(" + field.Name + "_tester.StoppedAtCase().stopcase.Length);");
                            }
                        }
                }
                //Local fields
                else if (namedField is BfsLocalField)
                {
                    BfsLocalField field = namedField as BfsLocalField;
                    if (field.AssignmentExpression != null)
                        codeMethod.CodeLines.Add( dataStruct.Name.ToLower() + "." + field.Name + " = " + CSHelper.MakeExpression(field.AssignmentExpression, dataStruct) + ";");
                }

            }
            codeMethod.CodeLines.Add("return " + dataStruct.Name.ToLower() + ";");
        }

        public string ReadType(IBfsType type)
        {
            //if(type.ArrayExtension != null)


            if (type is BfsNamedType)
            {
                BfsNamedType namedType = type as BfsNamedType;
                return namedType.DataBlock.Name + ".Read(file)";
            }
            if (type is BfsPrimitiveType)
            {
                BfsPrimitiveType primitiveType = type as BfsPrimitiveType;
                return "file.Read" + primitiveType.PrimitiveType.ToString() + "()";
            }
            if (type is BfsFunctionType)
            { 
                BfsFunctionType functionType = type as BfsFunctionType;
                if (functionType.FunctionName == "ascii")
                {
                    //CSharpPredefinedFunctions.AddReadAsciiStringMethod(rootParser);
                    return "file.ReadASCIIString(\"" + functionType.FunctionArgument + "\")";
                }
                else return "throw new NotImplementedException()";
            }

            return "null";
        }

        public bool MakeActionList(IBfsDataBlock owner, List<string> actionList, IList<IBfsAction> actions)
        {
            //This loop is terminated when encountering an error statement.
            foreach (IBfsAction action in actions)
            {
                if (action is BfsActionAssignment)
                {
                    BfsActionAssignment assignment = action as BfsActionAssignment;
                    actionList.Add( owner.Name.ToLower() + "." + assignment.LocalVariable + " = " + CSHelper.MakeExpression(assignment.Expression,owner)+";");
                }
                else if (action is BfsActionOutput)
                {
                    BfsActionOutput output = action as BfsActionOutput;
                    if (output.FunctionName == "error")
                    {
						actionList.Add("throw new Exception(" + output.Argument + ");");
                        return false;
                    }
                    else if (output.FunctionName == "warning")
						actionList.Add("//WARNING: " + output.Argument);
                }
            }
            return true;
        }

        public void MakeStruct(CodeClass codeClass, BfsStruct dataStruct)
        {
            foreach (BfsStructField structField in dataStruct.StructFieldList)
            {
                string type = "";
                string name = structField.Name;

                if (structField.FieldType is BfsNamedType)
                {
                    BfsNamedType namedType = structField.FieldType as BfsNamedType;
                    type = namedType.DataBlock.Name;
                }
                else if (structField.FieldType is BfsPrimitiveType)
                {
                    BfsPrimitiveType primitiveType = structField.FieldType as BfsPrimitiveType;
                    type = CSHelper.TypeMap(primitiveType).ToLower();
                }
                else if (structField.FieldType is BfsFunctionType)
                {
                    BfsFunctionType functionType = structField.FieldType as BfsFunctionType;
                    if (functionType.FunctionName == "ascii")
                        type = "string";
                    else
                        BfsCompiler.ReportError(functionType.SourceRange,
                            "Only ASCII strings are supported so far..!");
                }


                if (structField.FieldType.ArrayExtension == null)
                    codeClass.CodeFields.Add(type + " " + name + ";");
                else
                {
                    if (structField.FieldType.ArrayExtension is BfsKnownArray)
                        codeClass.CodeFields.Add(type + " [] " + name + ";");
                    else
                        if (structField.FieldType.ArrayExtension is BfsUnknownArray)
                            codeClass.CodeFields.Add("List<" + type + "> " + name + ";");
                        else
                            BfsCompiler.ReportError(structField.FieldType.ArrayExtension.SourceRange,
                                "Unknown array extended type. Known or unknown array type expected!");
                }

            }

            CodeMethod read = new CodeMethod(dataStruct.Name + " Read(BfsBinaryReader file)");
            codeClass.CodeMethods.Add(read);
            MakeReadStruct(dataStruct, read);


            //ToString() method
            CodeMethod toString = new CodeMethod("override string ToString()");
            codeClass.CodeMethods.Add(toString);

            toString.CodeLines.Add("StringBuilder sb = new StringBuilder();");
            toString.CodeLines.Add("sb.AppendLine(\"== Struct: "+ dataStruct.Name +" ==\");");
            foreach (BfsStructField field in dataStruct.StructFieldList)
                toString.CodeLines.Add("sb.AppendLine(\"" + field.Name + " : \" + " + field.ToString() + ".ToString());");
            toString.CodeLines.Add("return sb.ToString();");

        }

        public void MakeEnum(CodeClass codeClass, BfsEnum dataEnum)
        {
            codeClass.ClassType = CodeClass.ClassTypeEnum.Struct;

            //Only add nessecary internal enum aliases if there are any.
            if (dataEnum.EnumAliases.Count > 0)
            {
                StringBuilder b = new StringBuilder();
                b.Append("enum " + dataEnum.Name + "Enum {");

                List<string> foundEnumAliases = new List<string>();
                for (int i = 0; i < dataEnum.EnumFields.Count; i++)
                {
                    BfsEnumField enumField = dataEnum.EnumFields[i];
                    if (enumField.Alias != null && !foundEnumAliases.Contains(enumField.Alias))
                    {
                        if (i != 0)
                            b.Append(", ");
                        b.Append(enumField.Alias);
                        foundEnumAliases.Add(enumField.Alias);
                    }
                }
                b.Append("}");
                codeClass.CodeFields.Add(b.ToString());
                codeClass.CodeFields.Add(dataEnum.Name + "Enum " + dataEnum.Name.ToLower() + ";");
            }

            //Adds the implicit 'value' variable for storing the read primitive type.
            codeClass.CodeFields.Add(CSHelper.TypeMap(dataEnum.PrimitiveType).ToLower() + " value;");

            CodeMethod codeMethod = new CodeMethod(dataEnum.Name + " Read(BfsBinaryReader file)");
            codeClass.CodeMethods.Add(codeMethod);

            string objectType = dataEnum.Name;
            string varName = dataEnum.Name.ToLower();
            codeMethod.CodeLines.Add(objectType + " " + varName + " = new " + objectType + "();");
            codeMethod.CodeLines.Add(varName + ".value = file.Read" + dataEnum.PrimitiveType.PrimitiveType.ToString() + "();");

            //Sorts the enum fields into categories to make a pretty switch-case/if-then-else structure.
            List<BfsEnumField> singleValues = new List<BfsEnumField>();
            List<BfsEnumField> rangeValues = new List<BfsEnumField>();
            BfsEnumField elseValue = null;

            foreach (BfsEnumField enumField in dataEnum.EnumFields)
                if (enumField.EnumMatch is BfsEnumValue)
                    singleValues.Add(enumField);
                else if (enumField.EnumMatch is BfsEnumRange)
                    rangeValues.Add(enumField);
                else if (enumField.EnumMatch is BfsEnumElse)
                    elseValue = enumField;

            string tab = string.Empty;

            //If there are 2 or above single enum matches a switch/case statement is made with the values as cases.
            //In case the value isn't any of the values it checks the ranges one by one in an if/else/elseif/elseif/... kind of way.

            if (singleValues.Count > 1)
            {
                codeMethod.CodeLines.Add("switch(" + varName + ".value)");
                codeMethod.CodeLines.Add("{");
                tab = "\t\t";

                foreach (BfsEnumField enumField in singleValues)
                {
                    BfsEnumValue enumVal = enumField.EnumMatch as BfsEnumValue;
                    codeMethod.CodeLines.Add("\tcase " + enumVal.Value + ":");
                    if (enumField.Alias != null)
                        codeMethod.CodeLines.Add("\t\t" + varName + "." + varName + " = " + objectType + "Enum." + enumField.Alias + ";");

                    List<string> actionList = new List<string>();
                    bool addBreak = MakeActionList(dataEnum, actionList, enumField.Actions);

                    foreach (string str in actionList)
                        codeMethod.CodeLines.Add("\t\t" + str);

                    if (addBreak)
                        codeMethod.CodeLines.Add("\t\tbreak;");
                }

                codeMethod.CodeLines.Add("\tdefault:");
            }

            string defaultTab = "\t";
            if (singleValues.Count <= 1)
                defaultTab = string.Empty;

            //If there only are one single value case, then include it in the if-then-else sequence
            IfThenElseSequence ifthenelse = new IfThenElseSequence(defaultTab, varName + ": '" + "\" + " + varName + ".value + \"");

            if (elseValue != null)
            {
                ifthenelse.ElseBlock = new IfBlock();
                MakeActionList(dataEnum, ifthenelse.ElseBlock.CodeLines, elseValue.Actions);
            }

            int countIfElse = 0;
            if (singleValues.Count == 1)
            {
                BfsEnumField enumField = singleValues[0];
                BfsEnumValue enumVal = enumField.EnumMatch as BfsEnumValue;

                IfBlock ifblock = new IfBlock(varName + ".value == " + enumVal.Value);
                ifthenelse.IfBlocks.Add(ifblock);

                if (enumField.Alias != null)
                    ifblock.CodeLines.Add(varName + "." + varName + " = " + objectType + "Enum." + enumField.Alias + ";");

                MakeActionList(dataEnum, ifblock.CodeLines, enumField.Actions);
                countIfElse++;
            }

            //Enum ranges

            foreach (BfsEnumField enumField in rangeValues)
            {
                BfsEnumRange enumVal = enumField.EnumMatch as BfsEnumRange;
                string firstEquality = ((enumVal.StartInclusion == BfsInclusionEnum.Included) ? ">= " : "> ") + enumVal.StartValue;
                string secondEquality = ((enumVal.EndInclusion == BfsInclusionEnum.Included) ? "<= " : "< ") + enumVal.EndValue;

                IfBlock ifblock = new IfBlock(varName + ".value " + firstEquality + " && " + varName + ".value " + secondEquality);
                ifthenelse.IfBlocks.Add(ifblock);
                if (enumField.Alias != null)
                    ifblock.CodeLines.Add(varName + "." + varName + " = " + objectType + "Enum." + enumField.Alias + ";");

                MakeActionList(dataEnum, ifblock.CodeLines, enumField.Actions);
            }

            codeMethod.AddSplitLines(ifthenelse.ToString());

            //End off the switch
            if (singleValues.Count > 1)
            {
                //Only add the final 'break' if the default case doesn't throw any exception
                //to prevent the code not reachable error

                if (ifthenelse.IfBlocks.Count != 0 || !ifthenelse.ElseBlock.ToString().Contains("throw new "))
                    codeMethod.CodeLines.Add("\t\tbreak;");

                codeMethod.CodeLines.Add("}");
            }

            codeMethod.CodeLines.Add("return " + varName + ";");

            //ToString() method
            CodeMethod toString = new CodeMethod("override string ToString()");
            codeClass.CodeMethods.Add(toString);

            if(dataEnum.EnumAliases.Count > 0)
                toString.CodeLines.Add("return " + dataEnum.Name.ToLower() + ".ToString();");
            else
                toString.CodeLines.Add("return value.ToString();");

        }

        public void MakeBitfield(CodeClass codeClass, BfsBitfield dataField)
        {
            codeClass.ClassType = CodeClass.ClassTypeEnum.Struct;

            //Adds the implicit 'value' variable for storing the read primitive type.
            codeClass.CodeFields.Add(CSHelper.TypeMap(dataField.PrimitiveType).ToLower() + " value;");

            string objectType = dataField.Name;
            string varName = dataField.Name.ToLower();

            StringBuilder b = new StringBuilder("bool ");
            for ( int i = 0; i< dataField.BitFieldFields.Count; i++)
            {
                if (i != 0)
                    b.Append(", ");

                BfsBitfieldField field = dataField.BitFieldFields[i];
                if (field.Name == null)
                    b.Append("bit" + field.BitNumber);
                else
                    b.Append(field.Name);
            }
            b.Append(";");
            codeClass.CodeFields.Add(b.ToString());

            CodeMethod codeMethod = new CodeMethod(objectType + " Read(BfsBinaryReader file)");
            codeClass.CodeMethods.Add(codeMethod);

            codeMethod.CodeLines.Add(objectType + " " + varName + " = new " + objectType + "();");
            codeMethod.CodeLines.Add(varName + ".value = file.Read" + dataField.PrimitiveType.PrimitiveType.ToString() + "();");

            foreach( BfsBitfieldField field in dataField.BitFieldFields )
            {
                int size = Environments.GetSizeOfPrimitiveType(dataField.PrimitiveType);
                string mask = MakeMask(field.BitNumber, size);
                string bitName = ((field.Name != null) ? field.Name : ("bit" + field.BitNumber));

                if (field.Actions.Count > 0)
                {
                    IfBlock block = new IfBlock("(" + varName + ".value & " + mask + ") == 1");
                    block.CodeLines.Add(varName + "." + bitName + " = true;");
                    MakeActionList(dataField, block.CodeLines, field.Actions);
                    codeMethod.AddSplitLines(block.ToString());
                }
                else
                    codeMethod.CodeLines.Add(varName + "." + bitName + " = " + "(" + varName + ".value & " + mask + ") == 1;");
            }

            codeMethod.CodeLines.Add("return " + varName + ";");
        }

        private string MakeMask(long bit, int size)
        {
            StringBuilder b = new StringBuilder();
            b.Append("0x");

            //size * 8 - bit-1     // hvis det er big-endian
            string hexValue = (1 << (int)bit).ToString("x");
            
            for (int i = 0; i < size*2-hexValue.Length; i++)
                b.Append("0");

            b.Append(hexValue);
            return b.ToString();
        }

	}


}
