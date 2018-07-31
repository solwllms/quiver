using engine.display;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace game.states
{
    internal class Credits : IState
    {
        private Transition _fade;

        private float _t;
        private bool stop = false;

        public Credits()
        {
            _fade = new Wipe();
            Audio.StopTrack();
        }

        void IState.Init()
        {
            Audio.PlayTrack("music/theme", true);
        }

        void IState.Render()
        {
            Draw();

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
            }
        }

        void IState.Update()
        {
            if(!stop) _t += 0.17f;

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
                if (Statemanager.history.Peek() != null)
                    Statemanager.GoBack();
        }

        public void Dispose()
        {
            Audio.StopTrack();
        }

        private void Draw()
        {
            for (uint i = 0; i < Screen.width * Screen.height; i++)
            {
                var x = i % Screen.width;
                var y = i / Screen.width;
                Screen.SetPixel(x, y, Color.Black);
            }

            Gui.DrawTexture(Cache.GetTexture("gui/logo2"), 42, (int)GetY(95));

            _cy = 118;
            PrintCentre("a game by sol williams", 20);

            PrintCentre("music by");
            PrintCentre("www.hauntedillinois.com");
            PrintCentre("(aliens.mid \"Aliens Theme\")", 20);
            
            PrintCentre("powered by the quiver engine", 7);
            Gui.DrawTexture(Cache.GetTexture("gui/engine"), 61, (int)GetY(_cy));
            _cy += 39;

            PrintCentre("thank you for playing!");
            PrintCentre("(c) sol williams 2018", 20);

            stop = GetY(_cy) == Screen.height - 14;
            PrintCentre("press ESC");
        }

        private int _cy;
        private void PrintCentre(string t, int iy = 6)
        {
            Gui.WriteCentre(t, GetY(_cy));
            _cy += iy;
        }

        private uint GetY(int y)
        {
            return (uint)(y - _t);
        }
    }
}