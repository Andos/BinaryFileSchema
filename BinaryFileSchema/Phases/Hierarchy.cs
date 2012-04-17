
namespace BFSchema
{
	class Hierarchy : IPhase
	{
		private BinaryFileSchema schema;

		public void Check( BinaryFileSchema schema )
		{
			this.schema = schema;

			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				//Generate map over unresolved expression variables;
				foreach (BfsLocalField field in block.LocalFields.Values)
					if (field.AssignmentExpression != null)
						CheckExpression(field.AssignmentExpression, block);

				if (block is IBfsStructType)
				{
					IBfsStructType structType = (IBfsStructType)block;

					foreach (BfsStructField field in structType.StructFields.Values)
					{
						//Check conditional expressions
						if (field.Conditional != null)
							CheckExpression(field.Conditional, block);

						if (field.FieldType.ArrayExtension != null)
                            if(field.FieldType.ArrayExtension is BfsKnownArray)
							    CheckExpression((field.FieldType.ArrayExtension as BfsKnownArray).Expression, block);
                            //Check that no unknown sized arrays are located within any structure that is compressed (seeking not allowed)
                            else if(field.FieldType.ArrayExtension is BfsUnknownArray && structType.CompressionMethod != null)
                                BfsCompiler.ReportError(field.FieldType.ArrayExtension.SourceRange,
                                    "Cannot have unknown sized arrays within compressed blocks: '" + field.Name + "'");

					}
				}
				else if (block is BfsEnum)
				{
					foreach( BfsEnumField field in (block as BfsEnum).EnumFields)
						if(field.Actions != null)
							foreach( IBfsAction action in field.Actions )
								if( action is BfsActionAssignment )
									CheckExpression((action as BfsActionAssignment).Expression, block);
				}
				else if (block is BfsBitfield)
				{
					foreach (BfsBitfieldField field in (block as BfsBitfield).BitFieldFields)
						if (field.Actions != null)
							foreach (IBfsAction action in field.Actions)
								if (action is BfsActionAssignment)
									CheckExpression((action as BfsActionAssignment).Expression, block);
				}
			}
		}

		private void CheckExpression(BfsExpression expression, IBfsDataBlock block)
		{
			CheckExpressionNode(expression, expression.ExpressionGroup, block);
			CheckForEnumMembers(expression.ExpressionGroup, block);
		}

