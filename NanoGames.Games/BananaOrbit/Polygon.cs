// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;

namespace NanoGames.Games.BananaOrbit
{
    /// <summary>
    /// A closed polygon, orientated clockwise, edges cannot intersect.
    /// </summary>
    internal class Polygon
    {
        /// <summary>
        /// The list of all points.
        /// </summary>
        public List<Vector> Points;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon"/> class.
        /// </summary>
        public Polygon()
        {
            Points = new List<Vector> { };
        }

        /// <summary>
        /// Gets the first point in the polygon.
        /// </summary>
        public Vector FirstPoint => Points[0];
        
        /// <summary>
        /// Gets the last point in the polygon.
        /// </summary>
        public Vector LastPoint => Points[N - 1];

        /// <summary>
        /// Gets the number of points in the polygon.
        /// </summary>
        public int N => Points.Count;

        /// <summary>
        /// Gets the edges of the polygon.
        /// </summary>
        public List<Segment> Edges => GetEdges();

        /// <summary>
        /// Gets the list of unit normals to the edges of the polygon, pointing outward.
        /// </summary>
        public List<Vector> Normals => GetNormals();

        /// <summary>
        /// Gets the area of the polygon.
        /// </summary>
        public double Area => GetArea();

        /// <summary>
        /// Returns the list of the edges of the polygon.
        /// </summary>
        /// <returns>The edges.</returns>
        public List<Segment> GetEdges()
        {
            int k;
            List<Segment> edges = new List<Segment> { };

            for (int i = 0; i < N; i++)
            {
                k = (i + 1) % N;
                edges.Add(new Segment(Points[i], Points[k]));
            }

            return edges;
        }

        /// <summary>
        /// Returns the list of the normals to the edges of the polygon.
        /// </summary>
        /// <returns>The edges.</returns>
        public List<Vector> GetNormals()
        {
            List<Vector> normals = new List<Vector> { };

            for (int i = 0; i < N; i++)
            {
                normals.Add(Edges[i].Normal);
            }

            return normals;
        }

        /// <summary>
        /// Returns the area of the polygon.
        /// </summary>
        /// <returns>The area.</returns>
        public double GetArea()
        {
            int k;
            double area = 0;

            for (int i = 0; i < N; i++)
            {
                k = (i + 1) % Points.Count;
                area += (Points[i].X * (-Points[k].Y) - Points[k].X * -(Points[i].Y)) / 2;
            }

            return Math.Abs(area);
        }

        /// <summary>
        /// Adds a point to the polygon.
        /// </summary>
        public void Add(Vector point)
        {
            foreach (Vector oldPoint in Points)
            {
                if (oldPoint.Length < Constants.epsilon)
                {
                    throw new InvalidOperationException("Point is aready in Polygon");
                }
            }

            Points.Add(point);

        }

        /// <summary>
        /// Rotates the polygon counterclockwise around a given point.
        /// </summary>
        /// <param name="origin">The origin of the rotation.</param>
        /// <param name="angle">The angle of the rotation.</param>
        public void Rotate(double angle, Vector origin)
        {
            for (int i = 0; i < N; i++)
            {
                Points[i] = Points[i].RotateAngle(angle, origin);
            }

        }

        /// <summary>
        /// Draws the polygon.
        /// </summary>
        public void Draw(IGraphics g, Color c)
        {
            foreach (Segment edge in Edges)
            {
                edge.Draw(g, c);
            }

        }

        /// <summary>
        /// Draws all the things from the polygon.
        /// </summary>
        public void DrawDebug(IGraphics g, Color c)
        {
            /* Draw all points*/
            int i = 0;
            foreach (Vector point in Points)
            {
                g.Circle(c, point, 0.5);
                g.Print(c, 3, point - new Vector(0, -2), i.ToString());
                i++;
            }

            /* Draw all edges*/
            foreach (Segment edge in Edges)
            {
                edge.DrawDebug(g, c);
            }

            /*print area, number of points*/
            g.Print(c, 3, Points[0] + new Vector(0, -10), "AREA: " + Area.ToString());
            g.Print(c, 3, Points[0] + new Vector(0, -7), "N: " + N.ToString());
        }

    }
}
