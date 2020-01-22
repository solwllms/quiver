#region

using Quiver.Audio;
using Quiver.display;
using Quiver.game;
using Quiver.system;
using OpenTK.Input;
using Quiver.Network;

#endregion

namespace Quiver.states
{
    public class game_state : IState
    {
        public static game_state current;

        private static string _chapterMsgDraw = "";
        private static string _chapterMsg = "";
        private static int _chapterI;
        private static int _chapterA;

        public game_state(bool respawn)
        {
            //cache.ClearSounds();
            if (!respawn)
            {
                statemanager.SetTransition(new wipe());
            }
            else
            {
                audio.PlaySound("sound/player/spawn");
                statemanager.SetTransition(new fizzle());
            }
        }

        public void Dispose()
        {
            //current = null;
        }

        void IState.Focus()
        {
        }

        void IState.Init()
        {
            current = this;
        }

        void IState.Render()
        {
            renderer.Render();
            progs.dll.GetGamemode().DrawHud();

            // chapter message
            if (statemanager.IsTransitionFairlyDone() && _chapterMsg != "")
            {
                gui.Prompt(_chapterMsgDraw, 20 / _chapterA);

                if (engine.frame % 5 == 0)
                {
                    if (_chapterI > _chapterMsg.Length)
                    {
                        if (_chapterI - _chapterMsg.Length == 20)
                        {
                            _chapterI = 0;
                            _chapterMsg = "";
                            _chapterMsgDraw = "";
                        }
                        else
                        {
                            _chapterA--;
                            _chapterI++;
                        }
                    }
                    else
                    {
                        _chapterMsgDraw = _chapterMsg.Substring(0, _chapterI);
                        _chapterI++;
                    }
                }
            }

            if (!statemanager.IsTransitionDone()) world.clock.Restart();
        }

        void IState.Update()
        {
            if (!statemanager.IsTransitionFairlyDone()) return;

            game.game.Tick();

            if (input.IsKeyPressed(Key.Escape))
                statemanager.SetState(progs.dll.GetMenuState());
        }

        public static void SetChapterMsg(string s)
        {
            _chapterMsg = s;
            _chapterA = 20;
        }
    }
}