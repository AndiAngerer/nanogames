﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoGames.Games.KartRace
{
    class KartMatch : Match<KartPlayer>
    {
        public readonly List<TrackLine> TrackLines = new List<TrackLine>();

        private readonly double[] _offsets = new double[Constants.TrackAmplitudes.Length];

        public Vector GetTrackTangent(double angle)
        {
            return Rotation.FromRadians(angle) * new Vector(GetTrackDerivative(angle), 1).Normalized;
        }

        protected override void Initialize()
        {
            for (int i = 0; i < Constants.TrackAmplitudes.Length; ++i)
            {
                _offsets[i] = Random.NextDouble() * 2 * Math.PI;
            }

            double sf = Math.PI * 2 / Constants.TrackSegments;
            double lf = sf / Constants.TrackSegmentLines;
            for (int s = 0; s < Constants.TrackSegments; ++s)
            {
                double sa = s * sf;
                var sc = Constants.TrackSegmentColors[s % Constants.TrackSegmentColors.Length];

                for (int l = 0; l < Constants.TrackSegmentLines; ++l)
                {
                    double a1 = sa + l * lf;

                    double r1 = GetTrackRadius(a1);
                    double cos1 = Math.Cos(a1);
                    double sin1 = Math.Sin(a1);

                    double a2 = a1 + lf;
                    double r2 = GetTrackRadius(a2);
                    double cos2 = Math.Cos(a2);
                    double sin2 = Math.Sin(a2);

                    TrackLines.Add(
                        new TrackLine(
                            sc,
                            new Vector(cos1 * r1, sin1 * r1),
                            new Vector(cos2 * r2, sin2 * r2)));

                    r1 += Constants.TrackWidth;
                    r2 += Constants.TrackWidth;

                    TrackLines.Add(
                        new TrackLine(
                            sc,
                            new Vector(cos1 * r1, sin1 * r1),
                            new Vector(cos2 * r2, sin2 * r2)));
                }
            }

            {
                double r = GetTrackRadius(0);
                TrackLines.Add(
                        new TrackLine(
                            Constants.TrackSegmentColors[0],
                            new Vector(r, 0),
                            new Vector(r + Constants.TrackWidth, 0)));
            }

            var startPos = new Vector(Constants.TrackWidth * 0.5 + GetTrackRadius(0), 0);

            for (int i = 0; i < Players.Count; ++i)
            {
                var startAngle = -Constants.StartAngle;
                var startRadius = GetTrackRadius(startAngle) + (i + 1) * Constants.TrackWidth / (Players.Count + 1);

                Players[i].MaxAngle = startAngle + 2 * Math.PI;
                Players[i].Position = Vector.FromAngle(startAngle) * startRadius;

                var direction = GetTrackTangent(startAngle);
                Players[i].Direction = Rotation.FromVector(direction);
            }
        }

        protected override void Update()
        {
            foreach (var player in Players)
            {
                if (player.HasFinished) continue;

                if (player.Input.Left.IsPressed)
                {
                    player.Direction *= Constants.TurnSpeed;
                }

                if (player.Input.Right.IsPressed)
                {
                    player.Direction *= Constants.TurnSpeed.Inverse;
                }

                if (player.Input.Up.IsPressed)
                {
                    player.Velocity += Constants.Acceleration * player.Direction.ToVector();
                }

                if (player.Input.Down.IsPressed)
                {
                    player.Velocity -= Constants.Acceleration * player.Direction.ToVector();
                }

                player.Velocity = LimitSpeed(player.Velocity);

                player.Position += player.Velocity;

                var playerA = Math.Atan2(player.Position.Y, player.Position.X);
                if (playerA < 0) playerA += 2 * Math.PI;

                if (player.MaxAngle < playerA && playerA < player.MaxAngle + 0.2)
                {
                    player.MaxAngle = playerA;
                }

                if (playerA < 0.1 && player.MaxAngle > 6.1)
                {
                    player.MaxAngle = playerA;
                    ++player.Round;
                    if (player.Round == Constants.Rounds)
                    {
                        player.HasFinished = true;
                        player.Score = -Frame;
                    }
                }

                var playerR = player.Position.Length;
                var trackR = GetTrackRadius(playerA);
                var trackD = GetTrackDerivative(playerA);
                var trackNormal = Rotation.FromRadians(playerA) * new Vector(-1, trackD).Normalized;

                var dot = Vector.Dot(player.Velocity, trackNormal);

                if ((playerR < trackR && dot > 0) || (playerR > trackR + Constants.TrackWidth && dot < 0))
                {
                    player.Velocity -= 2 * dot * trackNormal;
                    player.Velocity *= Constants.TrackBumpElasticity;
                }
            }

            foreach (var player in Players)
            {
                if (player.HasFinished) continue;

                /* Check for collisions. */
                foreach (var otherPlayer in Players)
                {
                    var relativePosition = otherPlayer.Position - player.Position;

                    if (otherPlayer != player
                        && !otherPlayer.HasFinished
                        && relativePosition.Length < 2 * Constants.PlayerRadius)
                    {
                        /*
                         * We overlap with the other player.
                         * This is only a collision if the players are moving towards each other, otherwise, let them move apart naturally.
                         */

                        var relativeVelocity = otherPlayer.Velocity - player.Velocity;

                        /* The dot product tells us if the vectors are oriented towards each other. */
                        if (Vector.Dot(relativePosition, relativeVelocity) < 0)
                        {
                            /*
                             * The other player is moving towards us, relatively.
                             * Compute the result of a perfectly elastic condition.
                             */

                            var velocityExchange = Vector.Dot(relativeVelocity, relativePosition.Normalized) * relativePosition.Normalized;
                            player.Velocity = LimitSpeed(player.Velocity + velocityExchange);
                            otherPlayer.Velocity = LimitSpeed(otherPlayer.Velocity - velocityExchange);
                        }
                    }
                }
            }

            foreach (var player in Players)
            {
                if (player.HasFinished) continue;
                player.Render();
            }

            var activePlayers = Players.Count(p => !p.HasFinished);
            if (Players.Count <= 1 ? activePlayers == 0 : activePlayers <= 1)
            {
                IsCompleted = true;
            }
        }

        private static Vector LimitSpeed(Vector velocity)
        {
            var speed = velocity.Length;
            if (speed > Constants.MaxSpeed)
            {
                return velocity * (Constants.MaxSpeed / speed);
            }
            else
            {
                return velocity;
            }
        }

        private double GetTrackRadius(double angle)
        {
            double r = Constants.TrackRadius;

            for (int i = 0; i < Constants.TrackAmplitudes.Length; ++i)
            {
                r += Math.Sin(angle * (i + 1) + _offsets[i]) * Constants.TrackAmplitudes[i];
            }

            return Math.Max(Constants.MinTrackRadius, r);
        }

        private double GetTrackDerivative(double angle)
        {
            double d = 0;
            for (int i = 0; i < Constants.TrackAmplitudes.Length; ++i)
            {
                d += Math.Cos(angle * (i + 1) + _offsets[i]) * (i + 1) * Constants.TrackAmplitudes[i];
            }

            return d / Constants.TrackRadius;
        }
    }
}
