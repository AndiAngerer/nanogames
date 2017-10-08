// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;

namespace NanoGames.Games.Banana
{
    class Landscape
    {
        private bool[,] IsSolid = new bool[321, 201];
        private bool[,] IsBorder = new bool[321, 201];
        public List<List<Vector>> Border = new List<List<Vector>>();
        public List<List<Vector>> Normal = new List<List<Vector>>();

        public void CreateBlock(Vector p1, Vector p2)
        {
            
            int x1 = clamp((int)p1.X, 1, 319);
            int x2 = clamp((int)p2.X, 1, 319);
            int y1 = clamp((int)p1.Y, 1, 199);
            int y2 = clamp((int)p2.Y, 1, 199);

            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    IsSolid[i, j] = true;
                }
            }

        }

        public void Refresh()
        {
            IsBorder = getBorder();
            refreshBorder();
        }

        private bool[,] addLayer(bool[,] input, int count)
        {
            if (count <= 0) { return input; }

            bool[,] output = new bool[321, 201];

            for (int x = 1; x < 320; x++)
            {
                for (int y = 1; y < 200; y++)
                {
                    if (input[x, y])
                    {
                        output[x, y - 1] = true;
                        output[x + 1, y - 1] = true;
                        output[x + 1, y] = true;
                        output[x + 1, y + 1] = true;
                        output[x, y + 1] = true;
                        output[x - 1, y + 1] = true;
                        output[x - 1, y] = true;
                        output[x - 1, y - 1] = true;
                        output[x, y] = true;
                    }
                }
            }

            return addLayer(output, count - 1);
        }

        private bool[,] getBorder()
        {
            int n = IsSolid.GetLength(0);
            int m = IsSolid.GetLength(1);

            bool[,] output = new bool[n, m];

            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < m; y++)
                {
                    if (IsSolid[x, y])
                    {
                        output[x, y] = ((x < 1) ||
                                        (x > 319) ||
                                        (y < 1) ||
                                        (y > 199) ||
                                        (IsSolid[x - 1, y] == false) ||
                                        (IsSolid[x + 1, y] == false) ||
                                        (IsSolid[x, y - 1] == false) ||
                                        (IsSolid[x, y + 1] == false));
                    }
                }
            }

            return output;
        }

        private void refreshBorder()
        {
            Border.Clear();
            Normal.Clear();

            int n = IsBorder.GetLength(0);
            int m = IsBorder.GetLength(1);
            
            bool[,] wasTaken = new bool[n, m];

            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < m; y++)
                {
                    if (IsBorder[x, y] && !wasTaken[x, y])
                    {
                        int xx = x;
                        int yy = y;
                        Border.Add(new List<Vector>());
                        Normal.Add(new List<Vector>());
                        Vector s;

                        int last = 3;
                        int[,] mylist = new int[8, 2] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 } };

                        // while (!wasTaken[xx, yy])
                        do
                        {
                            wasTaken[xx, yy] = true;
                            
                            s = new Vector(0, 0);
                            for (int k = -2; k <= 2; k++)
                            {
                                for (int l = -2; l <= 2; l++)
                                {
                                    if (((xx + k) < 320) && ((xx + k) > 0) && ((yy + l) < 200) && ((yy + l) > 0))
                                    {
                                        if (IsSolid[xx + k, yy + l])
                                        {
                                            s += new Vector(k, l);
                                        }
                                    }

                                }
                            }
                            
                            Normal[Normal.Count - 1].Add(-s.Normalized);
                            Border[Border.Count - 1].Add(new Vector(xx, yy));
                           
                            for (int k = 0; k < 8; k++)
                            {
                                int loc = mod(last - 3 + k, 8);

                                if (IsBorder[xx + mylist[loc, 0], yy + mylist[loc, 1]])
                                {
                                    xx += mylist[loc, 0];
                                    yy += mylist[loc, 1];
                                    last = loc;
                                    break;
                                }
                            }

                        } while ((xx != x) || (yy != y));
                        
                    }
                }
            }
            
        } 

        public void makeCaldera(Vector position, int size)
        {
            for (int i = (int)position.X - size + 1; i <= (int)position.X + size + 1; i++)
            {
                for (int j = (int)position.Y - size + 1; j <= (int)position.Y + size + 1; j++)
                {
                    if ((position - new Vector(i, j)).Length <= size)
                    {
                        if ((i >= 0) && (i <= 320) && (j >= 0) && (j <= 200))
                        {
                            IsSolid[i, j] = false;
                        }
                    }
                }
            }

            Refresh();
        }

        public void Draw(IGraphics g)
        {
            for (int i = 0; i <= 320; i++)
            {
                for (int j = 0; j <= 200; j++)
                {
                    if (IsSolid[i,j])
                    {
                        // g.Point(new Color(0.3, 0.3, 0.3), new Vector(i, j));
                    }
                    
                    if (IsBorder[i, j])
                    {
                        // g.Point(new Color(1, 0, 0), new Vector(i, j));
                    }
                }
            }

            foreach (List<Vector> piece in Border)
            {
                for (int i = 0; i < piece.Count - 1; i++)
                {
                    g.Line(new Color(1, 1, 1), piece[i], piece[i + 1]);
                }
                
                g.Line(new Color(1, 1, 1), piece[piece.Count - 1], piece[0]);
            }
        }

        private T clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void BuildLandscape(string name)
        {
            /* the first landscape */
            if (name == "First")
            {
                CreateBlock(new Vector(100, 100), new Vector(130, 110));
                CreateBlock(new Vector(120, 120), new Vector(190, 300));
                CreateBlock(new Vector(150, 100), new Vector(300, 130));
            }
            
            if (name == "Blocks")
            {
                CreateBlock(new Vector(30, 60), new Vector(100, 90));
                CreateBlock(new Vector(150, 100), new Vector(300, 120));
                CreateBlock(new Vector(110, 160), new Vector(150, 190));
                CreateBlock(new Vector(230, 160), new Vector(280, 190));
            }

            if (name == "SimpleBlock")
            {
                CreateBlock(new Vector(100, 100), new Vector(150, 110));
            }


            Refresh();
        }
        
    }
}
