using engine.system;

namespace engine.game.types
{
    public class Projectile : Sprite
    {
        private readonly string _hitsound;

        protected int mindmg = 0;
        protected int maxdmg = 0;

        public Projectile(string texture, string hitsound, Vector pos, Vector dir, float speed, int mindmg, int maxdmg) : base(texture, pos,
            false)
        {
            this._hitsound = hitsound;

            this.mindmg = mindmg;
            this.maxdmg = maxdmg;

            sprwidth = 8;
            health = 100;
            stateLive = Livestate.Alive;
            velocity = dir / (100 - speed);
        }

        public override void Tick()
        {
            base.Tick();

            if (pos.DistanceTo(World.Player.pos) < 0.1)
            {
                World.Player.DoDamage((byte) system.Engine.random.Next(mindmg, maxdmg));
                OnCollide();
            }
        }

        public override void OnCollide()
        {
            base.OnCollide();

            Audio.PlaySound3D(_hitsound, pos, 40);
            Destroy();
        }
    }
}