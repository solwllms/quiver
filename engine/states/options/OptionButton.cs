using System;
using engine.display;
using engine.system;
using SFML.Window;

namespace engine.states.options
{
    public class OptionButton : OptionListing
    {
        private readonly Action _action;

        public bool save = true;
        public Action tickact;
        private readonly string _value;

        public OptionButton(string label, Action action, string value = "") : base(label)
        {
            this._action = action;
            this._value = value;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            base.Draw(hover, x, y);
            Gui.Write(_value, x + 78, y);
        }

        public override void Tick()
        {
            base.Tick();

            if (Input.IsKeyPressed(Keyboard.Key.Return))
            {
                _action.Invoke();
                if (save) Cmd.SaveConfig();
            }
            else
            {
                tickact?.Invoke();
            }
        }
    }
}