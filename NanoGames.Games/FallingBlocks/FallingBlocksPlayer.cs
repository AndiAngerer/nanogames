﻿
// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;

namespace NanoGames.Games.FallingBlocks
{
    /// <summary>
    /// The player state of a falling blocks game.
    /// </summary>
    internal sealed class FallingBlocksPlayer : Player<FallingBlocksMatch>
    {
        private readonly bool[,] _isOccupied = new bool[Constants.Width, Constants.Height];

        private readonly Color[,] _blockColor = new Color[Constants.Width, Constants.Height];

        private readonly List<Color> _inbox = new List<Color>();

        private int _fallingPieceX = 0;

        private int _fallingPieceY = 0;

        private int _fallingPieceRotation;

        private byte[][,] _fallingPiece;

        private int _lockInFrame = 0;

        private int _lastResortLockInFrame = 0;

        private int _lastGravityFrame = 0;

        private bool _hasLost;

        private int _nextPiece;

        public FallingBlocksPlayer LeftPlayer { get; set; }

        public FallingBlocksPlayer RightPlayer { get; set; }

        public bool HasLost
        {
            get
            {
                return _hasLost;
            }
        }

        public void Initialize()
        {
            Score = int.MaxValue;

            SetNextPiece();

            for (int i = 0; i < Constants.InitialGarbageLines; ++i)
            {
                _inbox.Add(new Color(0.33, 0.33, 0.33));
            }

            Output.Particles.Gravity = new Vector(0, 0.2);
            Output.Particles.Velocity = new Vector(0, -2);
            Output.Particles.Frequency = 2;
        }

        public void Update()
        {
            if (HasLost) return;

            for (int i = 0; i < _inbox.Count; ++i)
            {
                var color = _inbox[i];

                --_fallingPieceY;

                for (int x = 0; x < Constants.Width; ++x)
                {
                    if (_isOccupied[x, 0])
                    {
                        SetLost();
                    }

                    for (int y = 0; y < Constants.Height - 1; ++y)
                    {
                        _isOccupied[x, y] = _isOccupied[x, y + 1];
                        _blockColor[x, y] = _blockColor[x, y + 1];
                    }

                    _isOccupied[x, Constants.Height - 1] = true;
                    _blockColor[x, Constants.Height - 1] = color;
                }

                _isOccupied[Match.Random.Next(Constants.Width), Constants.Height - 1] = false;
                _isOccupied[Match.Random.Next(Constants.Width), Constants.Height - 1] = false;
            }

            _inbox.Clear();

            if (_fallingPiece == null)
            {
                _fallingPiece = Constants.RotatedPieces[_nextPiece];
                _fallingPieceRotation = 1;
                _fallingPieceX = (Constants.Width - _fallingPiece[_fallingPieceRotation].GetLength(0)) / 2;

                SetNextPiece();

                _fallingPieceY = -_fallingPiece[_fallingPieceRotation].GetLength(1);

                _lockInFrame = Match.Frame + Constants.LockInDelayFrames;
                _lastResortLockInFrame = Match.Frame + Constants.LastResortLockInDelayFrames;
            }

            if (_fallingPiece != null)
            {
                if (Input.Left.WasActivated)
                {
                    if (TryToMove(_fallingPieceX - 1, _fallingPieceY, _fallingPieceRotation))
                    {
                        SetHasMoved();
                    }
                }

                if (Input.Right.WasActivated)
                {
                    if (TryToMove(_fallingPieceX + 1, _fallingPieceY, _fallingPieceRotation))
                    {
                        SetHasMoved();
                    }
                }

                if (Input.Up.WasActivated)
                {
                    int rotation = (_fallingPieceRotation + 3) % 4;
                    int x = _fallingPieceX + (_fallingPiece[_fallingPieceRotation].GetLength(0) - _fallingPiece[rotation].GetLength(0)) / 2;
                    int y = _fallingPieceY + (_fallingPiece[_fallingPieceRotation].GetLength(1) - _fallingPiece[rotation].GetLength(1)) / 2;

                    if (TryToMove(x, y, rotation)
                        || TryToMove(x, y - 1, rotation)
                        || (_fallingPiece[rotation].GetLength(1) > 3 && TryToMove(x, y - 2, rotation))
                        || TryToMove(x + 1, y, rotation)
                        || TryToMove(x - 1, y, rotation)
                        || (_fallingPiece[rotation].GetLength(0) > 3 && TryToMove(x - 2, y, rotation)))
                    {
                        SetHasMoved();
                    }
                }

                if (Input.Down.WasActivated || _lastGravityFrame + Match.FallSpeed <= Match.Frame)
                {
                    DropOne(false);
                }

                if (Match.Frame >= _lockInFrame)
                {
                    DropOne(true);
                }

                if (Input.Fire.WasActivated || Match.Frame >= _lastResortLockInFrame)
                {
                    while (!HasLost && _fallingPiece != null)
                    {
                        DropOne(true);
                    }
                }
            }
        }

        public void DrawScreen()
        {
            Draw(Output.Graphics, default(Vector));
            LeftPlayer?.Draw(Output.Graphics, new Vector(-100, 0));
            RightPlayer?.Draw(Output.Graphics, new Vector(100, 0));
        }

