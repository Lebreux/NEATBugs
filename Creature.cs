using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal class Creature
    {
        public static int NB_INPUTS = 7;
        public static int NB_OUTPUTS = 3;

        public static float MIN_VELOCITY_DELTA = -1f;
        public static float MAX_VELOCITY_DELTA = 1f;

        public static float MIN_ROTATION_DELTA = -1f;
        public static float MAX_ROTATION_DELTA = 1f;

        public static float MIN_VELOCITY = 0f;
        public static float MAX_VELOCITY = 10f;

        public static float MIN_ANGLE = 0f;
        public static float MAX_ANGLE = 360f;

        public static float ENERGY_PER_TICK = .1f;
        public static float ENERGY_PER_VELOCITY = .025f;
        public static float ENERGY_PER_NODE = .001f;

        public int Generation { get; set; } = 0;
        public Genome Genome { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float Size { get; set; }
        public float Velocity { get; set; }
        public float Energy { get; set; }

        public SortedSet<int> Sensors { get; set; } = new SortedSet<int>();
        public SortedSet<int> Outputs { get; set; } = new SortedSet<int>();
        public SortedSet<int> Hiddens { get; set; } = new SortedSet<int>();

        public Dictionary<int, Dictionary<int, float>> Links { get; set; } = new Dictionary<int, Dictionary<int, float>>();

        public float[] NodeValues = null;

        public float SenseConstant { get { return this.NodeValues[0]; } set { this.NodeValues[0] = value; } }
        public float SenseLoopback { get { return this.NodeValues[1]; } set { this.NodeValues[1] = value; } }
        public float SenseEnergy { get { return this.NodeValues[2]; } set { this.NodeValues[2] = value; } }
        public float SenseFoodX { get { return this.NodeValues[3]; } set { this.NodeValues[3] = value; } }
        public float SenseFoodY { get { return this.NodeValues[4]; } set { this.NodeValues[4] = value; } }
        public float SenseFoodD { get { return this.NodeValues[5]; } set { this.NodeValues[5] = value; } }
        public float SenseTime { get { return this.NodeValues[6]; } set { this.NodeValues[6] = value; } }

        public float OutputLoopback { get { return this.NodeValues[7]; } }
        public float OutputVelocityDelta { get { return this.NodeValues[8]; } }
        public float OutputRotationDelta { get { return this.NodeValues[9]; } }

        public float FinalVelocityDelta { get { return Utils.Scale(this.OutputVelocityDelta, -1f, 1f, MIN_VELOCITY_DELTA, MAX_VELOCITY_DELTA); } }
        public float FinalAngleDelta { get { return Utils.Scale(this.OutputRotationDelta, -1f, 1f, MIN_ROTATION_DELTA, MAX_ROTATION_DELTA); } }

        public Creature() { }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public void Calc()
        {
            foreach (var id in this.Hiddens)
                Calc(id);
            foreach (var id in this.Outputs)
                Calc(id);
        }

        public float Calc(int id)
        {
            this.NodeValues[id] = 0f;
            if (this.Links.TryGetValue(id, out var nodeWeights))
                foreach (var nodeWeight in nodeWeights)
                    this.NodeValues[id] += this.NodeValues[nodeWeight.Key] * nodeWeight.Value;
            this.NodeValues[id] = Utils.Sigmoid(this.NodeValues[id]);
            return this.NodeValues[id];
        }
    }
}
