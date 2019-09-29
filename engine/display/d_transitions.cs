#region

using Quiver.system;

#endregion

namespace Quiver.display
{
    public class transition
    {
        public virtual void Draw()
        {
        }

        public virtual bool IsDone()
        {
            return false;
        }

        public virtual bool IsFairlyDone()
        {
            return IsDone();
        }
    }

    public class wipe : transition
    {
        private readonly uint[,] _prebuff;
        private readonly int[] _shift;

        public wipe()
        {
            _prebuff = new uint[screen.width, screen.height];
            for (uint x = 0; x < screen.width; x++)
            for (uint y = 0; y < screen.height; y++)
                _prebuff[x, y] = screen.GetPixel(x, y).ToUint();

            _shift = new int[screen.width];
            for (var x = 0; x < screen.width; x++) _shift[x] = -engine.random.Next(0, 50);
        }

        public override void Draw()
        {
            for (uint x = 0; x < screen.width; x++)
            {
                var ys = (uint) _shift[x].Clamp(0, (int) screen.height);
                for (uint y = 0; y < screen.height; y++)
                    if (y > ys)
                        screen.SetPixel(x, y,
                            _prebuff[x, (y - ys).Clamp((uint) 0, screen.height - 1)]);
                _shift[x]++;
            }
        }

        public override bool IsDone()
        {
            for (var x = 0; x < screen.width; x++)
                if (_shift[x] < screen.height - 1)
                    return false;
            return true;
        }
    }

    public class fizzle : transition
    {
        private readonly bool[,] _fade;
        private readonly uint[,] _prebuff;
        private uint _n;

        public fizzle()
        {
            _n = screen.width * screen.height;
            _fade = new bool[screen.width, screen.height];
            _prebuff = new uint[screen.width, screen.height];
            for (uint x = 0; x < screen.width; x++)
            for (uint y = 0; y < screen.height; y++)
                _prebuff[x, y] = screen.GetPixel(x, y).ToUint();
        }

        public override void Draw()
        {
            for (uint x = 0; x < screen.width; x++)
            for (uint y = 0; y < screen.height; y++)
            {
                if (_fade[x, y])
                    continue;

                if (x * y % 15 == engine.random.Next(0, 15))
                {
                    _fade[x, y] = true;
                    _n--;
                }
                else
                {
                    screen.SetPixel(x, y, _prebuff[x, y]);
                }
            }
        }

        public override bool IsDone()
        {
            return _n < 3;
        }

        public override bool IsFairlyDone()
        {
            return _n < 200;
        }
    }
}