        public void Draw(IGraphics graphics, Vector offset)
        {
            if (graphics != Output.Graphics)
            {
                graphics.PrintCenter(Color, 8, new Vector(160, 16) + offset, Name);
            }

            graphics.Line(Constants.ContainerColor, Constants.TopLeft + offset + new Vector(-Constants.ContainerBorder, -Constants.ContainerBorder), Constants.TopLeft + offset + new Vector(-Constants.ContainerBorder, Constants.Height * Constants.BlockSize + Constants.ContainerBorder));
            graphics.Line(Constants.ContainerColor, Constants.TopLeft + offset + new Vector(Constants.Width * Constants.BlockSize + Constants.ContainerBorder, -Constants.ContainerBorder), Constants.TopLeft + offset + new Vector(Constants.Width * Constants.BlockSize + Constants.ContainerBorder, Constants.Height * Constants.BlockSize + Constants.ContainerBorder));
            graphics.Line(Constants.ContainerColor, Constants.TopLeft + offset + new Vector(-Constants.ContainerBorder, Constants.Height * Constants.BlockSize + Constants.ContainerBorder), Constants.TopLeft + offset + new Vector(Constants.Width * Constants.BlockSize + Constants.ContainerBorder, Constants.Height * Constants.BlockSize + Constants.ContainerBorder));

            for (int x = 0; x < Constants.Width; ++x)
            {
                for (int y = 0; y < Constants.Height; ++y)
                {
                    if (_isOccupied[x, y])
                    {
                        DrawBlock(graphics, _blockColor[x, y], offset, x, y);
                    }
                }
            }

            if (_fallingPiece != null)
            {
                var piece = _fallingPiece[_fallingPieceRotation];
                for (int x = 0; x < piece.GetLength(0); ++x)
                {
                    for (int y = 0; y < piece.GetLength(1); ++y)
                    {
                        if (piece[x, y] != 0)
                        {
                            DrawBlock(graphics, new Color(1, 1, 1), offset, x + _fallingPieceX, y + _fallingPieceY);
                        }
                    }
                }
            }
        }

        private void SetNextPiece()
        {
            _nextPiece = Match.Random.Next(Constants.RotatedPieces.Length);
        }

        private void SetHasMoved()
        {
            _lockInFrame = Match.Frame + Constants.LockInDelayFrames;
        }

        private void SetLost()
        {
            if (!_hasLost)
            {
                _hasLost = true;
                Score = Match.Frame;
            }
        }

        private void DropOne(bool allowLockIn)
        {
            if (_fallingPiece == null || HasLost) return;

            _lastGravityFrame = Match.Frame;

            if (TryToMove(_fallingPieceX, _fallingPieceY + 1, _fallingPieceRotation))
            {
                SetHasMoved();
                return;
            }

            if (!allowLockIn)
            {
                return;
            }

            var piece = _fallingPiece[_fallingPieceRotation];
            for (int x = 0; x < piece.GetLength(0); ++x)
            {
                for (int y = 0; y < piece.GetLength(1); ++y)
                {
                    if (piece[x, y] != 0)
                    {
                        int xw = x + _fallingPieceX;
                        int yw = y + _fallingPieceY;

                        if (yw < 0)
                        {
                            SetLost();
                        }
                        else
                        {
                            _isOccupied[xw, yw] = true;
                            _blockColor[xw, yw] = Color;
                        }
                    }
                }
            }

            if (HasLost) return;

            int clearedLines = 0;
            for (int y = 0; y < piece.GetLength(1); ++y)
            {
                if (IsLineFull(y + _fallingPieceY))
                {
                    ++clearedLines;
                }
            }

            if (clearedLines > 0)
            {
                for (int y = 0; y < piece.GetLength(1); ++y)
                {
                    int yw = y + _fallingPieceY;
                    if (IsLineFull(yw))
                    {
                        for (int x = 0; x < Constants.Width; ++x)
                        {
                            if (_isOccupied[x, yw])
                            {
                                Output.Particles.Intensity = clearedLines;
                                DrawBlock(Output.Particles, 0.5 * _blockColor[x, yw], default(Vector), x, yw);
                            }
                        }

                        for (int y1 = yw; y1 > 0; --y1)
                        {
                            for (int x = 0; x < Constants.Width; ++x)
                            {
                                _isOccupied[x, y1] = _isOccupied[x, y1 - 1];
                                _blockColor[x, y1] = _blockColor[x, y1 - 1];
                            }
                        }

                        for (int x = 0; x < Constants.Width; ++x)
                        {
                            _isOccupied[x, 0] = false;
                        }
                    }
                }
            }

            Output.Audio.Play(Constants.DropSounds[clearedLines]);

            for (int i = 1; i < clearedLines; ++i)
            {
                LeftPlayer?._inbox.Add(Color);
                RightPlayer?._inbox.Add(Color);
            }

            _fallingPiece = null;
        }

        private bool IsLineFull(int yw)
        {
            for (int x = 0; x < Constants.Width; ++x)
            {
                if (!_isOccupied[x, yw])
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryToMove(int x, int y, int rotation)
        {
            if (_fallingPiece == null) return false;

            byte[,] piece = _fallingPiece[rotation];
            int w = piece.GetLength(0), h = piece.GetLength(1);

            for (int x1 = 0; x1 < w; ++x1)
            {
                for (int y1 = 0; y1 < h; ++y1)
                {
                    int xw = x + x1, yw = y + y1;
                    if (xw < 0 || xw >= Constants.Width || yw >= Constants.Height) return false;
                    if (yw >= 0 && piece[x1, y1] != 0 && _isOccupied[xw, yw]) return false;
                }
            }

            _fallingPieceX = x;
            _fallingPieceY = y;
            _fallingPieceRotation = rotation;
            return true;
        }

        private void DrawBlock(IGraphics graphics, Color color, Vector offset, int x, int y)
        {
            graphics.Rectangle(
                0.75 * color,
                Constants.TopLeft + offset + new Vector(x * Constants.BlockSize + Constants.BlockBorder, y * Constants.BlockSize + Constants.BlockBorder),
                Constants.TopLeft + offset + new Vector((x + 1) * Constants.BlockSize - Constants.BlockBorder, (y + 1) * Constants.BlockSize - Constants.BlockBorder));
        }
    }
}
