using System.Collections.Generic;
using engine.game;
using engine.game.types;

namespace engine.system
{
    internal class Vis
    {
        private static Vector[] _degvecs;

        public static void Precomp()
        {
            _degvecs = new Vector[360];
            for (var i = 0; i < 360; i++) _degvecs[i] = new Vector(i * 0.01745f);
        }

        public static bool CanSeePlayer(Ent e, int cone, int radius)
        {
            var pvec = World.Player.pos.Floor();
            var pvs = GetPvs(e.pos, (int) e.angle, cone, radius);
            return pvs.Contains(pvec);
        }

        public static bool CanHearPlayer(Ent e, int distance)
        {
            var pvec = World.Player.pos.Floor();
            var pvs = GetPvs(e.pos, distance);
            return pvs.Contains(pvec);
        }

        public static List<Vector> GetPvs(Vector pos, int angle, int cone, int radius)
        {
            var visset = new List<Vector>();
            visset.Add(pos);
            for (var i = -(cone / 2); i < cone / 2; i++)
                DoVis(ref visset, pos, Mod(angle + i, 360), radius);

            return visset;
        }

        public static List<Vector> GetPvs(Vector pos, int radius)
        {
            var visset = new List<Vector>();
            visset.Add(pos);
            for (var i = 0; i < 360; i++)
                DoVis(ref visset, pos, i, radius);

            return visset;
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private static void DoVis(ref List<Vector> visset, Vector pos, int a, int radius)
        {
            for (var i = 0; i < radius; i++)
            {
                pos += _degvecs[a];
                if (World.IsSeethrough(pos)) return;

                if (!visset.Contains(pos)) visset.Add(pos.Floor());
            }
        }
    }
}