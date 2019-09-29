#region

using Quiver.game.types;
using Quiver.system;

#endregion

namespace game.entities
{
    internal class p_Guts : projectile
    {
        public p_Guts(vector pos) : base("sprites/guts", "sound/guts/hit", pos, new vector(0, 0), 40, 6, 9)
        {
        }

        public p_Guts(vector pos, vector dir) : base("sprites/guts", "sound/guts/hit", pos, dir, 40, 6, 9)
        {
        }
    }
}