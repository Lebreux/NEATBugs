using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal static class Utils
    {
        public static Random R = new Random();

        public static T RandomChoose<T>(IEnumerable<T> list)
        {
            if (list == null)
                return default(T);
            if (list.Count() == 0)
                return default(T);
            return list.ElementAt(R.Next(0, list.Count()));
        }

        public static HashSet<(int @in, int @out)> GetPairs(List<Node> nodes)
        {
            var pairs = new HashSet<(int, int)>();
            var dctNodes = nodes.GroupBy(x => x.NodeType).ToDictionary(x => x.Key, x => x.ToList());
            var sensors = dctNodes.TryGetValue(NodeTypes.Sensor, out var s) ? s : new List<Node>();
            var outputs = dctNodes.TryGetValue(NodeTypes.Output, out var o) ? o : new List<Node>();
            var hiddens = dctNodes.TryGetValue(NodeTypes.Hidden, out var h) ? h : new List<Node>();
            foreach (var sensor in sensors)
            {
                foreach (var hidden in hiddens)
                    pairs.Add((sensor.Id, hidden.Id));
                foreach (var output in outputs)
                    pairs.Add((sensor.Id, output.Id));
            }
            foreach (var hidden in hiddens)
            {
                foreach (var hidden2 in hiddens)
                    if (hidden.Id < hidden2.Id)
                        pairs.Add((hidden.Id, hidden2.Id));
                foreach (var output in outputs)
                    pairs.Add((hidden.Id, output.Id));
            }
            return pairs;
        }

        public static float RandomWeight()
        {
            return (float)R.NextDouble() * 2f - 1f;
        }

        public static bool RandomCheck(float percent)
        {
            return R.NextDouble() < percent;
        }

        public static float RandomFloat()
        {
            return (float)R.NextDouble();
        }

        public static float RandomBetween(float min, float max)
        {
            return (max - min) * (float)R.NextDouble() + min;
        }

        public static float RandomNormal(float mean, float std)
        {
            return (float)(mean + std * (Math.Sqrt(-2.0 * Math.Log(R.NextDouble())) * Math.Sin(2.0 * Math.PI * R.NextDouble())));
        }

        public static float Sigmoid(float val)
        {
            return (float)Math.Tanh(val);
        }

        public static float Scale(float valueIn, float baseMin, float baseMax, float limitMin, float limitMax)
        {
            return ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static float Wrap(float value, float min, float max)
        {
            return (value + max) % max;
        }

        public static float DegreeToRadian(float value)
        {
            return (float)(Math.PI / 180f * value);
        }

        public static float RadianToDegree(float value)
        {
            return 180f / (float)Math.PI * value;
        }

        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            var ca = (float)Math.Cos(radians);
            var sa = (float)Math.Sin(radians);
            return new Vector2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }

        public static Vector2f ToV2f(this Vector2 v)
        {
            return new Vector2f(v.X, v.Y);
        }

        public static Vector2 Normalize(this Vector2 v)
        {
            return Vector2.Normalize(v);
        }

        public static float Cycle(float t, float b)
        {
            return (float)(Math.Sqrt((1f + b * b) / (1f + b * b * Math.Cos(t) * Math.Cos(t))) * Math.Cos(t));
        }

        public static Text Center(this Text t)
        {
            var b = t.GetLocalBounds();
            t.Origin = new Vector2f(
                b.Left + b.Width / 2.0f,
                b.Top + b.Height / 2.0f).Floor();
            return t;
        }

        public static Vector2f Floor(this Vector2f v)
        {
            return (Vector2f)(Vector2i)v;
        }

        public static (byte r, byte g, byte b) HSVToRGB(float h, float s, float v)
        {
            var rgb = new byte[3];

            var baseColor = ((int)h + 60) % 360 / 120;
            var shift = (h + 60f) % 360f - (120f * baseColor + 60f);
            var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;

            //Setting Hue
            rgb[baseColor] = 255;
            rgb[secondaryColor] = (byte)(Math.Abs(shift) / 60.0f * 255.0f);

            //Setting Saturation
            for (var i = 0; i < 3; i++)
                rgb[i] += (byte)((255f - rgb[i]) * ((100f - s) / 100.0f));

            //Setting Value
            for (var i = 0; i < 3; i++)
                rgb[i] -= (byte)(rgb[i] * (100f - v) / 100.0f);

            return (rgb[0], rgb[1], rgb[2]);
        }
    }
}
