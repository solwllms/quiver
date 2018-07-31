using engine.game;
using engine.game.types;
using engine.progs;
using engine.system;
using game.entities;
using game.states;
using game.weapons;

namespace game
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public class GameDLL : Dll
    {
        public GameDLL() : base("QUIVER", "Sol Williams", "v0.1", "vanilla quiver")
        {
            Discordrpc.Init("433659106147172354", null);
        }

        public override void Init()
        {
            // register all map events
            Progs.RegisterMapEvent("nextlevel", delegate
            {
                World.LoadLevel("maps/" + Level.next + ".lvl", false);
            });
            Progs.RegisterMapEvent("prevlevel", delegate
            {
                World.LoadLevel("maps/" + Level.prev + ".lvl", false);
                var c = Level.data.CoordinatesOf(-2);
                World.Player.pos = new Vector(c.Item1, c.Item2);
            });
            Progs.RegisterMapEvent("glassshot", delegate(Mapcell cell)
            {
                Audio.PlaySound3D("sound/glass/glass" + engine.system.Engine.random.Next(1, 4), cell.pos, 70);
            });

            // register all entities
            Progs.RegisterEnt(typeof(m_Eye));
            Progs.RegisterEnt(typeof(p_Guts));
            Progs.RegisterEnt(typeof(m_Robo));
            Progs.RegisterEnt(typeof(p_Lazer));

            // register all weapons
            Progs.RegisterWeapon(typeof(WRevolver));
        }

        public override Gmbase GetGamemode()
        {
            return new Gamemode();
        }

        public override IState GetMenuState()
        {
            return new Menu();
        }

        public override IState GetInitialState()
        {
            return new Splash();
        }

        public override void Shutdown()
        {
            Discordrpc.Shutdown();
        }
    }
}