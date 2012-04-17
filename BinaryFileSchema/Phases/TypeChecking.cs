using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFSchema
{
	public class TypeChecking : IPhase
	{
		public void Check( BinaryFileSchema schema )
		{
			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				//Assignment expressions on local variables
				foreach (BfsLocalField local in block.LocalFieldList)
					if (local.AssignmentExpression != null)
					{
						if (local.PrimitiveType.ArrayExtension != null)
							BfsCompiler.ReportError(local.SourceRange,
								"Local variables with array extensions cannot have assignment expressions.");

						CheckExpression(local.AssignmentExpression, block);
						CheckAssignment(local.SourceRange, local.PrimitiveType.PrimitiveType, local.AssignmentExpression.PrimitiveType);
					}

				//Check conditional expressions + array extensions
				if (block is IBfsStructType)
				{
					IBfsStructType structType = (IBfsStructType)block;
					foreach (BfsStructField field in structType.StructFields.Values)
					{
						if (field.Conditional != null)
						{
							CheckExpression(field.Conditional, block);
							if (field.Conditional.PrimitiveType != BfsPrimitiveTypeEnum.Bool)
								BfsCompiler.ReportError(field.Conditional.SourceRange,
									"The conditional isn't of boolean type!");
						}

						if (field.FieldType.ArrayExtension != null && field.FieldType.ArrayExtension is BfsKnownArray)
						{
							BfsKnownArray arr = field.FieldType.ArrayExtension as BfsKnownArray;
							CheckExpression(arr.Expression, block);
							if (arr.Expression.PrimitiveType <= BfsPrimitiveTypeEnum.Bool
								&& arr.Expression.PrimitiveType != BfsPrimitiveTypeEnum.CallExpression)
								BfsCompiler.ReportError(arr.SourceRange,
									"Unsuitable type for array-extension: " + arr.Expression.PrimitiveType );
						}
					}
				}
				// All the expressions in action-lists for both Enums and BitFields
				else if (block is BfsEnum)
				{
					foreach (BfsEnumField field in (block as BfsEnum).EnumFields)
						if (field.Actions != null)
							foreach (IBfsAction action in field.Actions)
								if (action is BfsActionAssignment)
								{
									BfsActionAssignment a = action as BfsActionAssignment;
									CheckExpression(a.Expression, block);
									CheckAssignment(a.SourceRange, a.LocalVariable.PrimitiveType.PrimitiveType, a.Expression.PrimitiveType);
								}
				}
				else if (block is BfsBitfield)
				{
					foreach (BfsBitfieldField field in (block as BfsBitfield).BitFieldFields)
						if (field.Actions != null)
							foreach (IBfsAction action in field.Actions)
								if (action is BfsActionAssignment)
								{
									BfsActionAssignment a = action as BfsActionAssignment;
									CheckExpression(a.Expression, block);
									CheckAssignment(a.SourceRange, a.LocalVariable.PrimitiveType.PrimitiveType, a.Expression.PrimitiveType);
								}
				}
			}
		}

		private void CheckExpression(BfsExpression expression, IBfsDataBlock block)
		{
			CheckGroup(expression.ExpressionGroup, block);
			//BfsCompiler.ReportWarning(expression.ExpressionGroup.SourceRange, expression.ToString() + ": " + expression.PrimitiveType);
		}

		private void CheckGroup( BfsExpGroup group, IBfsDataBlock block )
		{
			foreach (IBfsExpNode node in group.Members)
			{
				if (node is BfsNumberExp)
					CheckType(group, BfsPrimitiveTypeEnum.Int, block);
				else if (node is BfsBooleanExp)
					CheckType(group, BfsPrimitiveTypeEnum.Bool, block);
                else if (node is BfsCallExp)
                    CheckType(group, BfsPrimitiveTypeEnum.CallExpression, block);
				else if (node is BfsExpressionVariable)
				{
                    BfsExpressionVariable expvar = node as BfsExpressionVariable;

                    //Check if the type of the variable is a function type (like ascii("Hello"))
                    if (expvar.LastField is BfsStructField)
                        if ((expvar.LastField as BfsStructField).FieldType is BfsFunctionType)
                            BfsCompiler.ReportError(expvar.SourceRange, "Cannot have function types in expressions!");

                    CheckType(group, expvar.PrimitiveType, block);

					//Checking if any part of an expression variable references a variable that is array-extended
					foreach( IBfsNamedField field in expvar.NameHierarchy )
						if (field is BfsStructField)
						{
							BfsStructField f = field as BfsStructField;
							if (f != null)
							{
								if (f.FieldType.ArrayExtension != null)
									BfsCompiler.ReportError(expvar.SourceRange,
										"Cannot use array-extended type in expression: '" + expvar + "'");

								if (f.Skip)
									BfsCompiler.ReportError(expvar.SourceRange,
										"The variable '" + expvar + "' has been marked as skipped data and therefore cannot be used in expressions.");
							}
						}
				}
				else if (node is BfsEnumAliasExp)
					CheckType(group, BfsPrimitiveTypeEnum.EnumMember, block);
				else if (node is BfsOperator)
					CheckOperator(group, node as BfsOperator);
				else if (node is BfsExpGroup)
				{
					BfsExpGroup g = node as BfsExpGroup;
					CheckGroup(g, block);
					//Compare the subgroups type to the parents (this) type.
					CheckType(group, g.PrimitiveType, block);

					//If the group consists of comparissons then the type of the group must be boolean.
					if (g.OperatorPrecedenceLevel == BfsExpGroupOperatorLevel.Comparisson)
						group.PrimitiveType = BfsPrimitiveTypeEnum.Bool;
				}
			}
		}

		private void CheckOperator(BfsExpGroup group, BfsOperator op)
		{
			//Just a formality check if the operators in the same group actually do belong to the same operator precedence-group
			if (group.OperatorPrecedenceLevel == BfsExpGroupOperatorLevel.Undetermined)
				group.OperatorPrecedenceLevel = op.PrecendenceLevel;
			else if (group.OperatorPrecedenceLevel != op.PrecendenceLevel)
				BfsCompiler.ReportError(op.SourceRange,"Mixed operator-precedence groups! '"
					+ op + "' doesn't belong in '"+group.OperatorPrecedenceLevel+"'");
		}

		private void CheckType(BfsExpGroup group, BfsPrimitiveTypeEnum type, IBfsDataBlock block)
		{
			//If boolean and non-boolean types are mixed.
			if((group.PrimitiveType == BfsPrimitiveTypeEnum.Bool && type > BfsPrimitiveTypeEnum.Bool))
				BfsCompiler.ReportError(group.SourceRange, "Cannot mix boolean and " + type + " types");
			else if(type == BfsPrimitiveTypeEnum.Bool && group.PrimitiveType > BfsPrimitiveTypeEnum.Bool)
				BfsCompiler.ReportError(group.SourceRange, "Cannot mix boolean and " + group.PrimitiveType + " types");

            //if (type == BfsPrimitiveTypeEnum.FunctionType || group.PrimitiveType == BfsPrimitiveTypeEnum.FunctionType)
            //    BfsCompiler.ReportError(group.SourceRange, "You cannot use function typed variables in expressions");

			if (type == BfsPrimitiveTypeEnum.Undetermined)
				BfsCompiler.ReportError(group.SourceRange,"Undetermined type of variable");

			//If all is well then it will pick the largest type that can contain the variable.
			//Hiearchy: Undetermined,Bool,Ubyte,Sbyte,Ushort,Short,Uint,Int,Ulong,Long
			BfsPrimitiveTypeEnum a = group.PrimitiveType;
			BfsPrimitiveTypeEnum b = type;
			group.PrimitiveType = (a > b) ? a : b;
		}

		private void CheckAssignment(BfsSourceRange range, BfsPrimitiveTypeEnum target, BfsPrimitiveTypeEnum newvar)
		{
			if (target < newvar)
				BfsCompiler.ReportError(range,"The type '"+newvar+"' could not be stored into the type '"+target+"'");
		}

	}
}
