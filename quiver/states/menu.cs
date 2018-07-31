using System;
using engine.display;
using engine.game;
using engine.states;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace game.states
{
    internal class Menu : IState
    {
        private static uint _cursor;
        private uint[,] _background;
        private int _f;
        private Transition _fade;

        private bool _repaint;
        private bool _isgame;

        private readonly string[] _prompts =
        {
            "$quit.mess1",
            "$quit.mess2",
            "$quit.mess3"
        };

        public Menu()
        {

        }

        public void Init()
        {
            _isgame = Statemanager.history.Count > 0 && Statemanager.Wasgame;
            _repaint = _isgame || Statemanager.history.Peek().GetType() == typeof(Gameover);

            if (!_isgame)
                Discordrpc.Update("in menus", "");
            if (!_repaint)
                _fade = new Wipe();

            _background = new uint[Screen.width, Screen.height];
            if (_repaint)
                for (uint i = 0; i < Screen.width * Screen.height; i++)
                {
                    var x = i % Screen.width;
                    var y = i / Screen.width;
                    _background[x, y] = (Screen.GetPixel(x, y) >> 1) & 8355711;
                }

            _cursor = 0;
        }

        void IState.Render()
        {
            RenderBase();

            _f = (_f + 1) % 360;
            var ly = 13 - (uint) (Math.Cos((float) _f / 20) * 3);
            Cache.GetTexture("gui/logo2").Draw(42, ly, 0, 0, 12, 23); // Q
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 45) * 3);
            Cache.GetTexture("gui/logo2").Draw(56, ly, 14, 0, 10, 23); // U
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 90) * 3);
            Cache.GetTexture("gui/logo2").Draw(68, ly, 26, 0, 4, 23); // I
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 135) * 3);
            Cache.GetTexture("gui/logo2").Draw(74, ly, 32, 0, 10, 23); // V
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 180) * 3);
            Cache.GetTexture("gui/logo2").Draw(86, ly, 44, 0, 8, 23); // E
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 225) * 3);
            Cache.GetTexture("gui/logo2").Draw(96, ly, 54, 0, 12, 23); // R
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 270) * 3);

            Gui.Write("alpha", 100, 33, Gui.lighter);

            Drawmenuitem(_cursor, 0, Lang.Get("$menu.newgame"));
            Drawmenuitem(_cursor, 1, Lang.Get("$menu.loadgame"));
            Drawmenuitem(_cursor, 2, Lang.Get("$menu.savegame"), true);
            Drawmenuitem(_cursor, 3, Lang.Get("$menu.options"));
            Drawmenuitem(_cursor, 4, Lang.Get("$menu.credits"));
            Drawmenuitem(_cursor, 5, Lang.Get("$menu.quit"));

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
            }
        }

        void IState.Update()
        {
            Cmd.Checkbinds();

            if (Input.IsKeyPressed(Keyboard.Key.Down))
            {
                var pc = _cursor;
                _cursor = (_cursor + 1).Clamp((uint) 0, (uint) 5);
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }

            if (Input.IsKeyPressed(Keyboard.Key.Up))
            {
                var pc = _cursor;
                _cursor = (_cursor - 1) % 5;
                if (pc != _cursor)
                    Audio.PlaySound2D("sound/ui/hover");
            }

            if (Input.IsKeyPressed(Keyboard.Key.Return))
            {
                if (_cursor == 0)
                {
                    if (_isgame)
                        Statemanager.SetState(new Prompt(Lang.Get("$menu.newgame"), Lang.Get("$menu.unsavedprog"),
                            delegate() { World.LoadLevel("maps/E1M1.lvl", true); }));
                    else
                    {
                        World.LoadLevel("maps/E1M1.lvl", true);
                    }
                }
                else if (_cursor == 1)
                {
                    Statemanager.SetState(new Loadgame());
                }
                else if (_cursor == 2 && _isgame)
                {
                    Statemanager.SetState(new Savegame());
                    //saveload.SaveGame("test");
                }
                else if (_cursor == 3)
                {
                    Statemanager.SetState(new Options());
                }
                else if (_cursor == 4)
                {
                    Statemanager.SetState(new Credits());
                }
                else if (_cursor == 5)
                {
                    Statemanager.SetState(new Prompt(Lang.Get("$menu.quit")+"?",
                        Lang.Get(_prompts[engine.system.Engine.random.Next(1, _prompts.Length) - 1]), engine.system.Engine.Exit));
                }
            }

            if (Input.IsKeyPressed(Keyboard.Key.Escape) && _isgame)
            {
                Input.mouselock = true;
                Statemanager.GoBack();
            }
        }

        public void Dispose()
        {
            _background = null;
        }

        public void Drawmenuitem(uint cursor, uint n, string text, bool gameonly = false)
        {
            if (cursor == n)
                Gui.Write("> " + text, 49, 38 + n * 7 + 1, Gui.lighter);
            Gui.Write((cursor == n ? "> " : "  ") + text, 49, 38 + n * 7,
                gameonly && !_isgame ? Color.Black : Color.White);
        }

        public void RenderBase()
        {
            if (_repaint)
                for (uint i = 0; i < Screen.width * Screen.height; i++)
                {
                    var x = i % Screen.width;
                    var y = i / Screen.width;
                    Screen.SetPixel(x, y, _background[x, y]);
                }
            else Cache.GetTexture("gui/background").Draw(0, 0); //(uint)(Math.Cos((float)_f / 20) * 2)
        }
    }
}