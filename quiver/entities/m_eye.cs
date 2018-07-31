using engine.display;
using engine.game;
using engine.game.types;
using engine.progs;
using engine.system;

namespace game.entities
{
    internal class m_Eye : Monster
    {
        static Animation death = new Animation("sprites/eye_death", 0, 6, false);

        public m_Eye(Vector pos) : base(pos, "sprites/eye_rot")
        {
            SetRotational();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void AliveTick()
        {
            if (timer % 160 == 0)
            {
                timer = 0;
                GoTo(World.Player.pos);

                if (CanSeePlayer(7))
                {
                    Shoot();
                    timer = 0;
                }
            }
            else if(timer % 240 == 0)
                Audio.PlaySound3D("sound/eye/eyeidle" + engine.system.Engine.random.Next(1, 2), pos, 70);
        }

        void Shoot()
        {
            World.AddEnt((Ent)Progs.CreateEnt(2, pos, World.Player.pos + World.Player.velocity - pos));
        }

        public override void DyingTick()
        {
            if (anim != death) SetAnim(death);
            else if (anim.playing == false)
            {
                SetState(Livestate.Dead);
            }
        }

        public override void OnKilled()
        {
            SetAnim(death);
            anim.Stop();
            Audio.PlaySound3D("sound/eye/eyekill", pos, 200);
        }

        public override void DeadTick()
        {
            sprx = 6;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);
            Audio.PlaySound3D("sound/eye/eyepain", pos);
        }
    }
}