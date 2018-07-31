using engine.display;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace game.states
{
    public class Gameover : IState
    {
        private Transition _fade;

        void IState.Init()
        {
            _fade = new Wipe();
            Audio.PlayTrack("music/dead", true, 200);
        }

        void IState.Render()
        {
            for (uint i = 0; i < Screen.width * Screen.height; i++)
                Screen.SetPixel(i % Screen.width, i / Screen.width, new Color(107, 19, 0));

            Gui.Write("MISSION FAILED", 10, 38, Color.White);
            Gui.Write("THE DEAD HAVE NO VOICE", 10, 46, Color.White);

            Cache.GetTexture("gui/skull").Draw(100, 19);

            _fade.Draw();
        }

        void IState.Update()
        {
            if (!_fade.IsDone())
                return;

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
            {
                Statemanager.SetState(new Menu());
            }
        }

        public void Dispose()
        {

        }
    }
}
