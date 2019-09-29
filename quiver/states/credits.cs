#region

using System.Drawing;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    internal class credits : IState
    {
        private int _cy;
        private transition _fade;

        private float _t;
        private bool _stop;

        public credits()
        {
            _fade = new wipe();
            audio.StopTrack();
        }

        void IState.Init()
        {
        }

        void IState.Focus()
        {
            audio.StopTrack();
            audio.PlayTrack("music/dead", looping: true);
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
            if (!_stop) _t += 0.17f;

            if (input.IsKeyPressed(Key.Escape))
            {
                if (statemanager.history.Count > 0 && statemanager.history.Peek() != null)
                    statemanager.GoBack();
            }

            if (input.IsKeyPressed(Key.Space))
            {
                statemanager.SetState(new menu(), true);
            }
        }

        public void Dispose()
        {
            audio.StopTrack();
        }

        private void Draw()
        {
            for (uint i = 0; i < screen.width * screen.height; i++)
            {
                var x = i % screen.width;
                var y = i / screen.width;
                screen.SetPixel(x, y, Color.Black);
            }

            gui.DrawTexture(cache.GetTexture("gui/logo2"), 42, GetY(95));

            _cy = 118;
            PrintCentre("");
            PrintCentre("a game by sol williams");
            PrintCentre("");
            PrintCentre("produced as apart of my own");
            PrintCentre("a-level computer science");
            PrintCentre("course-work.");
            PrintCentre("");
            PrintCentre("");
            PrintCentre("all art, design and programming");
            PrintCentre("by sol williams");
            PrintCentre("");
            PrintCentre("");
            PrintCentre("music from");
            PrintCentre("christoph de babalon's");
            PrintCentre("\"If You're Into It I'm Out Of It\"");
            PrintCentre("(c) 1997 Digital Hardcore");
            PrintCentre("");
            PrintCentre("used without licence.");
            PrintCentre("");
            PrintCentre("");
            PrintCentre("powered by the quiver engine");
            gui.DrawTexture(cache.GetTexture("gui/engine"), 61, GetY(_cy));
            _cy += 39;

            PrintCentre("");
            PrintCentre("this game and it's technology");
            PrintCentre("is protected by UK and");
            PrintCentre("international copyright law.");
            PrintCentre("For more information, contact me.");
            PrintCentre("");
            PrintCentre("(c) 2018 sol williams.");
            PrintCentre("all rights reserved.", 20);

            PrintCentre("thank you for playing!");

            _stop = GetY(_cy) == 14;
            PrintCentre("press SPACE");
        }

        private void PrintCentre(string t, int iy = 6)
        {
            var y = GetY(_cy);
            _cy += iy;

            if (y < -7) return;
            gui.WriteCentre(t, (uint) y);
        }

        private int GetY(int y)
        {
            return (int) (y - _t);
        }
    }
}