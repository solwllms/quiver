#region

using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;

#endregion

namespace game.states
{
    internal class splash : IState
    {
        private readonly string[] _screens = {"gui/splash"};
        private transition _fade;

        private string _screen;

        private int _screenN;
        private int _t;

        public void Dispose()
        {
        }

        void IState.Init()
        {
            audio.PlayTrack("sound/theme", 60, true);
            rgbDevice.SetAll(0, 255, 0);
            ChangeScreen();
        }


        void IState.Render()
        {
            cache.GetTexture(_screen).Draw(0, 0);

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
            }
        }

        void IState.Update()
        {
            if (input.AnyKey() || cmd.GetValueb("nointro")) End();

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
            _fade = new fizzle();
            _screen = _screens[_screenN];
        }

        private void End()
        {
            statemanager.SetState(new menu());
        }
    }
}