using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class Genome
    {
        public float Hue { get; set; }
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Connect> Connects { get; set; } = new List<Connect>();

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
