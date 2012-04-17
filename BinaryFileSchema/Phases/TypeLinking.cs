using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFSchema
{
	class TypeLinking : IPhase
	{
		public void Check(BinaryFileSchema schema)
		{
			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				//Link all unresolved field-types to their declarations (data-blocks).
				if (block is IBfsStructType)
				{
					IBfsStructType structType = block as IBfsStructType;
					foreach (BfsStructField field in structType.StructFields.Values)
						if (field.FieldType is BfsUnresolvedNamedType)
						{
							BfsUnresolvedNamedType oldtype = (BfsUnresolvedNamedType)field.FieldType;
							if (schema.DataBlocks.ContainsKey(oldtype.Name))
							{
								BfsNamedType newtype = new BfsNamedType();
								newtype.DataBlock = schema.DataBlocks[oldtype.Name];
								newtype.ArrayExtension = oldtype.ArrayExtension;
								newtype.SourceRange = oldtype.SourceRange;
								field.FieldType = newtype;
							}
							else BfsCompiler.ReportError(oldtype.SourceRange, "Could not resolve '" + oldtype.Name + "' to a type.");
						}
				}
				else if (block is BfsEnum)
					foreach (BfsEnumField field in (block as BfsEnum).EnumFields)
						CheckActionList(field.Actions, block);
				else if (block is BfsBitfield)
					foreach (BfsBitfieldField field in (block as BfsBitfield).BitFieldFields)
						CheckActionList(field.Actions, block);
			}
		}



		private static void CheckActionList(IList<IBfsAction> actions, IBfsDataBlock block)
		{
			if (actions == null)
				return;

			for( int index = 0; index < actions.Count; index++)
			{
				IBfsAction action = actions[index];

				if (action is BfsActionUnresolvedAssignment)
				{
					BfsActionUnresolvedAssignment unresolved = action as BfsActionUnresolvedAssignment;
					BfsActionAssignment assignment = new BfsActionAssignment();
					assignment.Expression = unresolved.Expression;
					assignment.SourceRange = unresolved.SourceRange;
					if (block.LocalFields.ContainsKey(unresolved.UnresolvedVariableName))
						assignment.LocalVariable = block.LocalFields[unresolved.UnresolvedVariableName];
					else
						BfsCompiler.ReportError(assignment.SourceRange,
							"Could not find local variable: '"+unresolved.UnresolvedVariableName+"'");

					actions.Insert(index, assignment);
					actions.Remove(action);
				}
			}
		}


	}
}
