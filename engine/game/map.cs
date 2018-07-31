using engine.game.types;
using engine.system;

namespace engine.game
{
    public class Level
    {
        public static string skybox = "textures/sky1";
        public static string name;
        public static int[,] data;

        public static string prev = "";
        public static string next = "";

        public static void GenerateMap(string map, bool fresh, bool genents)
        {
            name = map;
            Lvl.ReadLevel(map);
            PostLoad(fresh, genents);

            Discordrpc.Update("playing " + name, "");
        }

        static Vector _playerSpawn;
        public static void Generate(int[,] d)
        {
            data = d;
            World.mapsize = d.GetLength(0);
            World.map = new Mapcell[World.mapsize, World.mapsize];

            for (var x = 0; x < World.mapsize; x++)
            for (var y = 0; y < World.mapsize; y++)
            {
                var cell = new Mapcell(new Vector(x, y), "textures/floor", "textures/floor", "textures/floor");
                if (data[x, y] == -1)
                {
                    _playerSpawn = new Vector(x + 0.5f, y + 0.5f);
                }

                Lvl.GetCell(ref cell, data[x, y].ToString());
                World.map[x, y] = cell;
            }
        }

        public static void Genents(bool fresh)
        {
            for (var x = 0; x < World.mapsize; x++)
            for (var y = 0; y < World.mapsize; y++)
            {
                var e = Lvl.GetEnt(new Vector(x, y), data[x, y].ToString());
                if (e != null)
                {
                    Genent(e, fresh);
                }
            }
        }

        private static void Genent(Ent e, bool fresh)
        {
            if (fresh || e.isstatic) World.AddEnt(e);
        }

        public static void PostLoad(bool fresh, bool genents = false)
        {
            if (fresh) World.CreatePlayer(_playerSpawn);
            else World.Player.SetPos(_playerSpawn);

            Genents(genents);
            World.Tick();
        }
    }
}