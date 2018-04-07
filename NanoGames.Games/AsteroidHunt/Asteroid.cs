using System;
using System.Collections.Generic;

namespace NanoGames.Games.AsteroidHunt
{
    internal class Asteroid
    {
        private double _damage;
        private bool _exploded;

        public Asteroid(AsteroidHuntMatch match, Color color, double minSize, double maxSize, double minVelocity, double maxVelocity, int minHitpoints, int maxHitpoints, bool hitpointsPlayerScaling, int baseScore)
        {
            double size = Choose(match.Random.NextDouble(), Constants.Simploid.MinSize, Constants.Simploid.MaxSize);
            double random = match.Random.NextDouble();
            double randomPow2 = random * random;
            Vector velocity = new Vector((randomPow2 -0.25) * 2, Choose(match.Random.NextDouble(), Constants.Simploid.MinVelocity, Constants.Simploid.MaxVelocity));

            Match = match;
            Color = color;
            Size = size;
            Position = new Vector(match.Random.NextDouble() * 320, -size / 2d);
            Velocity = velocity;
            Hitpoints = GetPlayerScale(match, hitpointsPlayerScaling) * Choose(match.Random.NextDouble(), minHitpoints, maxHitpoints);
            Score = baseScore + (int)Math.Round(velocity.X) + (int)Math.Round(velocity.Y / maxVelocity) + (int)Math.Round(minSize / size);
        }

        public AsteroidHuntMatch Match { private get; set; }

        public Color Color { private get; set; }

        public int Score { get; set; }

        public Vector Position { get; set; }

        public Vector Velocity { get; set; }

        public double Size { get; set; }

        public int Hitpoints { get; set; }

        public double HitpointsPlayerScaling { get; set; }

        public Action<Asteroid> ExplodeAction { get; set; }

        public bool Valid => !_exploded && Position.X > -Size / 2d && Position.X < 320 + Size / 2d && Position.Y < 200 + Size / 2d;

        public static List<Asteroid> Create(AsteroidHuntMatch match)
        {
            List<Asteroid> asteroids = new List<Asteroid>();

            var createProbability = match.Random.NextDouble();
            if (Constants.Simploid.Probability >= createProbability)
            {
                Asteroid simploid = new Asteroid(match, Constants.Simploid.Color, Constants.Simploid.MinSize, Constants.Simploid.MaxSize,
                    Constants.Simploid.MinVelocity, Constants.Simploid.MaxVelocity, Constants.Simploid.MinHitpoints, Constants.Simploid.MaxHitpoints,
                    Constants.Simploid.HitpointsPlayerScaling, Constants.Simploid.BaseScore);

                asteroids.Add(simploid);
            }
            if (Constants.Robustoid.Probability >= createProbability)
            {
                Asteroid robustoid = new Asteroid(match, Constants.Robustoid.Color, Constants.Robustoid.MinSize, Constants.Robustoid.MaxSize,
                    Constants.Robustoid.MinVelocity, Constants.Robustoid.MaxVelocity, Constants.Robustoid.MinHitpoints, Constants.Robustoid.MaxHitpoints,
                    Constants.Robustoid.HitpointsPlayerScaling, Constants.Robustoid.BaseScore);

                asteroids.Add(robustoid);
            }

            return asteroids;
        }

        internal void Hit(Bullet b)
        {
            _damage += b.Damage;
            if (_damage >= Hitpoints)
            {
                _exploded = true;
                Match.Output.Particles.Gravity = new Vector(0, 0);
                Match.Output.Particles.Velocity = Velocity;
                Match.Output.Particles.Intensity = 1;

                Match.Output.Particles.Circle(new Color(1, 0, 0), Position, Size / 2d);
            }
        }

        internal void Draw()
        {
            Match.Output.Graphics.Circle(Color, Position, Size / 2d);
        }

        private static int GetPlayerScale(AsteroidHuntMatch match, bool hitpointsPlayerScaling)
        {
            if (hitpointsPlayerScaling) return match.Players.Count;
            else return 1;
        }

        private static int Choose(double random, int minValue, int maxValue)
        {
            return (int)Choose(random, (double)minValue, (double)maxValue);
        }

        private static double Choose(double random, double minValue, double maxValue)
        {
            return minValue + random * (maxValue - minValue);
        }
    }
}
