#region

using Quiver;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    internal class consoleGame : IState
    {
        public void Init()
        {
        }

        void IState.Focus()
        {
        }

        public void Update()
        {
            cmd.Checkbinds();

            if (input.IsKeyPressed(Key.Escape)) statemanager.GoBack();
        }

        public void Dispose()
        {
        }

        void IState.Render()
        {
            cache.GetTexture("gui/console", true).Draw(0, 0);
            gui.Write("[ESC] BACK", 2, 82);
        }

        /*
        private uint[,] board;

        public Console_game()
        {
            board = new uint[bw, bh];
            InitPieces();

            NextPiece();
        }

        public void Init()
        {

        }

        private int peice_next = 0;
        private int peice_cur = 0;

        private int score = 0;

        private static uint nr = 0;
        private static uint nx = 0;
        private static uint ny = 0;

        private static uint mr = 0;
        private static uint mx = 0;
        private static uint my = 0;

        private const uint bw = 10;
        private const uint bh = 20;
        private const uint bx = 55;
        private const uint by = 4;
        void IState.Render()
        {
            Cache.GetTexture("gui/tetro", true).Draw(0 ,0);

            Gui.Write("SCORE", 12, 11);
            Gui.Write(score, 33, 11, Color.Green);

            Gui.Write("NEXT", 12, 20);

            for (uint x = 0; x < bw; x++)
            {
                for (uint y = 0; y < bh; y++)
                {
                    // foreach cell
                    DrawCell(bx + (x * 4), by + (y * 4), board[x, y]);
                }
            }
        }

        Color[] cols = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Magenta, };

        void NextPiece()
        {
            Random r = new Random();

            peice_cur = r.Next(0, pieces.Count - 1);
            AddPiece(peice_cur, Engine.UintColor(cols[r.Next(0, cols.Length - 1)]));
        }

        void DrawCell(uint x, uint y, uint col)
        {
            for (uint sx = 0; sx < 4; sx++)
            {
                for (uint sy = 0; sy < 4; sy++)
                {
                    Screen.SetPixel(x + sx, y + sy, col);
                }
            }
        }

        void Step()
        {
            StepDrop();
        }

        bool StepDrop()
        {
            bool[,] pd = pieces[peice_cur];

            bool blocked = IsBlocked(ref pd);

            if (!blocked)
            {
                uint col = 0;
                for (uint sx = 0; sx < GetBlockWidth(pd, nr); sx++)
                {
                    for (uint sy = 0; sy < GetBlockHeight(pd, nr); sy++)
                    {
                        if (GetBlockData(pd, sx, sy, nr))
                        {
                            col = board[nx + sx, ny + sy];
                            board[nx + sx, ny + sy] = 0;
                        }
                    }
                }

                for (uint sx = 0; sx < GetBlockWidth(pd, mr); sx++)
                {
                    for (uint sy = 0; sy < GetBlockHeight(pd, mr); sy++)
                    {
                        if (GetBlockData(pd, sx, sy, mr))
                        {
                            board[mx + sx, ny + sy + 1] = col;
                        }
                    }
                }

                ny++;
                nx = mx;
            }
            else
            {
                score += 10;
                NextPiece();
            }

            return blocked;
        }

        bool IsBlocked(ref bool[,] pd)
        {
            uint max = GetBlockHeight(pd, mr);
            for (uint sx = 0; sx < GetBlockWidth(pd, mr); sx++)
            {
                for (uint sy = max - 1; (int)sy >= 0; sy--)
                {
                    if (!GetBlockData(pd, sx, sy, mr)) continue;
                    if (ny + sy + 1 == bh) return true;
                    if (!(sy < max - 1) && board[nx + sx, ny + sy + 1] != 0) return true;
                }
            }

            return false;
        }

        void AddPiece(int p, uint col)
        {
            bool[,] pd = pieces[p];
            mr = 0;
            nr = mr;

            mx = (uint)(bw - (GetBlockWidth(pd, nr) - 1)) /2;
            nx = mx;

            ny = (uint)(GetBlockHeight(pd, nr) - 1);

            for (uint sx = 0; sx < GetBlockWidth(pd, nr); sx++)
            {
                for (uint sy = 0; sy < GetBlockHeight(pd, nr); sy++)
                {
                    AddCell(nx + sx, ny + sy, pd[sy, sx], col);
                }
            }
        }

        void AddCell(uint x, uint y, bool d, uint col)
        {
            if (d)
            {
                board[x, y] = col;
            }
        }

        bool GetBlockData(bool[,] d, uint x, uint y, uint rot)
        {
            if (rot == 0)
            {
                return d[y, x];
            }
            else if (rot == 1)
            {
                return d[x, y];
            }
            else if (rot == 2)
            {
                return d[y, d.GetLength(0) - x];
            }
            else if (rot == 3)
            {
                return d[x, d.GetLength(1) - y];
            }
            return false;
        }

        uint GetBlockWidth(bool[,] d, uint rot)
        {
            if (rot == 0)
            {
                return (uint)d.GetLength(1);
            }
            else if (rot == 1)
            {
                return (uint)d.GetLength(0);
            }
            else if (rot == 2)
            {
                return (uint)d.GetLength(1);
            }
            else if (rot == 3)
            {
                return (uint)d.GetLength(0);
            }
            return 0;
        }

        uint GetBlockHeight(bool[,] d, uint rot)
        {
            if (rot == 0)
            {
                return (uint)d.GetLength(0);
            }
            else if (rot == 1)
            {
                return (uint)d.GetLength(1);
            }
            else if (rot == 2)
            {
                return (uint)d.GetLength(0);
            }
            else if (rot == 3)
            {
                return (uint)d.GetLength(1);
            }
            return 0;
        }

        void IState.Update()
        {
            if (Engine.frame % 20 == 0)
            {
                Step();
            }

            Cmd.Checkbinds();

            if (Input.IsKeyPressed(Keyboard.Key.Left))
            {
                mx = (nx - 1).Clamp((uint)0, bw);
            }

            if (Input.IsKeyPressed(Keyboard.Key.Right))
            {
                mx = (nx + 1).Clamp((uint)0, bw - (uint)pieces[peice_cur].GetLength(1));
            }

            if (Input.IsKeyPressed(Keyboard.Key.Up))
            {
                mr = (nr + 1).Clamp((uint)0, (uint)4) % 4;
            }

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
            {
                Input.mouselock = true;
                Statemanager.GoBack();
            }
        }

        public void Dispose()
        {

        }

        static List<bool[,]> pieces;

        static void InitPieces()
        {
            pieces = new List<bool[,]>
            {
                new bool[1, 4]
            {
                {true, true, true, true}
            },
                new bool[2, 3]
            {
                {false, false, true},
                {true, true, true},
            },
                new bool[2, 2]
            {
                {true, true},
                {true, true}
            },
                new bool[2, 3]
            {
                {false, true, true},
                {true, true, false},
            },
                new bool[2, 3]
            {
                {false, true, false},
                {true, true, true},
            },
                new bool[2, 3]
            {
                {true, true, false},
                {false, true, true},
            }
            };
        }
        */
    }
}