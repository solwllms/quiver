#region

using System.Collections.Generic;
using Quiver.display;
using Quiver.game;
using Quiver.game.types;

#endregion

namespace Quiver.system
{
    internal class vis
    {
        private static vector[] _degvecs;

        public static void Precomp()
        {
            _degvecs = new vector[360 * 50];

            float a = 0;
            for (var i = 0; i < 360 * 50; i++)
            {
                _degvecs[i] = new vector(a * 0.01745f);
                a += 0.05f;
            }
        }

        public static bool CanSeePlayer(ent e, int cone, int radius)
        {
            var pvec = world.Player.pos.Floor();
            var pvs = GetPvs(e.pos, (int) e.angle, cone, radius);
            return pvs.Contains(pvec);
        }

        public static bool CanHearPlayer(ent e, int distance)
        {
            var pvec = world.Player.pos.Floor();
            var pvs = GetPvs(e.pos, distance);
            return pvs.Contains(pvec);
        }

        public static List<vector> GetPvs(vector pos, int angle, int cone, int radius)
        {
            var visset = new List<vector>();
            visset.Add(pos);
            for (var i = -(cone / 2); i < cone / 2; i++)
                DoVis(ref visset, pos, Mod(angle + i, 360), radius);

            return visset;
        }

        public static List<vector> GetPvs(vector pos, int radius)
        {
            var visset = new List<vector>();
            visset.Add(pos);
            for (var i = 0; i < 360; i++)
                DoVis(ref visset, pos, i, radius);

            return visset;
        }

        public static HashSet<vector> GetLitSet(vector pos)
        {
            var visset = new HashSet<vector>();
            visset.Add(pos);
            for (var i = 0; i < 360 * 50; i++) DoVisLight(ref visset, pos, i);
            return visset;
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private static void DoVis(ref List<vector> visset, vector pos, int a, int radius)
        {
            for (var i = 0; i < radius; i++)
            {
                pos += _degvecs[a * 50];
                if (world.IsOpaque(pos)) return;

                if (!visset.Contains(pos)) visset.Add(pos.Floor());
            }
        }

        private static void DoVisLight(ref HashSet<vector> visset, vector pos, int a)
        {
            for (var i = 0; i < level.LightmapSize; i++)
            {
                pos += _degvecs[a];
                if (world.IsOpaque((pos / renderer.TEXSIZE).Floor())) return;

                if (!visset.Contains(pos)) visset.Add(pos.Floor());
            }
        }
    }
}