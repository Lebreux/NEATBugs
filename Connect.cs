namespace NEAT
{
    public class Connect
    {
        public int In { get; set; }
        public int Out { get; set; }
        public float Weight { get; set; }
        public bool Enabled { get; set; }
        public uint Innov { get; set; }

        public Connect(int @in, int @out, float weight, bool enabled, uint innov)
        {
            In = @in;
            Out = @out;
            Weight = weight;
            Enabled = enabled;
            Innov = innov;
        }
    }
}