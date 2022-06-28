using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class MutationWeight : Mutation
    {
        public MutationWeight(Genome genome) : base(MutationTypes.Weight, genome)
        {
        }

        public override void Apply(GenomeFactory genomeFactory)
        {
            base.Apply(genomeFactory);

            var severe = Utils.RandomFloat() < .5f;
            var tailStart = this.Genome.Nodes.Count * .8f;

            for (var i = 0; i < this.Genome.Connects.Count; i++)
            {

                float update;
                float replace;

                if (severe)
                {
                    update = .7f;
                    replace = .9f;
                }
                else if (this.Genome.Nodes.Count >= 10 && i > tailStart)
                {
                    update = .5f;
                    replace = .7f;
                }
                else
                {
                    if (Utils.RandomFloat() < .5f)
                    {
                        update = .5f;
                        replace = .0f;
                    }
                    else
                    {
                        update = .5f;
                        replace = .7f;
                    }
                }

                var connect = this.Genome.Connects[i];
                var rand = Utils.RandomFloat();
                if (rand < update) // weight shifting
                {
                    connect.Weight += Utils.RandomBetween(-1f, 1f);
                }
                else if (rand < replace) // weight replacing
                {
                    connect.Weight = Utils.RandomBetween(-1f, 1f);
                }
            }
        }
    }
}
