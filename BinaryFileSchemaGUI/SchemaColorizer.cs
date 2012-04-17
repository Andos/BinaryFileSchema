using System;
using System.Collections.Generic;
using BFSchema;
using System.Windows.Forms;
using System.Drawing;

namespace BinaryFileSchemaGUI
{
	class SchemaColorizer
	{
		static readonly Font normalfont = new Font(FontFamily.GenericMonospace, 9.75f, FontStyle.Regular);
		static Font boldfont = new Font(FontFamily.GenericMonospace, 9.75f, FontStyle.Bold);

		static Color normalcolor = Color.Black;
		static Color keywordcolor = Color.Blue;
		static Color typecolor = Color.FromArgb(43, 145, 175);
		static Color numbercolor = Color.FromArgb(223, 112, 0);
		static Color enumaliascolor = Color.FromArgb(0, 128, 255);
		static Color functioncolor = Color.Red;
		static Color skipcolor = Color.Red;
		static Color stringcolor = Color.DarkRed;
        static Color compressioncolor = Color.DarkGoldenrod;

		static RichTextBox textBox;

		public static void ColorizeSchema(BinaryFileSchema schema, RichTextBox richTextBox)
		{
			textBox = richTextBox;
			richTextBox.SelectAll();
			richTextBox.SelectionColor = Color.Black;
			richTextBox.SelectionFont = normalfont;

			//Colorize the byte-order
			if ( schema.ByteOrder != null && schema.ByteOrder.ByteOrder != BfsByteOrderEnum.LanguageDefault)
			{
				richTextBox.Select(schema.ByteOrder.SourceRange.Begin, schema.ByteOrder.SourceRange.Length);
				richTextBox.SelectionColor = keywordcolor;
			}

			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				ColorRange(block.SourceRange, normalcolor, boldfont);
				ColorRange(block.BlockTypeSourceRange, keywordcolor, boldfont);

				if (block.IsFormat)
					ColorRange(block.FormatSourceRange, keywordcolor, boldfont);

				if (block is IBfsConsumptionType)
				{
					IBfsConsumptionType special = block as IBfsConsumptionType;
					ColorRange(special.PrimitiveType.SourceRange, typecolor);
				}

				foreach (BfsLocalField lfield in block.LocalFields.Values)
				{
					ColorRange(lfield.SourceRange, 5, keywordcolor);
					ColorRange(lfield.PrimitiveType.SourceRange, typecolor);

					if (lfield.AssignmentExpression != null)
						ColorExpression(lfield.AssignmentExpression.ExpressionGroup);
				}

                if (block is IBfsStructType)
                {
                    IBfsStructType structtype = block as IBfsStructType;

                    if (structtype.CompressionMethod != null)
                        ColorRange(structtype.CompressionRange, compressioncolor);

                    foreach (BfsStructField field in structtype.StructFields.Values)
                    {
                        ColorRange(field.FieldType.SourceRange, typecolor);
                        if (field.FieldType is BfsFunctionType)
                            ColorRange((field.FieldType as BfsFunctionType).ArgumentSourceRange, stringcolor);

                        if (field.Conditional != null)
                        {
                            ColorRange(field.ConditionalSourceRange, 2, keywordcolor);
                            ColorExpression(field.Conditional.ExpressionGroup);
                        }

                        if (field.Skip)
                            ColorRange(field.SkipSourceRange, skipcolor);

                        if (field.FieldType.ArrayExtension != null && field.FieldType.ArrayExtension is BfsKnownArray)
                            ColorExpression((field.FieldType.ArrayExtension as BfsKnownArray).Expression.ExpressionGroup);
                        else if (field.FieldType.ArrayExtension != null && field.FieldType.ArrayExtension is BfsUnknownArray)
                        {
                            BfsUnknownArray array = field.FieldType.ArrayExtension as BfsUnknownArray;

                            ColorRange(array.UntilSourceRange, keywordcolor);

                            foreach (BfsSourceRange range in array.OrWords)
                                ColorRange(range, keywordcolor);

                            foreach (IBfsStopCase stopcase in array.StopCases)
                            {
                                if (stopcase is BfsStopCaseString)
                                    ColorRange(stopcase.SourceRange, stringcolor);
                                else if (stopcase is BfsStopCaseEndOfFile)
                                    ColorRange(stopcase.SourceRange, enumaliascolor);
                                else if (stopcase is BfsStopCaseHex)
                                    ColorRange(stopcase.SourceRange, numbercolor);

                                BfsSourceRange zerorange = new BfsSourceRange();
                                if (stopcase.InclusionSourceRange != zerorange)
                                    ColorRange(stopcase.InclusionSourceRange, keywordcolor);
                            }

                        }
                    }
                }
                else if (block is BfsEnum)
                    foreach (BfsEnumField field in (block as BfsEnum).EnumFields)
                    {
                        if (field.Alias != null)
                            ColorRange(field.AliasSourceRange, enumaliascolor);

                        if (field.EnumMatch is BfsEnumValue)
                            ColorRange(field.EnumMatch.SourceRange, numbercolor);
                        else if (field.EnumMatch is BfsEnumRange)
                            ColorRange(field.EnumMatch.SourceRange.Begin + 1, field.EnumMatch.SourceRange.Length - 2, numbercolor);
                        else if (field.EnumMatch is BfsEnumElse)
                            ColorRange(field.EnumMatch.SourceRange, keywordcolor);

                        ColorActionList(field.Actions);
                    }
                else if (block is BfsBitfield)
                {
                    foreach (BfsBitfieldField field in (block as BfsBitfield).BitFieldFields)
                    {
                        ColorRange(field.SourceRange, numbercolor);
                        ColorActionList(field.Actions);
                    }
                }
			}
		}

