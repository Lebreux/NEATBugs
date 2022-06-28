using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal abstract class Mutation
    {
        public MutationTypes MutationType { get; set; }
        public Genome Genome { get; set; }

        protected Mutation(MutationTypes mutationType, Genome genome)
        {
            MutationType = mutationType;
            Genome = genome;
        }

        public virtual void Apply(GenomeFactory genomeFactory)
        {
            this.Genome.Hue += Utils.RandomBetween(-5f, 5f);
            this.Genome.Hue = Utils.Wrap(this.Genome.Hue, 0, 360);
        }
    }
}