		private void CheckExpressionNode(BfsExpression exp, BfsExpGroup group, IBfsDataBlock block)
		{
			for (int index = 0; index < group.Members.Count; index++)
			{
				IBfsExpNode node = group.Members[index];

				if (node is BfsOperator)
					continue;

				if (node is BfsExpGroup)
					CheckExpressionNode(exp, node as BfsExpGroup, block);
				else if (node is BfsCallExp)
				{
					BfsCallExp call = node as BfsCallExp;
					if (call.FunctionName != "sizeof")
					{
						BfsCompiler.ReportError(call.NameSourceRange,
							"Unknown function: '" + call.FunctionName + "'");
						break;
					}
					//If the argument in sizeof(data-block) can be found.
					else if (schema.DataBlocks.ContainsKey(call.FunctionArgument))
					{
						if (schema.DataBlocks[call.FunctionArgument] is IBfsStructType)
							CalculateStructSize(call.SourceRange, schema.DataBlocks[call.FunctionArgument] as IBfsStructType);

						call.SizeInBytes = schema.DataBlocks[call.FunctionArgument].SizeInBytes;
						continue;
					}
					else
						BfsCompiler.ReportError(call.ArgumentSourceRange,
							"Could not find the data-block: '"+call.FunctionArgument+"'");
				}
				else if (node is BfsUnresolvedVariableExp)
				{
					BfsUnresolvedVariableExp unresolved = node as BfsUnresolvedVariableExp;

					//Boolean(true/false)?
					if (unresolved.ToString() == "true" || unresolved.ToString() == "false")
					{
						BfsBooleanExp booleanExp = new BfsBooleanExp();
						booleanExp.SourceRange = unresolved.SourceRange;
						booleanExp.Value = (unresolved.ToString() == "true") ? true : false;
						group.Members.Insert(index, booleanExp);
						group.Members.Remove(unresolved);
						continue;
					}

					//Value expression?
					if (unresolved.ToString() == "value")
					{
						if (block is IBfsConsumptionType)
						{
							BfsValueExp valueExp = new BfsValueExp();
							valueExp.SourceRange = unresolved.SourceRange;
							group.Members.Insert(index, valueExp);
							group.Members.Remove(unresolved);
							continue;
						}
						else
						{
							BfsCompiler.ReportError(unresolved.SourceRange,
								"The 'value' expression variable may only be used in consumed types.");
							continue;
						}
					}

					//Else it is a named variable.
					BfsExpressionVariable namedVar = new BfsExpressionVariable();

					namedVar.SourceRange = node.SourceRange;

					group.Members.Insert(index, namedVar);
					group.Members.Remove(node);

					string containertype = block.ToString();

					//Check hiearchy
					IBfsDataBlock container = block;
					for (int i = 0; i < unresolved.NameHierarchy.Count; i++)
					{
						string varname = unresolved.NameHierarchy[i];
						if (container == null)
						{
							BfsCompiler.ReportError(node.SourceRange,
								"Variable '" + unresolved.NameHierarchy[i - 1] + "' cannot contain any variables because it of type: " + containertype);
							break;
						}

						if (container.LocalFields.ContainsKey(varname))
						{
							namedVar.NameHierarchy.Add(container.LocalFields[varname]);
							containertype = " local variable";
							container = null;
						}
						else if (container is IBfsStructType && (container as IBfsStructType).StructFields.ContainsKey(varname))
						{
							IBfsType type = (container as IBfsStructType).StructFields[varname].FieldType;
							IBfsNamedField namedtype = (container as IBfsStructType).StructFields[varname];

							if (type is BfsNamedType)
							{
								container = (type as BfsNamedType).DataBlock;

								if (container is IBfsConsumptionType)
								{
									IBfsConsumptionType consumed = container as IBfsConsumptionType;
									namedtype.PrimitiveType = consumed.PrimitiveType;
								}

								namedVar.NameHierarchy.Add(namedtype);
							}
							else if (type is BfsPrimitiveType)
							{
								containertype = "primitive type";
								namedVar.NameHierarchy.Add(namedtype);
								container = null;
							}
							else if (type is BfsFunctionType)
							{
								containertype = "function type";
								namedVar.NameHierarchy.Add(namedtype);
								container = null;
							}
							else BfsCompiler.ReportError(container.SourceRange, "Unexpected error. Unknown type.");
						}
						else
						{
							containertype = "unknown variable or enum-alias.";
							BfsExpressionUnknown unknown = new BfsExpressionUnknown();
							unknown.Name = varname;
							unknown.SourceRange = unresolved.SourceRange;
							namedVar.NameHierarchy.Add(unknown);
							container = null;
						}
					}
					//Building map over dependant variables.
					exp.DependantVariables.Add(namedVar);
				}
			}
		}

