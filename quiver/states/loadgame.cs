using System.Collections.Generic;
using engine.display;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace game.states
{
    internal class Loadgame : IState
    {
        private static int _cursor;
        private readonly Color _darker = new Color(79, 29, 64);
        private int _f = 0;

        private readonly Color _lighter = new Color(158, 58, 129);
        private List<Saveload.Savelisting> _listings;

        private readonly uint _mx = 23;
        private readonly uint _my = 20;

        public Loadgame()
        {
            _listings = new List<Saveload.Savelisting>();
        }

        void IState.Init()
        {
            Saveload.RefreshListings(ref _listings, ref _cursor);
        }

        void IState.Render()
        {
            ((Menu) Statemanager.history.Peek()).RenderBase();

            Gui.Write(Lang.Get("$menu.loadgame"), _mx, 9, Color.Black);
            Gui.Write(Lang.Get("$menu.loadgame"), _mx, 8);
            uint n = 0;

            var ls = (_cursor - 1).Clamp(0, _listings.Count);
            var le = (_cursor + 3).Clamp(0, _listings.Count);

            if (_cursor > 1)
                Cache.GetTexture("gui/arrows").Draw(147, 23, 0, 0, 5, 3);
            if (_cursor < _listings.Count - 2)
                Cache.GetTexture("gui/arrows").Draw(147, 76, 0, 4, 5, 3);

            for (var i = (uint) ls; i < le; i++)
            {
                DrawListing(n, i, _listings[(int) i]);
                n++;
            }

            if (_listings.Count == 0)
            {
                uint y = 20;
                Gui.Draw(_mx - 2, y - 2, 119, 22, _darker);
                Gui.WriteCentered(Lang.Get("$save.nosaves"), y + 5, Color.White);
            }
        }

        void IState.Update()
        {
            Cmd.Checkbinds();

            if (Input.IsKeyPressed(Keyboard.Key.Down))
            {
                var pc = _cursor;
                _cursor = (_cursor + 1).Clamp(0, _listings.Count - 1);
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }

            if (Input.IsKeyPressed(Keyboard.Key.Up))
            {
                var pc = _cursor;
                _cursor = (_cursor - 1).Clamp(0, _listings.Count - 1);
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }

            if (Input.IsKeyPressed(Keyboard.Key.Delete))
                if (_listings[_cursor].savefile != ".")
                    Saveload.DeleteSavePrompt(_listings[_cursor].savefile);

            if (Input.IsKeyPressed(Keyboard.Key.Return) && _listings.Count != 0)
                Saveload.LoadGame(_listings[_cursor].savefile);

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
                if (Statemanager.history.Peek() != null)
                {
                    Input.mouselock = true;
                    Statemanager.GoBack();
                }
        }

        public void Dispose()
        {
            _listings.Clear();
        }

        public void DrawListing(uint n, uint i, Saveload.Savelisting listing)
        {
            var y = _my + n * 22;
            if (y >= Screen.height - 22)
                return;

            if (_cursor == i) Gui.Draw(_mx - 2, y - 2, 119, 22, _darker);

            listing.texture.Draw(_mx, y);
            Gui.Write(listing.name, _mx + 34, y);
            Gui.Write(listing.datetime, _mx + 34, y + 7, _cursor == i ? _lighter : _darker);
        }
    }
}