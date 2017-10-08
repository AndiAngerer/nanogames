// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoGames.Games.Banana
{
    internal class Intersection
    {
        public bool Exists = false;
        public bool InSegment1 = false;
        public bool InSegment2 = false;
        public bool IsTrue = false;
        public Vector Point;

        private double epsilon = 0.00000000001;

        public Intersection(Vector p11, Vector p12, Vector p21, Vector p22)
        {
            p11.Y = -p11.Y;
            p12.Y = -p12.Y;
            p21.Y = -p21.Y;
            p22.Y = -p22.Y;
            
            double A1 = p12.Y - p11.Y;
            double B1 = p11.X - p12.X;
            double C1 = A1 * p11.X + B1 * p11.Y;
            double A2 = p22.Y - p21.Y;
            double B2 = p21.X - p22.X;
            double C2 = A2 * p21.X + B2 * p21.Y;

            double det = A1 * B2 - A2 * B1;

            if (det != 0)
            {
                Exists = true;

                Point.X = (B2 * C1 - B1 * C2) / det;
                Point.Y = (A1 * C2 - A2 * C1) / det;

                if (p21.X == 112 && p22.X == 113 && p21.Y ==104 && p22.Y == 105 && p11.Y > 104)
                {
                    
                }

                if ((Math.Min(p11.X, p12.X) <= Point.X + epsilon) && (Math.Max(p11.X, p12.X) >= Point.X - epsilon) &&
                    (Math.Min(p11.Y, p12.Y) <= Point.Y + epsilon) && (Math.Max(p11.Y, p12.Y) >= Point.Y - epsilon))
                {
                    InSegment1 = true;
                }

                if ((Math.Min(p21.X, p22.X) <= Point.X + epsilon) && (Math.Max(p21.X, p22.X) >= Point.X - epsilon) &&
                    (Math.Min(p21.Y, p22.Y) <= Point.Y + epsilon) && (Math.Max(p21.Y, p22.Y) >= Point.Y - epsilon))
                {
                    InSegment2 = true;
                }

                if (InSegment1 && InSegment2)
                {
                    IsTrue = true;
                }
            }
            
            p11.Y = -p11.Y;
            p12.Y = -p12.Y;
            p21.Y = -p21.Y;
            p22.Y = -p22.Y;
            Point.Y = -Point.Y;
        }

        public Intersection(Vector a, Vector b, Vector c)
        {
            Vector ab = b - a;
            Vector ac = c - a;
            double cross = ab.X * ac.Y - ab.Y * ab.X;

            IsTrue = true;
        }

    }
}
