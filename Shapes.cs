using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal static class Shapes
    {
        public static CircleShape Food = new CircleShape(2.5f)
        {
            FillColor = Color.Green,
            OutlineColor = Color.Black,
            OutlineThickness = 1,
            Origin = new Vector2f(2.5f, 2.5f),
        };
    }
}
