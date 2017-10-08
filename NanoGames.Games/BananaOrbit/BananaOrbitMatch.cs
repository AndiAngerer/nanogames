// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace NanoGames.Games.BananaOrbit
{
    class BananaOrbitMatch : Match<BananaOrbitPlayer>
    {
        protected override void Initialize()
        {

        }

        protected override void Update()
        {
            DrawScreen();
        }

        private void DrawScreen()
        {
            DrawTestScreen();
        }

        private void PlayAudio()
        {

        }

        private void DrawTestScreen()
        {
            var g = Output.Graphics;

            g.Circle(new Color(1, 1, 1), new Vector(160, 100), 20);

            ConvexPolygon poly01 = new ConvexPolygon();
            ConvexPolygon poly01rot = new ConvexPolygon();
            ConvexPolygon poly02 = new ConvexPolygon();
            Segment seg01 = new Segment(new Vector(20, 100), new Vector(30, 120));
            Segment seg02 = new Segment(new Vector(20, 100), new Vector(30, 120));
            Segment seg03 = new Segment(new Vector(20, 100), new Vector(30, 120));

            poly01.Add(new Vector(20, 20));
            poly01.Add(new Vector(40, 20));
            poly01.Add(new Vector(40, 40));
            poly01.Add(new Vector(20, 40));

            poly01rot.Add(new Vector(20, 20));
            poly01rot.Add(new Vector(40, 20));
            poly01rot.Add(new Vector(40, 40));
            poly01rot.Add(new Vector(20, 40));
            poly01rot.Rotate(DegToRad(-45), new Vector(20, 60));

            poly02.Add(new Vector(100, 100));
            poly02.Add(new Vector(120, 80));
            poly02.Add(new Vector(130, 110));
            poly02.Add(new Vector(99, 130));

            seg02.Rotate(DegToRad(10), seg02.Start);
            seg03.Rotate(DegToRad(45), seg03.Start);

            poly01.DrawDebug(g, new Color(1, 1, 1));
            poly02.DrawDebug(g, new Color(1, 1, 1));
            poly01rot.DrawDebug(g, new Color(1, 1, 1));

            seg01.DrawDebug(g, new Color(1, 1, 1));
            seg02.DrawDebug(g, new Color(1, 1, 1));
            seg03.DrawDebug(g, new Color(1, 1, 1));
        }

        private double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }
    }
}
