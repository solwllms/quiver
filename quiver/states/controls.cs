using System.Collections.Generic;
using engine.display;
using engine.states.options;
using engine.system;
using SFML.Graphics;

namespace game.states
{
    class Controls : IState
    {
        private readonly List<OptionListing> _keys;

        uint _mx = 23;
        uint _my = 20;

        public Controls()
        {
            _keys = new List<OptionListing> ();
        }

        void IState.Init()
        {
            _keys.Clear();
            _keys.Add(new OptionButton("forward", delegate() { Statemanager.SetState(new Controls()); }));
        }

        private static int _cursor = 0;
        int _f = 0;
        void IState.Render()
        {
            ((Options)Statemanager.history.Peek()).RenderBase();

            Gui.Write("controls", _mx, 9, Color.Black);
            Gui.Write("controls", _mx, 8);
            uint n = 0;

            int ls = ((_cursor + 1) - 6).Clamp(0, _keys.Count);
            int le = ((_cursor + 1) + 6).Clamp(0, _keys.Count);

            if (_cursor > 1)
                Cache.GetTexture("gui/arrows").Draw(147, 23, 0, 0, 5, 3);
            if (_cursor < _keys.Count - 2)
                Cache.GetTexture("gui/arrows").Draw(147, 76, 0, 4, 5, 3);

            for (uint i = (uint)ls; i < le; i++)
            {
                _keys[(int)i].Draw(_cursor == i, _mx, _my + (i * 8));
                n++;
            }
        }

        void IState.Update()
        {
            Cmd.Checkbinds();

            _keys[_cursor].Tick();

            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Down))
            {
                int pc = _cursor;
                _cursor = (_cursor + 1).Clamp(0, _keys.Count - 1);
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Up))
            {
                int pc = _cursor;
                _cursor = (_cursor - 1).Clamp(0, _keys.Count - 1);
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }

            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Escape))
            {
                if (Statemanager.history.Peek() != null)
                {
                    Statemanager.GoBack();
                }
            }
        }

        public void Dispose()
        {
            _keys.Clear();
        }
    }
}
