using System;
using System.IO;
using engine.system;

namespace engine.game.types
{
    public enum Livestate
    {
        Dead,
        Dying,
        Alive
    }

    public class Ent : Saveable
    {
        public float angle;

        public float collisionerror = 0.2f;

        public byte health = 100;
        public int id;
        public bool isstatic = false;
        public Vector pos;

        protected Livestate stateLive = Livestate.Alive;

        public Vector velocity;

        public Ent(Vector pos)
        {
            id = progs.Progs.GetEntId(GetType());

            velocity = new Vector(0, 0);
            this.pos = pos;
        }

        public virtual void Tick()
        {
            if (!World.map[(int) (pos.x + velocity.x + collisionerror), (int) (pos.y + velocity.y + collisionerror)]
                .solid && stateLive != Livestate.Dead)
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

        public virtual void SetPos(Vector p)
        {
            pos = p;
        }

        public virtual void SetState(Livestate s)
        {
            stateLive = s;
        }
        public Livestate GetState()
        {
            return stateLive;
        }

        public void Destroy()
        {
            World.DestroyEnt(this);
        }
        public virtual void OnDestroy()
        {
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            Vector.Serialize(ref pos, ref w);
            Vector.Serialize(ref velocity, ref w);
            w.Write(Convert.ToByte(health));
            w.Write(Convert.ToInt32(stateLive));
            w.Write(Convert.ToSingle(angle));
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            pos = Vector.DeSerialize(ref r);
            velocity = Vector.DeSerialize(ref r);
            health = r.ReadByte();
            stateLive = (Livestate) r.ReadInt32();
            angle = r.ReadSingle();
        }
    }
}