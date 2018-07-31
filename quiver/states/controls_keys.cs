using System;
using System.Collections.Generic;
using System.Linq;
using engine.display;
using engine.states.options;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace game.states
{
    public class ControlsKeys : IState
    {
        Dictionary<OptionListing, string> _listings;
        
        static int _cursor = 0;
        bool _selected = false;

        void IState.Init()
        {
            _listings = new Dictionary<OptionListing, string>();
            Populate();
        }

        void Populate() {
            _listings.Clear();
            Addbind("Forward", "+forward");
            Addbind("Back", "+back");
            Addbind("Left", "+left");
            Addbind("Right", "+right");
            Addbind("Fire", "fire");
            Addbind("Use", "use");
            Addbind("Reload", "reload");
            Addbind("Console", "toggleconsole");
        }

        void Addbind(string label, string bind)
        {
            _listings.Add(new OptionButton(label, delegate () { _selected = true; }, Cmd.binds[bind].ToString()), bind);
        }

        void Keyinput(string bind)
        {
            foreach (Keyboard.Key key in Enum.GetValues(typeof(Keyboard.Key)))
            {
                if (Input.IsKey(key) && key != Keyboard.Key.Return)
                {
                    Cmd.binds[bind] = key;
                    if(bind[0] == '+') Cmd.binds["-"+bind.Substring(1)] = key;

                    Populate();
                    _selected = false;

                    Cmd.SaveConfig();
                }
            }
        }

        void IState.Render()
        {
            for (uint i = 0; i < Screen.width * Screen.height; i++)
            {
                uint x = i % Screen.width;
                uint y = i / Screen.width;
                Screen.SetPixel(x, y, Gui.darker);
            }
            if (!_selected)
            {
                Gui.Write(Lang.Get("$options.editkeys"), 19, 5, Color.Black);
                Gui.Write(Lang.Get("$options.editkeys"), 19, 4, Color.White);
            }
            else
            {
                Gui.Write(Lang.Get("$general.anykey"), 19, 5, Color.Black);
                Gui.Write(Lang.Get("$general.anykey"), 19, 4, Color.White);
            }

            uint n = 0;
            int ls = ((_cursor + 1) - 3).Clamp(0, _listings.Count);
            int le = ((_cursor + 1) + (8 - _cursor)).Clamp(0, _listings.Count);

            for (uint i = (uint)ls; i < le; i++)
            {
                _listings.Keys.ElementAt((int)i).Draw(_cursor == i , 20, 14 + (n * 8));
                n++;
            }
        }

        void IState.Update()
        {
            if (_selected)
            {
                Keyinput(_listings.Values.ElementAt(_cursor));
            }
            else
                Cmd.Checkbinds();

            _listings.Keys.ElementAt(_cursor).Tick();

            if (Input.IsKeyPressed(Keyboard.Key.Down))
            {
                CursorMove(1);
                if (_cursor != 0) while (!_listings.Keys.ElementAt(_cursor).selectable) { CursorMove(1); }
            }
            if (Input.IsKeyPressed(Keyboard.Key.Up))
            {
                CursorMove(-1);
                if (_cursor != 0) while (!_listings.Keys.ElementAt(_cursor).selectable) { CursorMove(-1); }
            }

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
            {
                if (Statemanager.history.Peek() != null)
                {
                    Statemanager.GoBack();
                }
            }
        }

        public void CursorMove(int dir)
        {
            int pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _listings.Count - 1);
            if (pc != _cursor)
                Audio.PlaySound2D("sound/ui/hover");
        }                

        public void Dispose()
        {
            _listings.Clear();
        }
    }
}
