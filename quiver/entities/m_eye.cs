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
    internal class m_Eye : monster
    {
        private static readonly animation Death = new animation("sprites/eye_death", 0, 6, false);

        public m_Eye(vector pos) : base(pos, "sprites/eye_rot")
        {
            SetRotational();
        }

        public override void AliveTick()
        {
            if (timer % 160 == 0)
            {
                timer = 0;
                GoTo(world.Player.pos);

                if (CanSeePlayer(7))
                {
                    Shoot();
                    timer = 0;
                }
            }
            else if (timer % 240 == 0)
            {
                audio.PlaySound3D("sound/eye/eyeidle" + Quiver.engine.random.Next(1, 2), pos, 70);
            }
        }

        private void Shoot()
        {
            world.AddEnt((ent) progs.CreateEnt(2, pos, world.Player.pos + world.Player.velocity - pos));
        }

        public override void DyingTick()
        {
            if (anim != Death) SetAnim(Death);
            else if (anim.playing == false) SetState(livestate.Dead);
        }

        public override void OnKilled()
        {
            SetAnim(Death);
            anim.Stop();
            audio.PlaySound3D("sound/eye/eyekill", pos, 200);
        }

        public override void DeadTick()
        {
            sprx = 6;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);
            audio.PlaySound3D("sound/eye/eyepain", pos);
        }
    }
}