#region

using Quiver;
using Quiver.Audio;
using Quiver.display;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;

#endregion

namespace game.entities
{
    internal class m_Robo : monster
    {
        private static readonly animation AnimShoot = new animation("sprites/robo_shoot", 0, 7, false);
        private static readonly animation AnimDeath = new animation("sprites/robo_death", 0, 7, false);

        public m_Robo(vector pos) : base(pos, "sprites/robo_rot")
        {
            SetRotational();
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
                GoTo(world.Player.pos);

                if (CanSeePlayer(7))
                {
                    SetAnim(AnimShoot);
                    attacking = true;
                    timer = 0;
                }
            }
        }

        private void Shoot()
        {
            audio.PlaySound3D("sound/robo/lazer", pos, 80);
            world.AddEnt((ent) progs.CreateEnt(4, pos, world.Player.pos + world.Player.velocity - pos));
        }

        public override void DyingTick()
        {
            if (anim != AnimDeath) SetAnim(AnimDeath);
            else if (anim.playing == false) SetState(livestate.Dead);
        }

        public override void OnKilled()
        {
            SetAnim(AnimDeath);
            anim.Stop();
            audio.PlaySound3D("sound/robo/explosion", pos);
        }

        public override void DeadTick()
        {
            sprx = 7;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);
            audio.PlaySound3D("sound/robo/robot", pos);
        }
    }
}