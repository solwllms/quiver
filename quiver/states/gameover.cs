#region

using System.Drawing;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace game.states
{
    public class gameover : IState
    {

        void IState.Init()
        {
            statemanager.SetTransition(new wipe());
            audio.PlayTrack("music/dead", 200, true);
        }

        void IState.Focus()
        {
        }

        void IState.Render()
        {
            for (uint i = 0; i < screen.width * screen.height; i++)
                screen.SetPixel(i % screen.width, i / screen.width, Color.FromArgb(107, 19, 0));

            gui.Write("MISSION FAILED", 10, 38, Color.White);

            cache.GetTexture("gui/skull").Draw(100, 19);
        }

        void IState.Update()
        {
            if (!statemanager.IsTransitionDone()) return;

            if (input.IsKeyPressed(Key.Escape)) statemanager.SetState(new menu());
        }

        public void Dispose()
        {
        }
    }
}