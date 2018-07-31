using engine.system;

namespace engine.display
{
    public class Transition
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

    public class Wipe : Transition
    {
        private readonly uint[,] _prebuff;
        private readonly int[] _shift;

        public Wipe()
        {
            _prebuff = new uint[Screen.width, Screen.height];
            for (uint x = 0; x < Screen.width; x++)
            for (uint y = 0; y < Screen.height; y++)
                _prebuff[x, y] = Screen.GetPixel(x, y);

            _shift = new int[Screen.width];
            for (var x = 0; x < Screen.width; x++) _shift[x] = -system.Engine.random.Next(0, 50);
        }

        public override void Draw()
        {
            for (uint x = 0; x < Screen.width; x++)
            {
                var ys = (uint) _shift[x].Clamp(0, (int) Screen.height);
                for (uint y = 0; y < Screen.height; y++)
                    if (y > ys)
                        Screen.SetPixel(x, y,
                            _prebuff[x, (y - ys).Clamp((uint) 0, Screen.height - 1)]);
                _shift[x]++;
            }
        }

        public override bool IsDone()
        {
            for (var x = 0; x < Screen.width; x++)
                if (_shift[x] < Screen.height - 1)
                    return false;
            return true;
        }
    }

    public class Fizzle : Transition
    {
        private readonly bool[,] _fade;
        private uint _n;
        private readonly uint[,] _prebuff;

        public Fizzle()
        {
            _n = Screen.width * Screen.height;
            _fade = new bool[Screen.width, Screen.height];
            _prebuff = new uint[Screen.width, Screen.height];
            for (uint x = 0; x < Screen.width; x++)
            for (uint y = 0; y < Screen.height; y++)
                _prebuff[x, y] = Screen.GetPixel(x, y);
        }

        public override void Draw()
        {
            for (uint x = 0; x < Screen.width; x++)
            for (uint y = 0; y < Screen.height; y++)
            {
                if (_fade[x, y])
                    continue;

                if (x * y % 15 == system.Engine.random.Next(0, 15))
                {
                    _fade[x, y] = true;
                    _n--;
                }
                else
                {
                    Screen.SetPixel(x, y, _prebuff[x, y]);
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