﻿#region

using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;

#endregion

namespace game.states
{
    internal class splash : IState
    {
        private readonly string[] _screens = {"gui/splash_evoke", "gui/splash_by", "gui/splash_quiver"};

        private string _screen;

        private int _screenN;
        private int _t;

        public void Dispose()
        {
        }

        void IState.Init()
        {
            audio.PlayTrack("music/opium", 60, true);
            rgbDevice.SetAll(0, 255, 0);
            ChangeScreen();
        }

        void IState.Render()
        {
            cache.GetTexture(_screen).Draw(0, 0);
        }

        void IState.Update()
        {
            if (input.AnyKey() || cmd.GetValueb("game_nointro")) End();

            _t++;
            if (_t == 300)
            {
                _screenN++;
                _t = 0;

                if (_screenN == _screens.Length)
                {
                    End();
                    return;
                }

                ChangeScreen();
            }
        }

        void IState.Focus()
        {
        }

        private void ChangeScreen()
        {
            //audio.PlayTrack("music/theme", true, 60);
            statemanager.SetTransition(new wipe());
            _screen = _screens[_screenN];
        }

        private void End()
        {
            statemanager.SetState(new menu());
        }
    }
}