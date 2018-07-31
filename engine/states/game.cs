using engine.display;
using engine.game;
using engine.system;
using SFML.Window;

namespace engine.states
{
    public class Game : IState
    {
        public static Game current;
        private Transition _fade;

        public Game(bool respawn)
        {
            Cache.ClearSounds();
            if (!respawn)
            {
                _fade = new Wipe();
            }
            else
            {
                Audio.PlaySound2D("sound/player/spawn");
                _fade = new Fizzle();
            }
        }

        public void Dispose()
        {
            current = null;
        }

        void IState.Init()
        {
            current = this;
        }

        void IState.Render()
        {
            Renderer.Render();

            if (_fade != null)
            {
                _fade.Draw();

                if (_fade.IsDone())
                    _fade = null;
                else
                    World.clock.Restart();
            }
        }

        void IState.Update()
        {
            if (_fade != null)
                if (!_fade.IsFairlyDone())
                    return;

            World.Tick();
            Cmd.Checkbinds();

            if (Input.IsKeyPressed(Keyboard.Key.Escape))
                Statemanager.SetState(progs.Progs.dll.GetMenuState());
        }
    }
}