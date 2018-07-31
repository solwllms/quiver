using System.Collections.Generic;
using System.IO;
using engine.display;
using engine.system;

namespace engine.game.types
{
    public class Monster : Dirsprite
    {
        // animation / graphical
        private readonly string rotTex;
        protected Animation anim;

        // used by THIS to handle logic
        private bool _movex;
        private Vector offs;
        private List<Vector> path;
        protected float speed = 0.5f / 32;
        protected Vector target;

        // used by CHILDs to handle their own logic
        protected bool attacking = false;
        protected int timer;

        public Monster(Vector pos, string texture) : base(texture, pos, false)
        {
            path = new List<Vector>();

            offs = new Vector((float) system.Engine.random.Next(3, 7) / 10, (float) system.Engine.random.Next(3, 7) / 10);
            sprwidth = 8;
            health = 100;
            stateLive = Livestate.Alive;

            rotTex = texture;
        }

        public void SetRotational()
        {
            anim = null;
            SetTexture(rotTex);
            dodirectional = true;
        }

        public void SetAnim(Animation a)
        {
            anim = a;
            anim.Play();
            SetTexture(a.texture);
            dodirectional = false;
        }

        public override void SetState(Livestate s)
        {
            base.SetState(s);
            if (s == Livestate.Dead)
            {
                OnKilled();
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (!ReferenceEquals(target, null) && stateLive != Livestate.Dead)
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
            if (path.Count <= 1)
            {
                velocity.SetTo(0, 0);
                target = null;
                path.Clear();
                return;
            }

            target = path[1];
            path.RemoveAt(0);
        }

        public virtual void GoTo(Vector goal)
        {
            Pathfinding.GeneratePathTo(pos.Floor(), goal.Floor(), offs, ref path);
            velocity.SetTo(0, 0);
            AdvancePathing();
        }

        public bool CanSeePlayer(int radius)
        {
            return Vis.CanHearPlayer(this, radius);
        }

        public float DistToPlayer()
        {
            return pos.DistanceTo(World.Player.pos);
        }
        
        public virtual void AliveTick() { }
        public virtual void DyingTick() { }
        public virtual void DeadTick() { }
        public virtual void OnKilled() { }

        public void StateTick()
        {
            if (Engine.frame % 5 == 0 && anim != null)
            {
                sprx = (uint) anim.currrent;
                anim.Step();
            }

            if (stateLive == Livestate.Alive)
            {
                if (health == 0)
                {
                    velocity.SetTo(0, 0);
                    stateLive = Livestate.Dying;
                    fetchignore = true;
                }
                else
                {
                    AliveTick();
                }
            }
            else if (stateLive == Livestate.Dying)
            {
                DyingTick();
            }
            else if (stateLive == Livestate.Dead)
            {
                DeadTick();
            }

            timer++;
        }

        public virtual void MoveTowards(Vector newpos)
        {
            var d = velocity.x != 0 && velocity.y != 0;
            var s = d ? speed * 1.2f : speed;

            velocity.Zero();
            if (pos.x < newpos.x) velocity.x = s;
            if (pos.x > newpos.x) velocity.x = -s;
            if (pos.y < newpos.y) velocity.y = s;
            if (pos.y > newpos.y) velocity.y = -s;

            if (system.Engine.frame % 20 == 0) _movex = !_movex;
            if (d)
            {
                if (!_movex)
                    velocity.x = 0;
                else
                    velocity.y = 0;
            }

            if (system.Engine.frame % 50 == 0) angle = (float) (pos - target).Angle();
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(path.Count);
            for (var i = 0; i < path.Count; i++)
            {
                var v = path[i];
                Vector.Serialize(ref v, ref w);
            }
            w.Write(timer);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            var n = r.ReadInt32();
            for (var i = 0; i < n; i++) path.Add(Vector.DeSerialize(ref r));
            timer = r.ReadInt32();
        }
    }
}