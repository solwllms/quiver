using System.Collections.Generic;
using engine.display;
using engine.states;
using engine.states.options;
using engine.system;
using SFML.Graphics;
using SFML.Window;
using Texture = engine.display.Texture;

namespace game.states
{
    internal class Options : IState
    {
        public const uint MX = 20;
        public const uint MY = 14;
        public const uint MW = 119;

        private static int _tab;
        private static int _cursor;
        private readonly List<OptionListing> _listings;

        private Menu _m;

        private Texture _t;

        private readonly string[] _titles = { "$options.game", "$options.display", "$options.audio", "$options.controls" };

        public Options()
        {
            _listings = new List<OptionListing>();
        }

        void IState.Init()
        {
            _t = Cache.GetTexture("gui/settingstabs");
            CursorMove(-1);

            _m = (Menu) Statemanager.history.Peek();

            _tab = 0;
            UpdateMenu();
        }

        void IState.Render()
        {
            RenderBase();

            _t.DrawStencil(19, 3, 0, 0, 8, 8, _tab == 0 ? Color.White : Gui.darker);
            _t.DrawStencil(31, 3, 8, 0, 8, 8, _tab == 1 ? Color.White : Gui.darker);
            _t.DrawStencil(43, 3, 16, 0, 8, 8, _tab == 2 ? Color.White : Gui.darker);
            _t.DrawStencil(55, 3, 24, 0, 8, 8, _tab == 3 ? Color.White : Gui.darker);

            Gui.Write(Lang.Get(_titles[_tab]), 70, 5, Color.Black);
            Gui.Write(Lang.Get(_titles[_tab]), 70, 4, Color.White);

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
            Cmd.Checkbinds();

            if (_cursor == 0)
            {
                if (Input.IsKeyPressed(Keyboard.Key.Right))
                    TabMove(1);
                else if (Input.IsKeyPressed(Keyboard.Key.Left)) TabMove(-1);
            }
            else
            {
                _listings[_cursor - 1].Tick();
            }

            if (Input.IsKeyPressed(Keyboard.Key.Down))
            {
                CursorMove(1);
                while (_cursor < _listings.Count - 1 && !_listings[_cursor - 1].selectable) CursorMove(1);
            }

            if (Input.IsKeyPressed(Keyboard.Key.Up))
            {
                CursorMove(-1);
                while (_cursor != 0 && !_listings[_cursor - 1].selectable) CursorMove(-1);
            }

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
                if (Statemanager.history.Peek() != null)
                    Statemanager.GoBack();
        }

        public void Dispose()
        {
            _listings.Clear();
        }

        private void PopulateGame()
        {
            _listings.Clear();
            _listings.Add(new OptionMulti(Lang.Get("$options.difficulty"), new[] { Lang.Get("$options.easy"), Lang.Get("$options.normal"), Lang.Get("$options.hard"), Lang.Get("$options.hell") }, "difficulty"));
            _listings.Add(new OptionMulti(Lang.Get("$options.language"), Lang.langs, "language", delegate { PopulateGame(); }));
            _listings.Add(new OptionGap());
            _listings.Add(new OptionButton(Lang.Get("$menu.credits"), delegate { Statemanager.SetState(new Credits()); }));
            _listings.Add(new OptionGap());
            _listings.Add(new OptionButton(Cmd.GetValueb("showfps") ? Lang.Get("$options.hidefps") : Lang.Get("$options.showfps"), delegate
            {
                Cmd.Toggle("showfps");
                UpdateMenu();
            }));
            _listings.Add(new OptionButton(Lang.Get("$options.erasesaves"), delegate
            {
                bool y = false;
                Statemanager.SetState(new Prompt(Lang.Get("$options.erasesaves")+"?", Lang.Get("$general.cantundo"),
                    delegate ()
                    {
                        Saveload.DeleteAll();
                        y = true;
                    }));
                if (y) Statemanager.SetState(new Prompt(Lang.Get("general.success"), Lang.Get("$options.allsavesdel"), delegate () { }));
            }));
        }

        private void PopulateDisplay()
        {
            _listings.Clear();
            _listings.Add(new OptionHeader(Lang.Get("$options.device")));

            try // this CAN fail if a unsupported card or onsome OSes
            {
                _listings.Add(new OptionText(Hardware.GetInfo("Name").Truncate(29)) {col = Gui.darker});
            }
            catch { }

            _listings.Add(new OptionGap());
            _listings.Add(new OptionButton(Cmd.GetValueb("fullscreen") ? Lang.Get("$options.windowed") : Lang.Get("$options.fullscreen"), delegate
            {
                Cmd.Toggle("fullscreen");
                UpdateMenu();
            }));
        }

        private void PopulateAudio()
        {
            _listings.Clear();
            _listings.Add(new OptionButton(Cmd.GetValueb("audio") ? Lang.Get("$options.disablesound") : Lang.Get("$options.enablesound"), delegate
            {
                Cmd.Toggle("audio");
                UpdateMenu();
            }));
            _listings.Add(new OptionGap());
            _listings.Add(new OptionHeader(Lang.Get("$options.volume")));
            _listings.Add(new OptionSlider(Lang.Get("$options.mastervol"), "volume"));
            _listings.Add(new OptionSlider(Lang.Get("$options.musicvol"), "musicvol"));
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
            _listings.Add(new OptionButton(Lang.Get("$options.editkeys"), delegate { Statemanager.SetState(new ControlsKeys()); }));
        }

        public void CursorMove(int dir)
        {
            var pc = _cursor;
            _cursor = (_cursor + dir).Clamp(0, _listings.Count);
            if (pc != _cursor)
                Audio.PlaySound2D("sound/ui/hover");
        }

        public void TabMove(int dir)
        {
            var pc = _tab;
            _tab = (_tab + dir).Clamp(0, 3); // tab count = 4!
            if (pc != _tab)
            {
                Audio.PlaySound2D("sound/ui/hover");

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