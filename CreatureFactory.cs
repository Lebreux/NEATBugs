using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class CreatureFactory
    {
        public Creature Create(Genome genome)
        {
            Creature creature = new Creature();
            creature.Genome = genome;
            foreach (var node in genome.Nodes)
            {
                switch (node.NodeType)
                {
                    case NodeTypes.Sensor:
                        creature.Sensors.Add(node.Id);
                        break;
                    case NodeTypes.Output:
                        creature.Outputs.Add(node.Id);
                        break;
                    case NodeTypes.Hidden:
                        creature.Hiddens.Add(node.Id);
                        break;
                }
            }
            foreach (var connect in genome.Connects)
            {
                if (!connect.Enabled)
                    continue;
                if (!creature.Links.TryGetValue(connect.Out, out var @out))
                {
                    @out = new Dictionary<int, float>();
                    creature.Links[connect.Out] = @out;
                }
                @out[connect.In] = connect.Weight;
            }
            creature.NodeValues = new float[genome.Nodes.Count];
            return creature;
        }
    }
}
