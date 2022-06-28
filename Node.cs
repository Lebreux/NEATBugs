using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class Node
    {
        public int Id { get; set; }
        public NodeTypes NodeType { get; set; }

        public Node(int id, NodeTypes nodeType)
        {
            Id = id;
            NodeType = nodeType;
        }
    }
}
