using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFSchema
{
    public class BfsTopologicalSorting
    {
        HashSet<BfsTopologicalNode> nodes = new HashSet<BfsTopologicalNode>();
        List<IBfsNamedField> resultList;
        Stack<BfsTopologicalNode> stackVisitedNodes = new Stack<BfsTopologicalNode>();

        public HashSet<BfsTopologicalNode> Nodes { get { return nodes; } }

        public List<IBfsNamedField> TopologicalSort()
        {
            resultList = new List<IBfsNamedField>();

            foreach (BfsTopologicalNode node in nodes)
                Visit(node);

            //resultList.Reverse();
            return resultList;
        }

        private void Visit(BfsTopologicalNode node)
        {
            //If the node was found in the depth stack, report an error as this means a circular dependence was found.
            if (stackVisitedNodes.Contains(node))
            {
                List<BfsTopologicalNode> reversed = new List<BfsTopologicalNode>(stackVisitedNodes);
                reversed.Reverse();
                StringBuilder b = new StringBuilder("A circular depencence was detected: ");
                foreach (BfsTopologicalNode topoNode in reversed)
                    b.Append( topoNode.NamedField.Name + " -> ");
                b.Append(node.NamedField.Name);
                BfsCompiler.ReportError(node.NamedField.SourceRange,b.ToString());
            }

            if (!node.Visited)
            {
                node.Visited = true;
                stackVisitedNodes.Push(node);
                foreach (BfsTopologicalNode subnode in node.Nodes)
                    Visit(subnode);
                stackVisitedNodes.Pop();
                if(this.nodes.Contains(node))
                    resultList.Add(node.NamedField);
            }
        }
    }

    public class BfsTopologicalNode
    {
        public IBfsNamedField NamedField { get; set; }
        HashSet<BfsTopologicalNode> nodes = new HashSet<BfsTopologicalNode>();
        public HashSet<BfsTopologicalNode> Nodes { get { return nodes; } }

        public bool Visited { get; set; }

        public BfsTopologicalNode(IBfsNamedField field)
        {
            NamedField = field;
        }
        public override string ToString()
        {
            return NamedField.ToString();
        }
    }

    public class ByteArrayConverter
    {
        public static byte [] ConvertString(string text)
        {
            char[] chars = text.Substring(1,text.Length-2).ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < chars.Length; i++)
                bytes[i] = (byte)chars[i];
            return bytes;
        }

        public static byte[] ConvertHexString(string hex)
        {
            if (!hex.StartsWith("0x"))
                BfsCompiler.ReportError("Could not convert hex string to byte array!");

            hex = hex.Substring(2);

            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
