#region

using Quiver.Audio;
using Quiver.display;
using Quiver.game;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states
{
    public class game : IState
    {
        public static game current;

        private static string _chapterMsgDraw = "";
        private static string _chapterMsg = "";
        private static int _chapterI;
        private static int _chapterA;
        private transition _fade;

        public game(bool respawn)
        {
            //cache.ClearSounds();
            if (!respawn)
            {
                _fade = new wipe();
            }
            else
            {
                audio.PlaySound("sound/player/spawn");
                _fade = new fizzle();
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
            if ((_fade == null || _fade.IsFairlyDone()) && _chapterMsg != "")
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

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
                else
                    world.clock.Restart();
            }
        }

        void IState.Update()
        {
            if (_fade != null)
                if (!_fade.IsFairlyDone())
                    return;

            world.Tick();
            cmd.Checkbinds();

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