// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;

namespace NanoGames.Games
{
    /// <summary>
    /// A directory of game disciplines.
    /// </summary>
    public static class DisciplineDirectory
    {
        private static readonly List<Discipline> _disciplines = new List<Discipline>();

        static DisciplineDirectory()
        {
            /* Note: names must be in ALL-CAPS to render correctly. */

            Add<AsteroidHunt.AsteroidHuntMatch, AsteroidHunt.AsteroidHuntPlayer>("ASTEROID HUNT");
            Add<Bomberguy.BomberMatch, Bomberguy.BomberGuy>("BOMBERGUY");
            Add<Example.ExampleMatch, Example.ExamplePlayer>("INTO THE SQUARE");
            Add<FallingBlocks.FallingBlocksMatch, FallingBlocks.FallingBlocksPlayer>("FALLING BLOCKS");
            Add<KartRace.KartMatch, KartRace.KartPlayer>("KART RACE");
            Add<NanoSoccer.NanoSoccerMatch, NanoSoccer.NanoSoccerPlayer>("NANO SOCCER");
            Add<Snake.SnakeMatch, Snake.SnakePlayer>("SNAKE");
            Add<Banana.BananaMatch, Banana.BananaPlayer>("BANANA");
            Add<BananaOrbit.BananaOrbitMatch, BananaOrbit.BananaOrbitPlayer>("BANANAORBIT");
        }

        /// <summary>
        /// Gets the list of disciplines.
        /// </summary>
        public static IReadOnlyList<Discipline> Disciplines => _disciplines.AsReadOnly();

        private static void Add<TMatch, TPlayer>(string name)
            where TMatch : Match<TPlayer>, new()
            where TPlayer : Player<TMatch>, new()
        {
            _disciplines.Add(new Discipline<TMatch, TPlayer>(name));
        }
    }
}
