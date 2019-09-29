#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.states.options;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    public class controlsKeys : IState
    {
        private static int _cursor;
        private Dictionary<optionListing, string> _listings;
        private bool _selected;

        void IState.Init()
        {
            _listings = new Dictionary<optionListing, string>();
            Populate();
        }

        void IState.Focus()
        {
        }

        void IState.Render()
        {
            for (uint i = 0; i < screen.width * screen.height; i++)
            {
                var x = i % screen.width;
                var y = i / screen.width;
                screen.SetPixel(x, y, gui.darker);
            }

            if (!_selected)
            {
                gui.Write(lang.Get("$options.editkeys"), 19, 5, Color.Black);
                gui.Write(lang.Get("$options.editkeys"), 19, 4, Color.White);
            }
            else
            {
                gui.Write(lang.Get("$general.anykey"), 19, 5, Color.Black);
                gui.Write(lang.Get("$general.anykey"), 19, 4, Color.White);
            }

            uint n = 0;
            var ls = (_cursor + 1 - 3).Clamp(0, _listings.Count);
            var le = (_cursor + 1 + (8 - _cursor)).Clamp(0, _listings.Count);

            for (var i = (uint) ls; i < le; i++)
            {
                _listings.Keys.ElementAt((int) i).Draw(_cursor == i, 20, 14 + n * 8);
                n++;
            }
        }

        void IState.Update()
        {
            if (_selected)
                Keyinput(_listings.Values.ElementAt(_cursor));
            else
                cmd.Checkbinds();

            _listings.Keys.ElementAt(_cursor).Tick();

            if (input.IsKeyPressed(Key.Down))
            {
                CursorMove(1);
                if (_cursor != 0)
                    while (!_listings.Keys.ElementAt(_cursor).selectable)
                        CursorMove(1);
            }

            if (input.IsKeyPressed(Key.Up))
            {
                CursorMove(-1);
                if (_cursor != 0)
                    while (!_listings.Keys.ElementAt(_cursor).selectable)
                        CursorMove(-1);
            }

            if (input.IsKeyPressed(Key.Escape))
                if (statemanager.history.Peek() != null)
                    statemanager.GoBack();
        }

        public void Dispose()
        {
            _listings.Clear();
        }

        private void Populate()
        {
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

        private void Addbind(string label, string bind)
        {
            _listings.Add(new optionButton(label, delegate { _selected = true; }, cmd.binds[bind].ToString()), bind);
        }

        private void Keyinput(string bind)
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
                if (input.IsKey(key) && key != Key.Enter)
                {
                    cmd.binds[bind] = key;
                    if (bind[0] == '+') cmd.binds["-" + bind.Substring(1)] = key;

                    Populate();
                    _selected = false;

                    cmd.SaveConfig();
                }
        }

        public void CursorMove(int dir)
        {
            var pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _listings.Count - 1);
            if (pc != _cursor)
                audio.PlaySound("sound/ui/hover");
        }
    }
}