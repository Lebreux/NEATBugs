using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class MutationNode : Mutation
    {
        public Connect Connect { get; set; }

        public MutationNode(Genome genome, Connect connect) : base(MutationTypes.Node, genome)
        {
            this.Connect = connect;
        }

        public override void Apply(GenomeFactory genomeFactory)
        {
            base.Apply(genomeFactory);

            this.Connect.Enabled = false;
            var newNode = new Node(this.Genome.Nodes.Count, NodeTypes.Hidden);
            this.Genome.Nodes.Add(newNode);
            this.Genome.Connects.Add(new Connect(this.Connect.In, newNode.Id, this.Connect.Weight, true, genomeFactory.Innov++));
            this.Genome.Connects.Add(new Connect(newNode.Id, this.Connect.Out, 1f, true, genomeFactory.Innov++));
        }
    }
}
