using engine.game;
using engine.system;
using game.states;

namespace game
{
    internal class Gamemode : Gmbase
    {
        public override void Start()
        {
            base.Start();
        }

        public override void Tick()
        {
            base.Tick();
            if (World.Player?.health == 0) Statemanager.SetState(new Gameover());
        }

        public override void End()
        {
            base.End();
        }
    }
}