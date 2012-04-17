using System;
using System.Collections.Generic;
using System.Text;

namespace BFSchema
{
	public class DefiniteAssignment : IPhase
	{
        Dictionary<IBfsNamedField, BfsTopologicalNode> topoDictionary = new Dictionary<IBfsNamedField, BfsTopologicalNode>();
        private BfsTopologicalNode GetTopoNode(IBfsNamedField namedField)
        {
            if (topoDictionary.ContainsKey(namedField))
                return topoDictionary[namedField];

            BfsTopologicalNode newNode = new BfsTopologicalNode(namedField);
            topoDictionary.Add(namedField, newNode);
            return newNode;
        }

    	public void Check( BinaryFileSchema schema )
		{
			
            foreach (IBfsDataBlock block in schema.DatablockList)
            {
                if (!(block is IBfsStructType))
                    continue;
               
                //Clear before each run as nodes will otherwise be seen as visited in the next data block.
                topoDictionary.Clear();

                IBfsStructType structType = block as IBfsStructType;
                BfsTopologicalSorting topological = new BfsTopologicalSorting();
                BfsTopologicalNode prevField = null;

                foreach ( BfsStructField field in structType.StructFieldList)
                {
                    BfsTopologicalNode node = GetTopoNode(field);
                    topological.Nodes.Add(node);

                    //Make each struct field dependent on the previous field.
                    if (prevField != null)
                        node.Nodes.Add(prevField);
                    else
                        prevField = node;

                    if (field.Conditional != null)
                        foreach (BfsExpressionVariable expVar in field.Conditional.DependantVariables)
                            node.Nodes.Add(GetTopoNode(expVar.LastField));

                    if (field.PrimitiveType.ArrayExtension != null && field.PrimitiveType.ArrayExtension is BfsKnownArray)
                    {
                        BfsKnownArray knownArray = field.PrimitiveType.ArrayExtension as BfsKnownArray;
                        foreach (BfsExpressionVariable expVar in knownArray.Expression.DependantVariables)
                            node.Nodes.Add(GetTopoNode(expVar.LastField));
                    }
                }

                //Add all the structure field as well as their dependancies
                foreach (BfsLocalField local in structType.LocalFieldList)
                {
                    BfsTopologicalNode node = GetTopoNode(local);
                    if(local.AssignmentExpression != null)
                        foreach (BfsExpressionVariable expVar in local.AssignmentExpression.DependantVariables)
                            node.Nodes.Add(GetTopoNode(expVar.LastField));
                    topological.Nodes.Add(node);
                }

                //Find the execution/parse order of the struct fields and local fields by performing a topological sort.
                structType.ParseOrder = topological.TopologicalSort();
           
            }

            foreach (IBfsDataBlock block in schema.DatablockList)
			{
				//Check that local-fields aren't used in their own intialization expression.
				foreach (BfsLocalField local in block.LocalFieldList)
					if (local.AssignmentExpression != null)
                        foreach (BfsExpressionVariable expvar in local.AssignmentExpression.DependantVariables)
                            if (expvar.NameHierarchy[0] == local)
                                BfsCompiler.ReportError(local.SourceRange,
                                    "Cannot use local variable in it's own initilization expression");

				if (block is IBfsStructType)
				{
					bool foundEOFcase = false;
                    BfsStructField eofCase = null;
					IBfsStructType structType = (IBfsStructType)block;
					HashSet<string> namesSeenSoFar = new HashSet<string>(structType.LocalFields.Keys);

					foreach (BfsStructField field in structType.StructFields.Values)
					{
						if (foundEOFcase)
							BfsCompiler.ReportError(field.SourceRange,
								"Array '"+eofCase.Name+"' cannot have an EOF case because there are remaining fields in the struct.");

						//Check that no variables in conditional expressions or array length expressions aren't read from the file yet.
						//Check that struct-fields aren't used in their conditional expression.
						if (field.Conditional != null)
							foreach (BfsExpressionVariable var in field.Conditional.DependantVariables)
                                if (!namesSeenSoFar.Contains(var.NameHierarchy[0].Name) && !(var.NameHierarchy[0] is BfsEnumAliasExp))
                                    BfsCompiler.ReportError(var.SourceRange,
                                        " The variable '" + var.ToString() + "' cannot be used before it is read from the file.");

                        if (field.FieldType.ArrayExtension != null && field.FieldType.ArrayExtension is BfsKnownArray)
                        {
                            BfsKnownArray knownArray = field.FieldType.ArrayExtension as BfsKnownArray;
                            foreach (BfsExpressionVariable var in knownArray.Expression.DependantVariables)
                                if (!namesSeenSoFar.Contains(var.NameHierarchy[0].Name) && !(var.NameHierarchy[0] is BfsEnumAliasExp))
                                    BfsCompiler.ReportError(var.SourceRange,
                                        " The variable '" + var.ToString() + "' cannot be used before it is read from the file.");
                        }
						namesSeenSoFar.Add(field.Name);

						//Check that no struct fields come after an unknown array extension only terminating at EOF
						//Check that no stopcases comes after an EOF
						if (field.FieldType.ArrayExtension is BfsUnknownArray)
						{
							BfsUnknownArray extension = field.FieldType.ArrayExtension as BfsUnknownArray;
							bool stopcaseafter = false;
							foreach (IBfsStopCase stopcase in extension.StopCases)
							{
								if (stopcaseafter)
									BfsCompiler.ReportWarning(stopcase.SourceRange,
										"Stopcase after an EOF will never be triggered. Move the EOF to the end of the list.");

								if (stopcase is BfsStopCaseEndOfFile)
								{
									stopcaseafter = true;
									foundEOFcase = true;
                                    eofCase = field;
								}
							}
						}
					}
				}
			}
		}
	}

    

	
}
