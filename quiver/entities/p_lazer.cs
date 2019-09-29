#region

using Quiver.game.types;
using Quiver.system;

#endregion

namespace game.entities
{
    internal class p_Lazer : projectile
    {
        //private Sound _s;

        public p_Lazer(vector pos, vector dir) : base("sprites/lazer", "", pos, dir, 50, 6, 9)
        {
            //_s = audio.PlaySound3D("sound/robo/lazer_move", pos, 2);
        }

        public override void Tick()
        {
            base.Tick();
            //if (_s != null) _s.Position = new Vector3f(pos.x, 0, pos.y);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //_s.Stop();
            //_s = null;
        }
    }
}