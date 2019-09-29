#region

using System.Drawing;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states.options
{
    public class optionSlider : optionListing
    {
        private readonly string _cvar;
        private int _held;

        private int _pv;
        private int _value;

        public optionSlider(string label, string cvar) : base(label)
        {
            _value = (int) cmd.GetValuef(cvar);
            _cvar = cvar;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);

            var sx = x + 48;
            gui.Draw(sx, y + 3, 50, 1, Color.Black);
            gui.Draw((uint) (sx + _value / 2), y + 2, 1, 3, Color.White);

            gui.Write(_value, sx + 55, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (input.IsKey(Key.Left))
            {
                _held++;
                if (_held == 1 || _held > 40 && _held % 2 == 0)
                    SetValue((_value - 1).Clamp(0, 100));
            }

            if (input.IsKey(Key.Right))
            {
                _held++;
                if (_held == 1 || _held > 40 && _held % 2 == 0)
                    SetValue((_value + 1).Clamp(0, 100));
            }

            if (!input.IsKey(Key.Right) && !input.IsKey(Key.Left)) _held = 0;

            if (_value != _pv && engine.frame % 60 == 0)
            {
                _pv = _value;
                cmd.SaveConfig();
            }
        }

        public void SetValue(int value)
        {
            _value = value;
            cmd.SetValue(_cvar, value.ToString());
        }
    }
}