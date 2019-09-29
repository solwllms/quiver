#region

using System;
using System.Drawing;
using Quiver.Audio;
using Quiver.display;
using Quiver.game;
using Quiver.states;
using Quiver.system;
using Quiver;
using OpenTK.Input;

#endregion

namespace game.states
{
    internal class menu : IState
    {
        private static uint _cursor;

        private readonly string[] _prompts =
        {
            "$quit.mess1",
            "$quit.mess2",
            "$quit.mess3"
        };

        private uint[,] _background;
        private int _f;
        private transition _fade;
        private bool _isgame;

        private bool _repaint;

        void IState.Focus()
        {
            if (!_isgame && audio.GetTrack() != "music/opium")
            {
                audio.StopTrack();
                audio.PlayTrack("music/opium", 60, true);
            }
        }

        public void Init()
        {
            _isgame = statemanager.history.Count > 0 && statemanager.Wasgame;
            _repaint = statemanager.history.Count > 0 && (_isgame || statemanager.history.Peek().GetType() == typeof(gameover));

            if (!_isgame)
                discordrpc.Update("in menus", "");
            if (!_repaint)
                _fade = new wipe();

            _background = new uint[screen.width, screen.height];
            if (_repaint)
                for (uint i = 0; i < screen.width * screen.height; i++)
                {
                    var x = i % screen.width;
                    var y = i / screen.width;
                    _background[x, y] = (screen.GetPixel(x, y).ToUint() >> 1) & 8355711;
                }

            _cursor = 0;
            rgbDevice.SetAll(0, 255, 0);
        }

        void IState.Render()
        {
            RenderBase();

            _f = (_f + 1) % 360;
            var ly = 13 - (uint) (Math.Cos((float) _f / 20) * 3);
            cache.GetTexture("gui/logo2").Draw(42, ly, 0, 0, 14, 25); // Q
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 45) * 3);
            cache.GetTexture("gui/logo2").Draw(56, ly, 14, 0, 12, 25); // U
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 90) * 3);
            cache.GetTexture("gui/logo2").Draw(68, ly, 26, 0, 6, 25); // I
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 135) * 3);
            cache.GetTexture("gui/logo2").Draw(74, ly, 32, 0, 12, 25); // V
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 180) * 3);
            cache.GetTexture("gui/logo2").Draw(86, ly, 44, 0, 10, 25); // E
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 225) * 3);
            cache.GetTexture("gui/logo2").Draw(96, ly, 54, 0, 14, 25); // R
            ly = 13 - (uint) (Math.Cos((float) _f / 20 + 270) * 3);

            //Gui.Write("alpha", 100, 33, Gui.lighter);

            Drawmenuitem(_cursor, 0, lang.Get("$menu.newgame"));
            Drawmenuitem(_cursor, 1, lang.Get("$menu.loadgame"));
            Drawmenuitem(_cursor, 2, lang.Get("$menu.savegame"), true);
            Drawmenuitem(_cursor, 3, lang.Get("$menu.options"));
            Drawmenuitem(_cursor, 4, lang.Get("$menu.credits"));
            Drawmenuitem(_cursor, 5, lang.Get("$menu.quit"));

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
            }
        }

        void IState.Update()
        {
            cmd.Checkbinds();

            if (input.IsKeyPressed(Key.Down))
            {
                var pc = _cursor;
                _cursor = (_cursor + 1).Clamp((uint) 0, (uint) 5);
                if (pc != _cursor)
                    audio.PlaySound("sound/ui/hover");
            }

            if (input.IsKeyPressed(Key.Up))
            {
                var pc = _cursor;
                _cursor = (_cursor - 1) % 5;
                if (pc != _cursor)
                    audio.PlaySound("sound/ui/hover");
            }

            if (input.IsKeyPressed(Key.Enter))
            {
                if (_cursor == 0)
                {
                    if (_isgame)
                        statemanager.SetState(new prompt(lang.Get("$menu.newgame"), lang.Get("$menu.unsavedprog"),
                            delegate() { level.Load("maps/E1M1.lvl", true); }));
                    else
                        level.Load("maps/E1M1.lvl", true);
                }
                else if (_cursor == 1)
                {
                    statemanager.SetState(new loadgame());
                }
                else if (_cursor == 2 && _isgame)
                {
                    statemanager.SetState(new savegame());
                    //saveload.SaveGame("test");
                }
                else if (_cursor == 3)
                {
                    statemanager.SetState(new options());
                }
                else if (_cursor == 4)
                {
                    statemanager.SetState(new credits());
                }
                else if (_cursor == 5)
                {
                    statemanager.SetState(new prompt(lang.Get("$menu.quit") + "?",
                        lang.Get(_prompts[Quiver.engine.random.Next(1, _prompts.Length) - 1]),
                        () => {
                            Quiver.engine.Exit();
                        }));
                }
            }

            if (input.IsKeyPressed(Key.Escape) && _isgame)
            {
                input.mouselock = true;
                statemanager.GoBack();
            }
        }

        public void Dispose()
        {
            _background = null;
        }

        public void Drawmenuitem(uint cursor, uint n, string text, bool gameonly = false)
        {
            if (cursor == n)
                gui.Write("{ " + text, 49, 38 + n * 7 + 1, _isgame ? gui.back : Color.Black);
            gui.Write((cursor == n ? "{ " : "  ") + text, 49, 38 + n * 7,
                gameonly && !_isgame ? gui.back : (cursor == n ? gui.lighter : (_isgame ? gui.back : Color.Black)));
        }

        public void RenderBase()
        {
            if (_repaint)
                for (uint i = 0; i < screen.width * screen.height; i++)
                {
                    var x = i % screen.width;
                    var y = i / screen.width;
                    screen.SetPixel(x, y, _background[x, y]);
                }
            else cache.GetTexture("gui/background").Draw(0, 0); //(uint)(Math.Cos((float)_f / 20) * 2)
        }
    }
}