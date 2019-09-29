#region

using Quiver.Audio;
using Quiver.system;

#endregion

namespace Quiver.game.types
{
    public class projectile : sprite
    {
        private readonly string _hitsound;
        protected int maxdmg;

        protected int mindmg;

        public projectile(string texture, string hitsound, vector pos, vector dir, float speed, int mindmg,
            int maxdmg) : base(texture, pos,
            false)
        {
            _hitsound = hitsound;

            this.mindmg = mindmg;
            this.maxdmg = maxdmg;

            sprwidth = 8;
            health = 100;
            stateLive = livestate.Alive;
            velocity = dir / (100 - speed);
        }

        public override void Tick()
        {
            base.Tick();

            if (pos.DistanceTo(world.Player.pos) < 0.1)
            {
                world.Player.DoDamage((byte) engine.random.Next(mindmg, maxdmg));
                OnCollide();
            }
        }

        public override void OnCollide()
        {
            base.OnCollide();

            audio.PlaySound3D(_hitsound, pos, 40);
            Destroy();
        }
    }
}