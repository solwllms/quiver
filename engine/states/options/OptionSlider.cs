using engine.display;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace engine.states.options
{
    public class OptionSlider : OptionListing
    {
        private readonly string _cvar;
        private int _held;

        private int _pv;
        private int _value;

        public OptionSlider(string label, string cvar) : base(label)
        {
            _value = (int) Cmd.GetValuef(cvar);
            this._cvar = cvar;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);

            var sx = x + 48;
            Gui.Draw(sx, y + 3, 50, 1, Color.Black);
            Gui.Draw((uint) (sx + _value / 2), y + 2, 1, 3, Color.White);

            Gui.Write(_value, sx + 55, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (Input.IsKey(Keyboard.Key.Left))
            {
                _held++;
                if (_held == 1 || _held > 40 && _held % 2 == 0)
                    SetValue((_value - 1).Clamp(0, 100));
            }

            if (Input.IsKey(Keyboard.Key.Right))
            {
                _held++;
                if (_held == 1 || _held > 40 && _held % 2 == 0)
                    SetValue((_value + 1).Clamp(0, 100));
            }

            if (!Input.IsKey(Keyboard.Key.Right) && !Input.IsKey(Keyboard.Key.Left)) _held = 0;

            if (_value != _pv && system.Engine.frame % 60 == 0)
            {
                _pv = _value;
                Cmd.SaveConfig();
            }
        }

        public void SetValue(int value)
        {
            this._value = value;
            Cmd.SetValue(_cvar, value.ToString());
        }
    }
}