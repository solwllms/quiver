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

        private uint[,] _background;
        private bool _isgame;

        private bool _repaint;

        void IState.Focus()
        {
            Music();
        }

        void Music()
        {
            if (!_isgame && audio.GetTrack() != "sound/theme")
            {
                audio.StopTrack();
                audio.PlayTrack("sound/theme", 60, true);
            }
        }

        public void Init()
        {
            _isgame = statemanager.history.Count > 0 && statemanager.Wasgame;
            _repaint = statemanager.history.Count > 0 && _isgame;

            if (!_isgame)
                discordrpc.Update("in menus", "");
            if (!_repaint) statemanager.SetTransition(new wipe());

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

            Music();
        }

        void IState.Render()
        {
            RenderBase();
            
            cache.GetTexture("gui/logo").Draw(7, 30);

            Drawmenuitem(_cursor, 0, lang.Get("PLAY DE_DUST"));
            Drawmenuitem(_cursor, 1, lang.Get("QUIT"));
        }

        void IState.Update()
        {
            cmd.Checkbinds();

            if (input.IsKeyPressed(Key.Down))
            {
                var pc = _cursor;
                _cursor = (_cursor + 1).Clamp((uint) 0, (uint) 1);
                if (pc != _cursor)
                    audio.PlaySound("sound/buttonrollover");
            }

            if (input.IsKeyPressed(Key.Up))
            {
                var pc = _cursor;
                _cursor = (_cursor - 1) % 1;
                if (pc != _cursor)
                    audio.PlaySound("sound/buttonrollover");
            }

            if (input.IsKeyPressed(Key.Enter))
            {
                audio.PlaySound("sound/buttonclick");

                if (_cursor == 0)
                {
                    if (_isgame)
                        statemanager.SetState(new prompt(lang.Get("$menu.newgame"), lang.Get("$menu.unsavedprog"),
                            delegate() { level.Load("maps/dust.lvl", true); }));
                    else
                        level.Load("maps/dust.lvl", true);
                }
                else if (_cursor == 1)
                {
                    statemanager.SetState(new prompt("quit?",
                        "are you sure?",
                        () => {
                            engine.Exit();
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

        public void Drawmenuitem(uint cursor, uint n, string text, bool inactive = false)
        {
            gui.Write(text, 7, 40 + n * 7 + 1, inactive ? Color.Black : (cursor == n ? Color.White : Color.DarkGray));
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
            else cache.GetTexture("gui/background2").Draw(0, 0); //(uint)(Math.Cos((float)_f / 20) * 2)
        }
    }
}