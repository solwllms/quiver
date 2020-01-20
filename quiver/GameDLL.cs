#region

using Quiver;
using Quiver.Audio;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;
using game.entities;
using game.states;
using game.weapons;

#endregion

namespace game
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public class GameDLL : dll
    {
        private gmbase _gmode;

        public GameDLL() : base("Quiver", "Sol Williams", "v0.1", "vanilla quiver")
        {
            discordrpc.Init("433659106147172354", null);
        }

        public override void Init()
        {
            // register all map events
            progs.RegisterMapEvent("nextlevel", delegate (mapcell cell) {
                if (world.GetTextureAlias(cell.walltex) != "textures/exit") return;
                level.Load("maps/" + level.next + ".lvl", false);
            });
            progs.RegisterMapEvent("prevlevel", delegate
            {
                level.Load("maps/" + level.prev + ".lvl", false);
                var c = level.data.CoordinatesOf(-2);
                world.Player.pos = new vector(c.Item1, c.Item2);
            });
            progs.RegisterMapEvent("glassshot", delegate (mapcell cell)
            {
                audio.PlaySound3D("sound/glass/glass" + Quiver.engine.random.Next(1, 4), cell.pos, 70);
            });
            progs.RegisterMapEvent("useconsole1", delegate { statemanager.SetState(new consoleGame()); });

            progs.RegisterMapEvent("ws_use", delegate (mapcell cell)
            {
                mapcell door = world.FindTexturedCell("textures/exit_lock");
                door.SetWalltex("textures/exit");
                door.interactable = true;

                cell.SetWalltex("textures/brick1_sw1");
            });
            progs.RegisterMapEvent("escape", delegate (mapcell cell)
            {
                statemanager.SetState(new credits(), true);
            });

            // register all entities
            progs.RegisterEnt(typeof(m_Eye));
            progs.RegisterEnt(typeof(p_Guts));
            progs.RegisterEnt(typeof(m_Robo));
            progs.RegisterEnt(typeof(p_Lazer));

            // register all weapons
            progs.RegisterWeapon(typeof(wRevolver));

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