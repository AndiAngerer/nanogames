// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace NanoGames.Games.BananaOrbit
{
    /// <summary>
    /// A 2D segment.
    /// </summary>
    internal class Segment
    {
        /// <summary>
        /// The postion vector to the startpoint.
        /// </summary>
        public Vector Start;

        /// <summary>
        /// The postion vector to the endpoint.
        /// </summary>
        public Vector Stop;

        /// <summary>
        /// Initializes a new instance of the <see cref="Segment"/> class.
        /// </summary>
        /// <param name="start">The postion vector to the startpoint.</param>
        /// <param name="stop">The postion vector to the endpoint.</param>
        public Segment(Vector start, Vector stop)
        {

            // check for start = stop
            Start = start;
            Stop = stop;
        }

        /// <summary>
        /// Gets the squared length of the segment.
        /// </summary>
        public double SquaredLength => (Start - Stop).SquaredLength;

        /// <summary>
        /// Gets the length of the segement.
        /// </summary>
        public double Length => (Stop - Start).Length;

        /// <summary>
        /// Gets the directional vector of the segment, pointing from start to stop.
        /// </summary>
        public Vector DirectionalVector => Stop - Start;

        /// <summary>
        /// Gets the midpoint of the segment.
        /// </summary>
        public Vector MidPoint => Start + 0.5 * DirectionalVector;
        
        /// <summary>
        /// Gets the normal unit vector to the segment, counterclockwise.
        /// </summary>
        public Vector Normal => new Vector(DirectionalVector.Y, -DirectionalVector.X).Normalized;

        /// <summary>
        /// Rotates the segment counterclockwise around a given point.
        /// </summary>
        /// <param name="origin">The origin of the rotation.</param>
        /// <param name="angle">The angle of the rotation.</param>
        public void Rotate(double angle, Vector origin)
        {
            Start = Start.RotateAngle(angle, origin);
            Stop = Stop.RotateAngle(angle, origin);
        }

        /// <summary>
        /// Draws the segment.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="c">Color.</param>
        public void Draw(IGraphics g, Color c)
        {
            g.Line(c, Start, Stop);
        }

        /// <summary>
        /// Draws the segment plus debug info.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="c">Color.</param>
        public void DrawDebug(IGraphics g, Color c)
        {
            g.Line(c, Start, Stop);
            g.Circle(c, Start, 1);
            g.Line(c, MidPoint, MidPoint + Normal);
        }

    }
}
