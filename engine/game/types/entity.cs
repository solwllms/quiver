#region

using System;
using System.IO;
using Quiver.system;

#endregion

namespace Quiver.game.types
{
    public enum livestate
    {
        Dead,
        Dying,
        Alive
    }

    public class ent : saveable
    {
        public float angle;

        public float collisionerror = 0.2f;

        public byte health = 100;
        public int id;
        public bool isstatic = false;
        public vector pos;

        protected livestate stateLive = livestate.Alive;

        public vector velocity;

        public ent(vector pos)
        {
            id = progs.GetEntId(GetType());

            velocity = new vector(0, 0);
            this.pos = pos;
        }

        public virtual void Tick()
        {
            if (!world.map[(int) (pos.x + velocity.x + collisionerror), (int) (pos.y + velocity.y + collisionerror)]
                    .solid && stateLive != livestate.Dead)
                pos += velocity;
            else
                OnCollide();
        }

        public virtual void OnCollide()
        {
        }

        public virtual void DoDamage(byte damage)
        {
            var h = health - damage;
            if (h < 0) h = 0;
            if (h > 255) h = 255;
            health = (byte) h;
        }

        public virtual void SetPos(vector p)
        {
            pos = p;
        }

        public virtual void SetState(livestate s)
        {
            stateLive = s;
        }

        public livestate GetState()
        {
            return stateLive;
        }

        public void Destroy()
        {
            world.DestroyEnt(this);
        }

        public virtual void OnDestroy()
        {
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            vector.Serialize(ref pos, ref w);
            vector.Serialize(ref velocity, ref w);
            w.Write(Convert.ToByte(health));
            w.Write(Convert.ToInt32(stateLive));
            w.Write(Convert.ToSingle(angle));
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            pos = vector.DeSerialize(ref r);
            velocity = vector.DeSerialize(ref r);
            health = r.ReadByte();
            stateLive = (livestate) r.ReadInt32();
            angle = r.ReadSingle();
        }
    }
}