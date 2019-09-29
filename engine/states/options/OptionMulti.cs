#region

using System;
using System.Linq;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states.options
{
    public class optionMulti : optionListing
    {
        private readonly string _cvar;
        private readonly string[] _options;

        private readonly Action _onChange;
        private int _cursor;

        public optionMulti(string label, string[] options, string cvar) : this(label, options, cvar, null)
        {
        }

        public optionMulti(string label, string[] options, string cvar, Action onChange) : base(label)
        {
            _options = options;
            _cvar = cvar;
            this._onChange = onChange;

            if (options.Contains(cmd.GetValue(cvar)))
                _cursor = Array.IndexOf(options, cmd.GetValue(cvar));
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);

            gui.Write(cmd.GetValue(_cvar), x + 78, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (input.IsKeyPressed(Key.Left)) CursorMove(-1);
            if (input.IsKeyPressed(Key.Right)) CursorMove(1);
        }

        public void CursorMove(int dir)
        {
            var pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _options.Length - 1);
            if (pc != _cursor)
            {
                audio.PlaySound("sound/ui/hover");
                cmd.SetValue(_cvar, _options[_cursor]);
                _onChange?.Invoke();
            }
        }
    }
}