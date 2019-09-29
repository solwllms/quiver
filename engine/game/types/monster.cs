#region

using System.Collections.Generic;
using System.IO;
using Quiver.display;
using Quiver.system;

#endregion

namespace Quiver.game.types
{
    public class monster : dirsprite
    {
        private readonly vector _offs;

        // animation / graphical
        private readonly string _rotTex;

        // used by THIS to handle logic
        private bool _movex;
        protected animation anim;

        // used by CHILDs to handle their own logic
        protected bool attacking = false;
        private List<vector> _path;
        protected float speed = 0.5f / 32;
        protected vector target;
        protected int timer;

        public monster(vector pos, string texture) : base(texture, pos, false)
        {
            _path = new List<vector>();

            _offs = new vector((float) engine.random.Next(3, 7) / 10, (float) engine.random.Next(3, 7) / 10);
            sprwidth = 8;
            health = 100;
            stateLive = livestate.Alive;

            _rotTex = texture;
        }

        public void SetRotational()
        {
            anim = null;
            SetTexture(_rotTex);
            dodirectional = true;
        }

        public void SetAnim(animation a)
        {
            anim = a;
            anim.Play();
            SetTexture(a.texture);
            dodirectional = false;
        }

        public override void SetState(livestate s)
        {
            base.SetState(s);
            if (s == livestate.Dead) OnKilled();
        }

        public override void Tick()
        {
            base.Tick();

            if (!ReferenceEquals(target, null) && stateLive != livestate.Dead)
            {
                if (pos.Floor().Equals(target.Floor())) AdvancePathing();

                if (!ReferenceEquals(target, null))
                    MoveTowards(target);
            }

            StateTick();
            //Log.WriteLine("state: "+stateLive);
        }

        public virtual void AdvancePathing()
        {
            if (_path.Count <= 1)
            {
                velocity.SetTo(0, 0);
                target = null;
                _path.Clear();
                return;
            }

            target = _path[1];
            _path.RemoveAt(0);
        }

        public virtual void GoTo(vector goal)
        {
            pathfinding.GeneratePathTo(pos.Floor(), goal.Floor(), _offs, ref _path);
            velocity.SetTo(0, 0);
            AdvancePathing();
        }

        public bool CanSeePlayer(int radius)
        {
            return vis.CanHearPlayer(this, radius);
        }

        public float DistToPlayer()
        {
            return pos.DistanceTo(world.Player.pos);
        }

        public virtual void AliveTick()
        {
        }

        public virtual void DyingTick()
        {
        }

        public virtual void DeadTick()
        {
        }

        public virtual void OnKilled()
        {
        }

        public void StateTick()
        {
            if (engine.frame % 5 == 0 && anim != null)
            {
                sprx = (uint) anim.currrent;
                anim.Step();
            }

            if (stateLive == livestate.Alive)
            {
                if (health == 0)
                {
                    velocity.SetTo(0, 0);
                    stateLive = livestate.Dying;
                    fetchignore = true;
                }
                else
                {
                    AliveTick();
                }
            }
            else if (stateLive == livestate.Dying)
            {
                DyingTick();
            }
            else if (stateLive == livestate.Dead)
            {
                DeadTick();
            }

            timer++;
        }

        public virtual void MoveTowards(vector newpos)
        {
            var d = velocity.x != 0 && velocity.y != 0;
            var s = d ? speed * 1.2f : speed;

            velocity.Zero();
            if (pos.x < newpos.x) velocity.x = s;
            if (pos.x > newpos.x) velocity.x = -s;
            if (pos.y < newpos.y) velocity.y = s;
            if (pos.y > newpos.y) velocity.y = -s;

            if (engine.frame % 20 == 0) _movex = !_movex;
            if (d)
            {
                if (!_movex)
                    velocity.x = 0;
                else
                    velocity.y = 0;
            }

            if (engine.frame % 50 == 0) angle = (float) (pos - target).Angle();
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(_path.Count);
            for (var i = 0; i < _path.Count; i++)
            {
                var v = _path[i];
                vector.Serialize(ref v, ref w);
            }

            w.Write(timer);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            var n = r.ReadInt32();
            for (var i = 0; i < n; i++) _path.Add(vector.DeSerialize(ref r));
            timer = r.ReadInt32();
        }
    }
}