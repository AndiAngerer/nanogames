﻿// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace NanoGames.Games.FallingBlocks
{
    /// <summary>
    /// A match of falling blocks.
    /// </summary>
    internal sealed class FallingBlocksMatch : Match<FallingBlocksPlayer>
    {
        private int _activePlayers;

        public int Frame { get; private set; }

        public double FallSpeed { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            _activePlayers = Players.Count;

            if (Players.Count == 2)
            {
                Players[0].RightPlayer = Players[1];
                Players[1].LeftPlayer = Players[0];
            }
            else if (Players.Count > 2)
            {
                for (int i = 0; i < Players.Count; ++i)
                {
                    int j = (i + 1) % Players.Count;
                    Players[i].RightPlayer = Players[j];
                    Players[j].LeftPlayer = Players[i];
                }
            }

            foreach (var player in Players)
            {
                player.Initialize();
            }
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            ++Frame;

            foreach (var player in Players)
            {
                player.Update();
            }

            foreach (var player in Players)
            {
                if (player.HasLost && (player.LeftPlayer != null || player.RightPlayer != null))
                {
                    --_activePlayers;
                    if (player.LeftPlayer != null)
                    {
                        if (player.RightPlayer != player.LeftPlayer.LeftPlayer)
                        {
                            player.LeftPlayer.RightPlayer = player.RightPlayer;
                        }
                        else
                        {
                            player.LeftPlayer.RightPlayer = null;
                        }
                    }

                    if (player.RightPlayer != null)
                    {
                        if (player.RightPlayer.RightPlayer != player.LeftPlayer)
                        {
                            player.RightPlayer.LeftPlayer = player.LeftPlayer;
                        }
                        else
                        {
                            player.RightPlayer.LeftPlayer = null;
                        }
                    }

                    player.LeftPlayer = null;
                    player.RightPlayer = null;
                }
            }

            foreach (var player in Players)
            {
                player.DrawScreen();
            }

            if (_activePlayers <= 0 || (_activePlayers <= 1 && Players.Count >= 2))
            {
                IsCompleted = true;
            }
        }
    }
}
