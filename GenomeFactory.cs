using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class GenomeFactory
    {
        public uint Innov { get; set; } = 0;

        public GenomeFactory() { }

        public IEnumerable<Genome> Create(int n, int sensor, int output, int hidden, int connect)
        {
            for (int _ = 0; _ < n; _++)
            {
                var nodeId = 0;
                Genome genome = new Genome();
                genome.Hue = Utils.RandomBetween(0, 360);
                for (int i = 0; i < sensor; i++)
                    genome.Nodes.Add(new Node(nodeId++, NodeTypes.Sensor));
                for (int i = 0; i < output; i++)
                    genome.Nodes.Add(new Node(nodeId++, NodeTypes.Output));
                for (int i = 0; i < hidden; i++)
                    genome.Nodes.Add(new Node(nodeId++, NodeTypes.Hidden));
                var pairs = Utils.GetPairs(genome.Nodes);
                for (int i = 0; i < connect; i++)
                {
                    if (pairs.Count == 0)
                        break;

                    var pair = Utils.RandomChoose(pairs);
                    pairs.Remove(pair);

                    genome.Connects.Add(new Connect(pair.@in, pair.@out, Utils.RandomWeight(), true, Innov++));
                }

                yield return genome;
            }
        }

        public Genome Mutate(Genome genome)
        {
            Genome newGenome = JsonConvert.DeserializeObject<Genome>(JsonConvert.SerializeObject(genome));

            if (Utils.RandomFloat() < .01f) // Mutate add node
            {
                new MutationNode(genome, Utils.RandomChoose(genome.Connects)).Apply(this);
            }
            else if (Utils.RandomFloat() < .3f) // Mutate add link
            {
                var pairs = Utils.GetPairs(genome.Nodes);
                foreach (var connect in genome.Connects)
                    pairs.Remove((connect.In, connect.Out));
                if (pairs.Count > 0)
                {
                    var pair = Utils.RandomChoose(pairs);
                    new MutationConnect(genome, pair.@in, pair.@out).Apply(this);
                }
            }
            else
            {
                if (Utils.RandomFloat() < .8f) // Change weights
                {
                    new MutationWeight(genome).Apply(this);
                }
                if (Utils.RandomFloat() < .0f) // Toggle
                {
                }
                if (Utils.RandomFloat() < .0f) // Enable
                {
                }
            }

            return newGenome;
        }
    }
}
