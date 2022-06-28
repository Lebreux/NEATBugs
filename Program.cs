using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace NEAT
{
    class Program
    {
        static uint WIDTH = 2000;
        static uint HEIGHT = 2000;
        static ulong T = 0;
        static ulong TDiff = 0;
        static ulong TLast = 0;
        static float S = 1f;
        static float Z = 0.02f;
        static bool TurboMode = false;

        static List<(int, Genome)> Scoreboard = new List<(int, Genome)>();

        static Font font = new Font(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"));
        static List<Vector2f> foods = new List<Vector2f>();
        static List<Creature> creatures = new List<Creature>();

        static Creature target = null;

        static GenomeFactory genomeFactory = new GenomeFactory();
        static CreatureFactory creatureFactory = new CreatureFactory();

        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (!TurboMode)
                        Thread.Sleep((int)(50 / S));
                    Update();
                }
            }, cts.Token);

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    TDiff = T - TLast;
                    TLast = T;
                }
            }, cts.Token);

            var w = new RenderWindow(SFML.Window.VideoMode.DesktopMode, "NEAT");
            var v = w.DefaultView;
            w.SetFramerateLimit(60);
            w.Closed += (sender, e) => { w.Close(); cts.Cancel(); };
            w.Resized += (sender, e) =>
            {
                v.Size = new Vector2f(e.Width, e.Height);
                w.SetView(v);
            };
            int x = 0, y = 0;
            w.MouseButtonPressed += (sender, e) => { x = e.X; y = e.Y; };
            w.MouseButtonReleased += (sender, e) => { v.Center += new Vector2f(x, y) - new Vector2f(e.X, e.Y); w.SetView(v); };
            w.MouseWheelScrolled += (sender, e) =>
            {
                if (e.Delta > 0)
                    v.Zoom(1f - Z);
                else if (e.Delta < 0)
                    v.Zoom(1f + Z);
                w.SetView(v);
            };
            w.KeyPressed += (sender, e) =>
            {
                switch (e.Code)
                {
                    case SFML.Window.Keyboard.Key.Add:
                        S *= 2f;
                        break;
                    case SFML.Window.Keyboard.Key.Subtract:
                        S /= 2f;
                        break;
                    case SFML.Window.Keyboard.Key.Space:
                        TurboMode = !TurboMode;
                        break;
                }
            };
            while (w.IsOpen)
            {
                w.DispatchEvents();
                Draw(w);
                w.Display();
            }
        }

        static void Init(Creature creature)
        {
            creature.Position = new Vector2(
                Utils.RandomBetween(0f, WIDTH),
                Utils.RandomBetween(0f, HEIGHT));
            creature.Direction = Vector2.Normalize(new Vector2(
                Utils.RandomBetween(-1f, 1f),
                Utils.RandomBetween(-1f, 1f)));
            creature.Size = 5f;
            creature.Energy = 50f;
        }

        static void Update()
        {
            while (creatures.Count < 10)
            {
                var genome = Scoreboard.Count > 0 && Utils.RandomFloat() < .2f ? 
                    Utils.RandomChoose(Scoreboard).Item2 : 
                    genomeFactory.Create(1, Creature.NB_INPUTS, Creature.NB_OUTPUTS, 0, 5).First();

                var creature = creatureFactory.Create(genome);
                Init(creature);
                creatures.Add(creature);
            }

            if (creatures.Count > 0)
                target = creatures[0];
            else
                target = null;

            if (Utils.RandomCheck(.2f))
            {
                var xMean = WIDTH / 2f;
                var xStd = WIDTH / 8f;
                var yMean = HEIGHT / 2f;
                var yStd = HEIGHT / 8f;
                foods.Add(new Vector2f(
                    Utils.RandomNormal(xMean, xStd),
                    Utils.RandomNormal(yMean, yStd)));
            }

            //Parallel.ForEach(creatures.ToArray(), c => UpdateCreature(c));

            foreach (var creature in creatures.ToList())
                UpdateCreature(creature);

            T++;
        }

        static void UpdateCreature(Creature creature)
        {
            if (creature == null)
                return;

            creature.Energy -= Creature.ENERGY_PER_TICK;
            creature.Energy -= (float)Math.Pow(Creature.ENERGY_PER_VELOCITY * creature.Velocity, 2);
            creature.Energy -= Creature.ENERGY_PER_NODE * creature.NodeValues.Length;

            if (creature.Energy <= 0f)
            {
                creatures.Remove(creature);
                return;
            }
            else if (creature.Energy > 80f)
            {
                creature.Energy -= 50f;

                var newCreature = Utils.RandomCheck(.2f) ?
                    creatureFactory.Create(genomeFactory.Mutate(creature.Genome)) :
                    creatureFactory.Create(creature.Genome);
                Init(newCreature);
                newCreature.Generation = creature.Generation + 1;
                newCreature.Position = creature.Position + new Vector2(Utils.RandomBetween(5, 10), Utils.RandomBetween(5, 10));
                creatures.Add(newCreature);

                if (Scoreboard.Count > 10)
                {
                    foreach (var generationGenome in Scoreboard.ToArray())
                    {
                        if (newCreature.Generation > generationGenome.Item1)
                        {
                            Scoreboard.Remove(generationGenome);
                            Scoreboard.Add((newCreature.Generation, newCreature.Genome));
                            break;
                        }
                    }
                }
                else
                    Scoreboard.Add((newCreature.Generation, newCreature.Genome));
            }

            creature.Velocity = Utils.Clamp(creature.Velocity + creature.FinalVelocityDelta, Creature.MIN_VELOCITY, Creature.MAX_VELOCITY);
            creature.Direction = creature.Direction.Rotate(creature.FinalAngleDelta);
            creature.Position += creature.Direction * creature.Velocity;
            //creature.Position = new Vector2(Utils.Wrap(creature.Position.X, 0, WIDTH), Utils.Wrap(creature.Position.Y, 0, HEIGHT));

            var minDist = 100f;
            Vector2? vClosest = null;
            foreach (var food in foods.ToArray())
            {
                var vFood = new Vector2(food.X, food.Y);
                var dist = (vFood - creature.Position).Length();
                if (dist < creature.Size + 2.5f)
                {
                    creature.Energy += 10f;
                    foods.Remove(food);
                }
                else if (dist < minDist)
                {
                    minDist = dist;
                    vClosest = vFood;
                }
            }

            if (vClosest != null)
            {
                var foodDir = (creature.Direction - (vClosest.Value - creature.Position).Normalize());
                creature.SenseFoodX = foodDir.X / 2f;
                creature.SenseFoodY = foodDir.Y / 2f;
                creature.SenseFoodD = Utils.Scale((vClosest.Value - creature.Position).Length(), 100f, 0f, 0f, 1f);
            }
            else
            {
                creature.SenseFoodX = 0f;
                creature.SenseFoodY = 0f;
                creature.SenseFoodD = 0f;
            }

            creature.SenseEnergy = Utils.Scale(creature.Energy, 0f, 100f, -1f, 1f);
            creature.SenseTime = Utils.Cycle(T / 100f, 5f);
            creature.SenseConstant = 1f;
            creature.SenseLoopback = creature.OutputLoopback;

            creature.Calc();
        }

        static void Draw(RenderWindow w)
        {
            if (!TurboMode)
            {
                var c = (byte)Utils.Scale(Utils.Cycle(T / 100f, 5), -1, 1, 0, 200);
                w.Clear(new Color(0, c, c));

                var circle = Shapes.Food;
                foreach (var food in foods.ToArray())
                {
                    circle.Position = food;
                    w.Draw(circle);
                }

                foreach (var creature in creatures.ToArray())
                    DrawCreature(w, creature);

                if (target != null)
                {
                    DrawDebug(w, target);
                    DrawNodesCircle(w, target.NodeValues, target.Sensors.ToList(), target.Outputs.ToList(), target.Links);
                }
            }
            else
            {
                w.Clear(new Color(0, 0, 0));
                w.Draw(new Vertex[] { new Vertex(new Vector2f(0, 0), Color.Red), new Vertex(new Vector2f(WIDTH, HEIGHT), Color.Red) }, PrimitiveType.Lines);
                w.Draw(new Vertex[] { new Vertex(new Vector2f(WIDTH, 0), Color.Red), new Vertex(new Vector2f(0, HEIGHT), Color.Red) }, PrimitiveType.Lines);
            }

            w.Draw(new RectangleShape(new Vector2f(WIDTH, HEIGHT)) { FillColor = Color.Transparent, OutlineColor = Color.Black, OutlineThickness = 1 });

            w.Draw(new Text("Speed: " + S.ToString(), font, 12));
            w.Draw(new Text("TPS: " + TDiff.ToString(), font, 12) { Position = new Vector2f(0, 10) });
        }

        static void DrawCreature(RenderWindow w, Creature creature)
        {
            var rgb = Utils.HSVToRGB(creature.Genome.Hue, 100, 100);

            var circle = new CircleShape(creature.Size);
            circle.FillColor = new Color(rgb.r, rgb.g, rgb.b);
            circle.Origin = new Vector2f(circle.Radius, circle.Radius);
            circle.Position = new Vector2f(creature.Position.X, creature.Position.Y);
            circle.OutlineColor = Color.Black;
            circle.OutlineThickness = 1;
            w.Draw(circle);

            w.Draw(new Vertex[] {
                new Vertex(new Vector2f(creature.Position.X, creature.Position.Y), Color.Red),
                new Vertex(new Vector2f(creature.Position.X + creature.Direction.X*10f, creature.Position.Y + creature.Direction.Y*10f), Color.Red)
            }, PrimitiveType.Lines);

            w.Draw(new Vertex[] {
                new Vertex(creature.Position.ToV2f(), Color.Green),
                new Vertex((new Vector2(creature.SenseFoodX*20f, creature.SenseFoodY*20f) + creature.Position).ToV2f(), Color.Green),
            }, PrimitiveType.Lines);

            var health = new RectangleShape(new Vector2f(2f, creature.Energy / 5f));
            health.FillColor = Color.Green;
            health.Origin = new Vector2f(0 - 5f, health.Size.Y + 5f);
            health.Position = creature.Position.ToV2f();
            if (creature.Energy > 75f)
                health.FillColor = Color.Blue;
            else if (creature.Energy > 50f)
                health.FillColor = Color.Green;
            else if (creature.Energy > 25f)
                health.FillColor = Color.Yellow;
            else
                health.FillColor = Color.Red;
            w.Draw(health);

            var text = new Text(creature.Generation.ToString(), font, 12);
            text.Position = creature.Position.ToV2f();
            w.Draw(text);
        }

        static void DrawDebug(RenderWindow w, Creature creature)
        {
            var circle = new CircleShape(5f);
            circle.OutlineColor = Color.Yellow;
            circle.OutlineThickness = 1f;
            circle.FillColor = Color.Transparent;
            circle.Origin = new Vector2f(circle.Radius, circle.Radius);
            circle.Position = new Vector2f(creature.Position.X, creature.Position.Y);
            w.Draw(circle);
        }

        static void DrawNodes(RenderWindow w, float[] nodeValues, List<int> inputs, List<int> outputs, Dictionary<int, Dictionary<int, float>> links)
        {
            var offset = new Vector2f(10f, 10f);

            var col1 = new Vector2f(0f, 0f);
            var col2 = new Vector2f(100f, 0f);
            var col3 = new Vector2f(150f, 0f);
            var col4 = new Vector2f(200f, 0f);
            var col5 = new Vector2f(250f, 0f);

            var row = new Vector2f(0f, 20f);

            var dct = new Dictionary<int, Vector2f>();

            for (var i = 0; i < inputs.Count; i++)
            {
                var r = row * i;

                Text t = null;
                switch (i)
                {
                    case 0:
                        t = new Text("SenseEnergy", font, 13);
                        break;
                    case 1:
                        t = new Text("SenseHarm", font, 13);
                        break;
                    case 2:
                        t = new Text("SenseFoodX", font, 13);
                        break;
                    case 3:
                        t = new Text("SenseFoodY", font, 13);
                        break;
                    case 4:
                        t = new Text("SenseFoodD", font, 13);
                        break;
                    case 5:
                        t = new Text("SenseTime", font, 13);
                        break;
                }
                if (t != null)
                {
                    t.Origin = new Vector2f(0f, 10f);
                    t.Position = col1 + r + offset;
                    w.Draw(t);
                }

                var c = (byte)Utils.Scale(nodeValues[inputs[i]], -1f, 1f, 0, 255);
                var s = new CircleShape(5f);
                s.Origin = new Vector2f(s.Radius, s.Radius);
                s.FillColor = new Color(c, c, c);
                s.OutlineColor = Color.White;
                s.OutlineThickness = 1f;
                s.Position = col2 + r + offset;
                w.Draw(s);

                dct[inputs[i]] = s.Position;
            }

            for (var i = 0; i < nodeValues.Length - inputs.Count - outputs.Count; i++)
            {
                var r = row * i;

                var c = (byte)Utils.Scale(nodeValues[i + inputs.Count + outputs.Count], -1f, 1f, 0, 255);
                var s = new CircleShape(5f);
                s.Origin = new Vector2f(s.Radius, s.Radius);
                s.FillColor = new Color(c, c, c);
                s.OutlineColor = Color.White;
                s.OutlineThickness = 1f;
                s.Position = col3 + r + offset + new Vector2f(0f, 10f);
                w.Draw(s);

                dct[i + inputs.Count + outputs.Count] = s.Position;
            }

            for (var i = 0; i < outputs.Count; i++)
            {
                var r = row * i;

                Text t = null;
                switch (i)
                {
                    case 0:
                        t = new Text("OutputVelocityDelta", font, 13);
                        break;
                    case 1:
                        t = new Text("OutputRotationDelta", font, 13);
                        break;
                }
                if (t != null)
                {
                    t.Origin = new Vector2f(0f, 10f);
                    t.Position = col5 + r + offset;
                    w.Draw(t);
                }

                var c = (byte)Utils.Scale(nodeValues[outputs[i]], -1f, 1f, 0, 255);
                var s = new CircleShape(5f);
                s.Origin = new Vector2f(s.Radius, s.Radius);
                s.FillColor = new Color(c, c, c);
                s.OutlineColor = Color.White;
                s.OutlineThickness = 1f;
                s.Position = col4 + r + offset;
                w.Draw(s);

                dct[outputs[i]] = s.Position;
            }

            foreach (var outputLinks in links)
            {
                var outputId = outputLinks.Key;
                foreach (var inputWeight in outputLinks.Value)
                {
                    var d = (byte)Utils.Scale(inputWeight.Value, -1f, 1f, 128, 255);

                    Color c = new Color(128, 128, 128);
                    if (inputWeight.Value < 0f)
                        c = new Color(d, 128, 128);
                    else if (inputWeight.Value > 0f)
                        c = new Color(128, d, 128);

                    var inputId = inputWeight.Key;
                    w.Draw(new Vertex[] {
                        new Vertex(dct[inputId], Color.White),
                        new Vertex(dct[outputId], c),
                    }, PrimitiveType.Lines);
                }
            }
        }

        static void DrawNodesCircle(RenderWindow w, float[] nodeValues, List<int> inputs, List<int> outputs, Dictionary<int, Dictionary<int, float>> links)
        {
            var vec = new Vector2f[nodeValues.Length];
            var radius = 100f;
            var offset = new Vector2f(20f, 20f);
            var origin = offset + new Vector2f(radius, radius);
            for (var i = 0; i < nodeValues.Length; i++)
            {
                var c = (byte)Utils.Scale(nodeValues[i], -1f, 1f, 0f, 255f);

                var circle = new CircleShape(5f);
                circle.FillColor = new Color(c, c, c);
                circle.OutlineThickness = 1;
                circle.Origin = new Vector2f(circle.Radius, circle.Radius);
                circle.Position = origin + new Vector2f(
                    (float)Math.Cos(Math.PI * 2f / nodeValues.Length * i),
                    (float)Math.Sin(Math.PI * 2f / nodeValues.Length * i)) * radius;

                if (inputs.Contains(i))
                    circle.OutlineColor = Color.Green;
                else if (outputs.Contains(i))
                    circle.OutlineColor = Color.Red;
                else
                    circle.OutlineColor = Color.White;

                w.Draw(circle);

                vec[i] = circle.Position;
            }

            foreach (var outputInputs in links)
            {
                var outputId = outputInputs.Key;
                foreach (var inputWeight in outputInputs.Value)
                {
                    var inputId = inputWeight.Key;
                    var weight = inputWeight.Value;
                    var c = new Color(128, 128, 128);
                    if (weight < 0)
                        c = new Color((byte)(Math.Abs(weight) * 127 + 128), 128, 128);
                    else if (weight > 0)
                        c = new Color(128, (byte)(Math.Abs(weight) * 127 + 128), 128);
                    w.Draw(new Vertex[] {
                        new Vertex(vec[inputId], new Color(128, 128, 128)),
                        new Vertex(vec[outputId], c)
                    }, PrimitiveType.Lines);
                }
            }

            var offsetText = new Vector2f(0f, 10f);

            w.Draw(new Text("SenseConstant", font, 12) { Position = vec[0].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseLoopback", font, 12) { Position = vec[1].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseEnergy", font, 12) { Position = vec[2].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseFoodX", font, 12) { Position = vec[3].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseFoodY", font, 12) { Position = vec[4].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseFoodD", font, 12) { Position = vec[5].Floor() + offsetText }.Center());
            w.Draw(new Text("SenseTime", font, 12) { Position = vec[6].Floor() + offsetText }.Center());
            w.Draw(new Text("OutputLoopback", font, 12) { Position = vec[7].Floor() + offsetText }.Center());
            w.Draw(new Text("OutputVelocityDelta", font, 12) { Position = vec[8].Floor() + offsetText }.Center());
            w.Draw(new Text("OutputRotationDelta", font, 12) { Position = vec[9].Floor() + offsetText }.Center());
        }
    }
}
