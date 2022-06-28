using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class MutationConnect : Mutation
    {
        public int NodeIn { get; set; }
        public int NodeOut { get; set; }

        public MutationConnect(Genome genome, int NodeIn, int NodeOut) : base(MutationTypes.Connect, genome)
        {
            this.NodeIn = NodeIn;
            this.NodeOut = NodeOut;
        }

        public override void Apply(GenomeFactory genomeFactory)
        {
            base.Apply(genomeFactory);

            this.Genome.Connects.Add(new Connect(this.NodeIn, this.NodeOut, Utils.RandomWeight(), true, genomeFactory.Innov++));
        }
    }
}
