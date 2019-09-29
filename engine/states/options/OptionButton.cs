#region

using System;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states.options
{
    public class optionButton : optionListing
    {
        private readonly Action _action;
        private readonly string _value;

        public bool save = true;
        public Action tickact;

        public optionButton(string label, Action action, string value = "") : base(label)
        {
            _action = action;
            _value = value;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);
            gui.Write(_value, x + 78, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (input.IsKeyPressed(Key.Enter))
            {
                _action.Invoke();
                if (save) cmd.SaveConfig();
            }
            else
            {
                tickact?.Invoke();
            }
        }
    }
}