// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;

namespace NanoGames.Games.BananaOrbit
{
    /// <summary>
    /// A voroni diagram.
    /// </summary>
    internal class Voronoi
    {
        private List<double> x;
        private List<double> y;
        private List<double> z;
        private List<double> xc;
        private List<double> yc;
        private List<double> zc;

        public Voronoi(List<Vector> points)
        {
            /* projected points on unit elliptic paraboloid */
            foreach (Vector p in points)
            {
                x.Add(p.X);
                x.Add(p.Y);
                z.Add(p.X * p.X + p.Y * p.Y);
            }

            /* get convex hull of (x,y,z) */
            if (points.Count > 4)
            {

            }

        }
    }
}
