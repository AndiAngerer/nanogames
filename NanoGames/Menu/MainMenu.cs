﻿// Copyright (c) the authors of NanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace NanoGames.Menu
{
    /// <summary>
    /// The game's main menu.
    /// </summary>
    internal sealed class MainMenu : IView, IDisposable
    {
        private IView _fpsView = new FpsView();
        private IView _background = new Backgrounds.Starfield();

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void Refresh(Terminal terminal)
        {
            _fpsView.Refresh(terminal);

            terminal.Line(new Color(0.5, 0, 0), new Vector(10, 10), new Vector(200, 10));
            terminal.Line(new Color(0.5, 0, 0.5), new Vector(10, 10), new Vector(630, 350));

            terminal.Line(new Color(0, 0.5, 0.8), new Vector(10, 150), new Vector(200, 150));
            terminal.Line(new Color(0, 0.5, 0.8), new Vector(200, 150), new Vector(400, 150));

            _background.Refresh(terminal);
        }
    }
}