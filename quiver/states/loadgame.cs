#region

using System.Collections.Generic;
using System.Drawing;
using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    internal class loadgame : IState
    {
        private static int _cursor;

        private readonly uint _mx = 23;
        private readonly uint _my = 20;
        private List<saveload.savelisting> _listings;

        public loadgame()
        {
            _listings = new List<saveload.savelisting>();
        }

        void IState.Init()
        {
        }

        void IState.Focus()
        {
            saveload.RefreshListings(ref _listings, ref _cursor);
        }

        void IState.Render()
        {
            ((menu) statemanager.history.Peek()).RenderBase();

            gui.Write(lang.Get("$menu.loadgame"), _mx, 9, Color.Black);
            gui.Write(lang.Get("$menu.loadgame"), _mx, 8);
            uint n = 0;

            var ls = (_cursor - 1).Clamp(0, _listings.Count);
            var le = (_cursor + 3).Clamp(0, _listings.Count);

            if (_cursor > 1)
                cache.GetTexture("gui/arrows").Draw(147, 23, 0, 0, 5, 3);
            if (_cursor < _listings.Count - 2)
                cache.GetTexture("gui/arrows").Draw(147, 76, 0, 4, 5, 3);

            for (var i = (uint) ls; i < le; i++)
            {
                DrawListing(n, i, _listings[(int) i]);
                n++;
            }

            if (_listings.Count == 0)
            {
                uint y = 20;
                gui.Draw(_mx - 2, y - 2, 119, 22, gui.darker);
                gui.WriteCentered(lang.Get("$save.nosaves"), y + 5, Color.White);
            }
        }

        void IState.Update()
        {
            cmd.Checkbinds();

            if (input.IsKeyPressed(Key.Down))
            {
                var pc = _cursor;
                _cursor = (_cursor + 1).Clamp(0, _listings.Count - 1);
                if (pc != _cursor)
                    audio.PlaySound("sound/ui/hover");
            }

            if (input.IsKeyPressed(Key.Up))
            {
                var pc = _cursor;
                _cursor = (_cursor - 1).Clamp(0, _listings.Count - 1);
                if (pc != _cursor)
                    audio.PlaySound("sound/ui/hover");
            }

            if (input.IsKeyPressed(Key.Delete))
                if (_listings[_cursor].savefile != ".")
                    saveload.DeleteSavePrompt(_listings[_cursor].savefile);

            if (input.IsKeyPressed(Key.Enter) && _listings.Count != 0)
                saveload.LoadGame(_listings[_cursor].savefile);

            if (input.IsKeyPressed(Key.Escape))
                if (statemanager.history.Peek() != null)
                {
                    input.mouselock = true;
                    statemanager.GoBack();
                }
        }

        public void Dispose()
        {
            _listings.Clear();
        }

        public void DrawListing(uint n, uint i, saveload.savelisting listing)
        {
            var y = _my + n * 22;
            if (y >= screen.height - 22)
                return;

            if (_cursor == i) gui.Draw(_mx - 2, y - 2, 119, 22, gui.darker);

            listing.texture.Draw(_mx, y);
            gui.Write(listing.name, _mx + 34, y);
            gui.Write(listing.datetime, _mx + 34, y + 7, _cursor == i ? gui.lighter : gui.darker);
        }
    }
}