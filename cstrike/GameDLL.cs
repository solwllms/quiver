#region

using System.Drawing;
using Quiver;
using Quiver.Audio;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;
using game.states;
using Quiver.display;

#endregion

namespace game
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public class GameDLL : dll
    {
        private gmbase _gmode;

        public GameDLL() : base("COUNTER-STRIKE", "Valve Software", "v0.1", "A port of CS 1.6 to the QUIVER engine.")
        {
            discordrpc.Init("433659106147172354", null);
        }

        public override void Init()
        {
            gui.lighter = Color.DarkOrange;

            // register all map events
            progs.RegisterMapEvent("nextlevel", delegate { Quiver.game.game.LoadLevel("maps/" + level.next + ".lvl"); });

            // register all entities

            // register all weapons

            _gmode = new gameMode();
        }

        public override gmbase GetGamemode()
        {
            return _gmode;
        }

        public override IState GetMenuState()
        {
            return new menu();
        }

        public override IState GetInitialState()
        {
            return new splash();
        }

        public override void Shutdown()
        {
            discordrpc.Shutdown();
        }
    }
}