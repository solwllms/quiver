using System;
using System.Linq;
using engine.display;
using engine.system;
using SFML.Window;

namespace engine.states.options
{
    public class OptionMulti : OptionListing
    {
        private int _cursor;
        private readonly string _cvar;
        private readonly string[] _options;

        private Action onChange;

        public OptionMulti(string label, string[] options, string cvar) : this(label, options, cvar, null)
        {
        }

        public OptionMulti(string label, string[] options, string cvar, Action onChange) : base(label)
        {
            this._options = options;
            this._cvar = cvar;
            this.onChange = onChange;

            if (options.Contains(Cmd.GetValue(cvar)))
                _cursor = Array.IndexOf(options, Cmd.GetValue(cvar));
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);

            Gui.Write(Cmd.GetValue(_cvar), x + 78, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (Input.IsKeyPressed(Keyboard.Key.Left)) CursorMove(-1);
            if (Input.IsKeyPressed(Keyboard.Key.Right)) CursorMove(1);
        }

        public void CursorMove(int dir)
        {
            var pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _options.Length - 1);
            if (pc != _cursor)
            {
                Audio.PlaySound2D("sound/ui/hover");
                Cmd.SetValue(_cvar, _options[_cursor]);
                onChange?.Invoke();
            }
        }
    }
}