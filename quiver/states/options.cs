#region

using System.Collections.Generic;
using System.Drawing;
using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.states;
using Quiver.states.options;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    internal class options : IState
    {
        public const uint MX = 20;
        public const uint MY = 14;
        public const uint MW = 119;

        private static int _tab;
        private static int _cursor;
        private readonly List<optionListing> _listings;

        private readonly string[] _titles =
            {"$options.game", "$options.display", "$options.audio", "$options.controls"};

        private menu _m;

        private texture _t;

        public options()
        {
            _listings = new List<optionListing>();
        }

        void IState.Init()
        {
            _t = cache.GetTexture("gui/settingstabs");
            CursorMove(-1);

            _m = (menu) statemanager.history.Peek();

            _tab = 0;
            UpdateMenu();
        }

        void IState.Focus()
        {
        }

        void IState.Render()
        {
            RenderBase();

            _t.DrawStencil(19, 3, 0, 0, 8, 8, _tab == 0 ? Color.White : gui.darker);
            _t.DrawStencil(31, 3, 8, 0, 8, 8, _tab == 1 ? Color.White : gui.darker);
            _t.DrawStencil(43, 3, 16, 0, 8, 8, _tab == 2 ? Color.White : gui.darker);
            _t.DrawStencil(55, 3, 24, 0, 8, 8, _tab == 3 ? Color.White : gui.darker);

            gui.Write(lang.Get(_titles[_tab]), 70, 5, Color.Black);
            gui.Write(lang.Get(_titles[_tab]), 70, 4, Color.White);

            uint n = 0;
            var ls = (_cursor - 3).Clamp(0, _listings.Count);
            var le = (_cursor + (8 - _cursor)).Clamp(0, _listings.Count);

            for (var i = (uint) ls; i < le; i++)
            {
                _listings[(int) i].Draw(_cursor == i + 1, MX, MY + n * 8);
                n++;
            }
        }

        void IState.Update()
        {
            cmd.Checkbinds();

            if (_cursor == 0)
            {
                if (input.IsKeyPressed(Key.Right))
                    TabMove(1);
                else if (input.IsKeyPressed(Key.Left)) TabMove(-1);
            }
            else
            {
                _listings[_cursor - 1].Tick();
            }

            if (input.IsKeyPressed(Key.Down))
            {
                CursorMove(1);
                while (_cursor < _listings.Count - 1 && !_listings[_cursor - 1].selectable) CursorMove(1);
            }

            if (input.IsKeyPressed(Key.Up))
            {
                CursorMove(-1);
                while (_cursor != 0 && !_listings[_cursor - 1].selectable) CursorMove(-1);
            }

            if (input.IsKeyPressed(Key.Escape))
                if (statemanager.history.Peek() != null)
                    statemanager.GoBack();
        }

        public void Dispose()
        {
            _listings.Clear();
        }

        private void PopulateGame()
        {
            _listings.Clear();
            _listings.Add(new optionMulti(lang.Get("$options.difficulty"),
                new[]
                {
                    lang.Get("$options.easy"), lang.Get("$options.normal"), lang.Get("$options.hard"),
                    lang.Get("$options.hell")
                }, "difficulty"));
            _listings.Add(new optionMulti(lang.Get("$options.language"), lang.langs, "language",
                delegate { PopulateGame(); }));
            _listings.Add(new optionGap());
            _listings.Add(new optionButton(lang.Get("$menu.credits"),
                delegate { statemanager.SetState(new credits()); }));
            _listings.Add(new optionGap());
            _listings.Add(new optionButton(
                cmd.GetValueb("showfps") ? lang.Get("$options.hidefps") : lang.Get("$options.showfps"), delegate
                {
                    cmd.Toggle("showfps");
                    UpdateMenu();
                }));
            _listings.Add(new optionButton(lang.Get("$options.erasesaves"), delegate
            {
                var y = false;
                statemanager.SetState(new prompt(lang.Get("$options.erasesaves") + "?", lang.Get("$general.cantundo"),
                    delegate()
                    {
                        saveload.DeleteAll();
                        y = true;
                    }));
                if (y)
                    statemanager.SetState(new prompt(lang.Get("general.success"), lang.Get("$options.allsavesdel"),
                        delegate() { }));
            }));
        }

        private void PopulateDisplay()
        {
            _listings.Clear();
            _listings.Add(new optionButton(
                screen.cvarFullscreen.Valueb() ? lang.Get("$options.windowed") : lang.Get("$options.fullscreen"), delegate
                {
                    screen.cvarFullscreen.Toggle();
                    UpdateMenu();
                }));
        }

        private void PopulateAudio()
        {
            _listings.Clear();
            _listings.Add(new optionButton(
                cmd.GetValueb("audio") ? lang.Get("$options.disablesound") : lang.Get("$options.enablesound"), delegate
                {
                    cmd.Toggle("audio");
                    UpdateMenu();
                }));
            _listings.Add(new optionGap());
            _listings.Add(new optionHeader(lang.Get("$options.volume")));
            _listings.Add(new optionSlider(lang.Get("$options.mastervol"), "volume"));
            _listings.Add(new optionSlider(lang.Get("$options.musicvol"), "musicvol"));
        }

        private void PopulateControls()
        {
            _listings.Clear();
            /*
            _listings.Add(new OptionText("no controller found"));
            _listings.Add(new OptionButton("search for controller",
                delegate { Statemanager.SetState(new Prompt("unsupported!", "an error occured", delegate() { })); }));
            _listings.Add(new OptionGap());
            */
            _listings.Add(new optionButton(lang.Get("$options.editkeys"),
                delegate { statemanager.SetState(new controlsKeys()); }));
        }

        public void CursorMove(int dir)
        {
            var pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _listings.Count);
            if (pc != _cursor)
                audio.PlaySound("sound/ui/hover");
        }

        public void TabMove(int dir)
        {
            var pc = _tab;
            _tab = (_tab + dir).Clamp(0, 3); // tab count = 4!
            if (pc != _tab)
            {
                audio.PlaySound("sound/ui/hover");

                UpdateMenu();
            }
        }

        public void UpdateMenu()
        {
            if (_tab == 0) PopulateGame();
            else if (_tab == 1) PopulateDisplay();
            else if (_tab == 2) PopulateAudio();
            else if (_tab == 3) PopulateControls();
        }

        public void RenderBase()
        {
            _m.RenderBase();
        }
    }
}