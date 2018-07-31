using engine.game.types;
using engine.system;

namespace game.entities
{
    internal class p_Guts : Projectile
    {
        public p_Guts(Vector pos) : base("sprites/guts", "sound/guts/hit", pos, new Vector(0, 0), 40, 6, 9)
        {
        }

        public p_Guts(Vector pos, Vector dir) : base("sprites/guts", "sound/guts/hit", pos, dir, 40, 6, 9)
        {
        }
    }
}