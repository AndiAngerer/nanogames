﻿// Copyright (c) the authors of nanoGames. All rights reserved.

// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace NanoGames.Games.Bomberguy
{
    internal class BomberGuy : Player<BomberMatch>, BomberThing
    {
        public bool Destroyable
        {
            get { return true; }
        }

        public Vector Position
        {
            get; set;
        }

        public Vector Size
        {
            get; set;
        }

        public void Draw(Graphics g)
        {
            g.Line(Colors.White, Position + new Vector(Size.X / 2d, 0), Position + new Vector(Size.X, Size.Y / 2d));
            g.Line(Colors.White, Position + new Vector(Size.X, Size.Y / 2d), Position + new Vector(Size.X / 2d, Size.Y));
            g.Line(Colors.White, Position + new Vector(Size.X / 2d, Size.Y), Position + new Vector(0, Size.Y / 2d));
            g.Line(Colors.White, Position + new Vector(0, Size.Y / 2d), Position + new Vector(Size.X / 2d, 0));
        }

        internal override void Initialize()
        {
        }

        internal override void Update()
        {
            Draw(this.Graphics);
        }
    }
}