		private static void ColorActionList(IList<IBfsAction> actions)
		{
			if (actions == null)
				return;

			foreach (IBfsAction action in actions)
			{
				if (action is BfsActionAssignment)
				{
					BfsActionAssignment assignmentAction = action as BfsActionAssignment;
					ColorExpression(assignmentAction.Expression.ExpressionGroup);
				}
				else if (action is BfsActionOutput)
				{
					BfsActionOutput outputAction = action as BfsActionOutput;
					ColorRange(outputAction.SourceRange, functioncolor);
					ColorRange(outputAction.ArgumentSourceRange, stringcolor);
				}
				else throw new Exception("Unresolved assignment detected: " + action);
			}
		}


		private static void ColorExpression(BfsExpGroup group)
		{
			if (group == null)
				return;

			foreach (IBfsExpNode node in group.Members)
			{
				//Put nodes here that should have been replaced in the TypeLinking phase.
				//if (node is BfsUnresolvedVariableExp)
					//throw new Exception("Unresolved variable expression detected!");

				if (node is BfsExpGroup)
					ColorExpression(node as BfsExpGroup);
				else if (node is BfsBooleanExp)
				{
					BfsBooleanExp exp = node as BfsBooleanExp;
					ColorRange(exp.SourceRange, keywordcolor);
				}
				else if (node is BfsNumberExp)
				{
					BfsNumberExp val = node as BfsNumberExp;
					ColorRange(val.SourceRange, numbercolor);
				}
				else if (node is BfsOperator)
				{ 
					//TODO
				}
				else if (node is BfsValueExp)
				{
					BfsValueExp val = node as BfsValueExp;
					ColorRange(val.SourceRange, keywordcolor);
				}
				else if (node is BfsEnumAliasExp)
				{
					BfsEnumAliasExp val = node as BfsEnumAliasExp;
					ColorRange(val.SourceRange, enumaliascolor);
				}
				else if (node is BfsCallExp)
				{
					BfsCallExp val = node as BfsCallExp;
					ColorRange(val.NameSourceRange, keywordcolor);
					ColorRange(val.ArgumentSourceRange, typecolor);
				}
			}
		}

		private static void ColorRange( BfsSourceRange range, Color color )
		{
			textBox.Select(range.Begin, range.Length);
			textBox.SelectionColor = color;
		}
		private static void ColorRange(BfsSourceRange range, int length, Color color)
		{
			textBox.Select(range.Begin,length);
			textBox.SelectionColor = color;
		}
		private static void ColorRange(int begin, int length, Color color)
		{
			textBox.Select(begin, length);
			textBox.SelectionColor = color;
		}
		private static void ColorRange(BfsSourceRange range, Color color, Font font)
		{
			textBox.Select(range.Begin, range.Length);
			textBox.SelectionColor = color;
			textBox.SelectionFont = font;
		}
	}

}
