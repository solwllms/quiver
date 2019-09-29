#region

using System.Collections.Generic;
using System.Drawing;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states
{
    internal struct linedata
    {
        public string line;
        public Color col;
    }

    public class console : IState
    {
        private const uint FIN_HEIGHT = engine.SCREEN_HEIGHT / 2;
        private const int MAX_CHARSX = 36;
        private const int SPLIT_CHARSX = 34;

        private const int LOG_SIZE = 128;
        private const int LOG_DISPLAY = 6;

        private static readonly Queue<linedata> Log = new Queue<linedata>(LOG_SIZE);
        private uint _h;

        private string _inputbuffer;
        private int _scroll;

        private bool _ticker = true;

        void IState.Init()
        {
            _inputbuffer = "";
            _h = 0;
        }

        void IState.Focus()
        {
        }

        void IState.Render()
        {
            for (uint x = 0; x < screen.width; x++)
            for (uint y = 0; y < _h; y++)
                screen.SetPixel(x, y, Color.Black);

            var logq = Log.ToArray();
            for (var i = 0; i < LOG_DISPLAY && i + _scroll < logq.Length; i++)
            {
                var d = logq[logq.Length - 1 - (i + _scroll)];
                Writeline(d.line, 2, (uint) ((i + 1) * 6) + 8, d.col);
            }

            Writeline("> " + _inputbuffer + (_ticker ? "_" : ""), 2, 8);


            if (engine.frame % 10 == 0)
                _ticker = !_ticker;
            _h = (_h + 3).Clamp((uint) 0, FIN_HEIGHT);
        }

        void IState.Update()
        {
            if (_inputbuffer.Length < MAX_CHARSX)
                _inputbuffer += input.inputstring;

            if (input.IsKeyPressed(Key.Up) && _scroll + LOG_DISPLAY < LOG_SIZE)
                _scroll++;
            if (input.IsKeyPressed(Key.Down) && _scroll >= 1)
                _scroll--;

            if (input.IsKeyPressed(Key.Enter) && _inputbuffer.Length > 0)
            {
                system.log.WriteLine("> " + _inputbuffer);
                cmd.Exec(_inputbuffer, true);
                _inputbuffer = "";
            }

            if (input.IsKeyPressed(Key.BackSpace) && _inputbuffer.Length > 0)
                _inputbuffer = _inputbuffer.Remove(_inputbuffer.Length - 1);
            if (input.IsKeyPressed(Key.Escape) || input.IsKeyPressed(cmd.Getbind("toggleconsole")))
                cmd.Exec("toggleconsole", false);
        }

        public void Dispose()
        {
        }

        private void Writeline(string s, uint x, uint y)
        {
            Writeline(s, x, y, Color.White);
        }

        private void Writeline(string s, uint x, uint y, Color col)
        {
            if (y > _h)
                return;

            var sy = _h - y;

            gui.Write(s, x, sy, col);
        }

        public static void Print(string s)
        {
            Print(s, Color.White);
        }

        public static void Print(string s, Color color)
        {
            var plong = 0;
            var charc = 0;
            var prev = 0;
            var words = s.Split(' ');
            for (var w = 0; w < words.Length; w++)
            {
                var pc = charc;
                charc += words[w].Length + 1;
                if (charc < SPLIT_CHARSX && w != words.Length - 1)
                    continue;

                if (pc < SPLIT_CHARSX && !(charc < SPLIT_CHARSX))
                {
                    if (plong == w)
                    {
                        var subs = words[w].SplitEvery(SPLIT_CHARSX);
                        foreach (var ss in subs)
                        {
                            Log.Enqueue(new linedata {line = ss, col = color});
                            charc = 0;
                        }

                        continue;
                    }

                    plong = w;
                    charc = pc;
                    w--;
                }

                var l = "";
                for (var i = prev; i <= w; i++) l += words[i] + " ";
                Log.Enqueue(new linedata {line = l, col = color});
                prev = w + 1;
                charc = 0;
            }
        }
    }
}