// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;

namespace NanoGames.Games.BananaOrbit
{
    /// <summary>
    /// A closed convex polygon, orientated clockwise.
    /// </summary>
    internal class ConvexPolygon : Polygon
    {  
        /// <summary>
        /// Adds a point to the polygon.
        /// </summary>
        public new void Add(Vector point)
        {
            foreach (Vector oldPoint in Points)
            {
                if (oldPoint.Length < Constants.epsilon)
                {
                    throw new InvalidOperationException("Point is aready in Polygon");
                }
            }

            foreach (Segment edge in Edges)
            {
                if (!clockwise(edge.Start, edge.Stop, point))
                {
                    // throw new InvalidOperationException("Point cannot be added, point not clockwise orientated");
                }
            }

            Points.Add(point);

        }

        private bool clockwise(Vector p1, Vector p2, Vector p3)
        {
            double tmp;
            tmp = (p2.X - p1.X) * (p2.Y - p1.Y);
            tmp += (p3.X - p2.X) * (p3.Y - p2.Y);
            tmp += (p1.X - p3.X) * (p1.Y - p3.Y);

            if (tmp >= -Constants.epsilon)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

    }
}
