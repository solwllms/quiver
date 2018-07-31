using engine.display;
using engine.game;
using engine.game.types;
using engine.progs;
using engine.system;

namespace game.entities
{
    internal class m_Robo : Monster
    {
        static Animation shoot = new Animation("sprites/robo_shoot", 0, 7, false);
        static Animation death = new Animation("sprites/robo_death", 0, 7, false);

        public m_Robo(Vector pos) : base(pos, "sprites/robo_rot")
        {
            SetRotational();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void AliveTick()
        {
            if (attacking)
            {
                if (sprx == 7)
                {
                    Shoot();
                    attacking = false;
                    SetRotational();
                }
            }
            else if (timer % 160 == 0)
            {
                timer = 0;
                GoTo(World.Player.pos);

                if (CanSeePlayer(7))
                {
                    SetAnim(shoot);
                    attacking = true;
                    timer = 0;
                }
            }
        }

        void Shoot()
        {
            Audio.PlaySound3D("sound/robo/lazer", pos, 80);
            World.AddEnt((Ent)Progs.CreateEnt(4, pos, World.Player.pos + World.Player.velocity - pos));
        }

        public override void DyingTick()
        {
            if(anim != death) SetAnim(death);
            else if (anim.playing == false)
            {
                SetState(Livestate.Dead);
            }
        }

        public override void OnKilled()
        {
            SetAnim(death);
            anim.Stop();
            Audio.PlaySound3D("sound/robo/explosion", pos);
        }

        public override void DeadTick()
        {
            sprx = 7;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);
            Audio.PlaySound3D("sound/robo/robot", pos);
        }
    }
}