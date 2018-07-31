using engine.display;
using engine.system;
using SFML.Window;

namespace game.states
{
    internal class Splash : IState
    {
        private Transition _fade;

        private string _screen;

        private int _screenN;
        private readonly string[] _screens = {"gui/splash_evoke", "gui/splash_by", "gui/splash_quiver"};
        private int _t;

        public void Dispose()
        {
        }

        void IState.Init()
        {
            Audio.PlayTrack("music/theme", true, 60);
            ChangeScreen();
        }

        void IState.Render()
        {
            Cache.GetTexture(_screen).Draw(0, 0);

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
            }
        }

        void IState.Update()
        {
            if (Input.IsKeyPressed(Keyboard.Key.Return) || Cmd.GetValueb("nointro")) End();

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

        private void ChangeScreen()
        {
            //audio.PlayTrack("music/theme", true, 60);
            _fade = new Wipe();
            _screen = _screens[_screenN];
        }

        private void End()
        {
            Statemanager.SetState(new Menu());
        }
    }
}