		private void CheckForEnumMembers(BfsExpGroup group, IBfsDataBlock block)
		{
            //For each expression variable in the expression group
			for (int index = 0; index < group.Members.Count; index++)
			{
                //Recursively visit the sub-expressions
				IBfsExpNode node = group.Members[index];
				if (node is BfsExpGroup)
				{
					CheckForEnumMembers(node as BfsExpGroup, block);
					continue;
				}

                //Ignore irrelevant objects (operators and known variables)
				if (!(node is BfsExpressionVariable))
					continue;
				BfsExpressionVariable expressionvar = node as BfsExpressionVariable;
                if (!(expressionvar.LastField is BfsExpressionUnknown))
                    continue;

                //Only interested in resolving the BfsExpressionUnknowns
				BfsExpressionUnknown unknown = expressionvar.LastField as BfsExpressionUnknown;

				BfsExpressionVariable candidatevar = null;
				bool resolvedTheVar = false;

				BfsOperator op = null;

                if (index >= 2 && group.Members[index - 2] is BfsExpressionVariable)
                {
                    op = group.Members[index - 1] as BfsOperator;
                    candidatevar = group.Members[index - 2] as BfsExpressionVariable;
                }
                else if (index + 2 < group.Members.Count && group.Members[index + 2] is BfsExpressionVariable)
                {
                    op = group.Members[index + 1] as BfsOperator;
                    candidatevar = group.Members[index + 2] as BfsExpressionVariable;
                }
                

				if ( candidatevar != null
					&& candidatevar.LastField is BfsStructField
					&& (candidatevar.LastField as BfsStructField).FieldType is BfsNamedType)
				{
					BfsStructField field = candidatevar.LastField as BfsStructField;
					BfsNamedType fieldtype = field.FieldType as BfsNamedType;

					if (fieldtype.DataBlock is BfsEnum && (fieldtype.DataBlock as BfsEnum).EnumAliases.ContainsKey(unknown.Name))
					{
						resolvedTheVar = true;
						BfsEnumFieldAlias enumalias = (fieldtype.DataBlock as BfsEnum).EnumAliases[unknown.Name];
						BfsEnumAliasExp aliasExp = new BfsEnumAliasExp();
                        aliasExp.EnumBlock = fieldtype.DataBlock as BfsEnum;
						aliasExp.Alias = enumalias;
						aliasExp.SourceRange = unknown.SourceRange;
						
                        //Only allow equality operators on EnumAliases
						if (op.Operator == "==" || op.Operator == "!=")
						{
							group.Members.Insert(index, aliasExp);
							group.Members.Remove(node);
						}
						else
							BfsCompiler.ReportError(op.SourceRange,
								"The operator '"+op.Operator+"' cannot be used with the enum alias '"+enumalias.Name+"'. Only == and != are allowed.");

                        expressionvar.LastField = aliasExp;
					}
				}

				if(!resolvedTheVar)
					BfsCompiler.ReportError(unknown.SourceRange,
						"Unresolved variable name: '" + unknown.Name + "'");
			}
		}

		private void CalculateStructSize(BfsSourceRange range, IBfsStructType structtype)
		{
			int totalstructsize = 0;
			foreach (BfsStructField field in structtype.StructFieldList)
			{
				if (field.PrimitiveType.ArrayExtension == null)
					totalstructsize += Environments.GetSizeOfPrimitiveType(field.PrimitiveType);
				else
					if (field.PrimitiveType.ArrayExtension is BfsKnownArray)
					{
						BfsKnownArray known = field.PrimitiveType.ArrayExtension as BfsKnownArray;
						BfsExpGroup expgroup = known.Expression.ExpressionGroup;
						if (expgroup.Members.Count == 1 && expgroup.Members[0] is BfsNumberExp)
						{
							totalstructsize += Environments.GetSizeOfPrimitiveType(field.PrimitiveType)
								* (int)(expgroup.Members[0] as BfsNumberExp).Value;
							//BfsCompiler.ReportMessage("Known array of size: " + (expgroup.Members[0] as BfsNumberExp).Value);
						}
						else
							BfsCompiler.ReportError(range,
								"Could not get size of block '"+structtype.Name+"' as it has a field with non-simple array-extension.");
					}
					else
					{
						totalstructsize = 0;
						BfsCompiler.ReportError(range,
							"Cannot get size of a block with unknown size (because of array extension in: '" + structtype.Name + "." + field.Name + "')");
						break;
					}
			}
			structtype.SizeInBytes = totalstructsize;
			//BfsCompiler.ReportWarning(structtype.SourceRange, "Total size of struct: "+ totalstructsize);
		}

	}
}
