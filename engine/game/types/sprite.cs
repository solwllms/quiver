using System;
using System.IO;
using engine.display;
using engine.system;

namespace engine.game.types
{
    public class Sprite : Ent
    {
        public bool fetchignore = false;

        public bool solid;

        public uint sprheight = 16;
        public uint sprwidth = 16;
        public uint sprx;
        private string _texname;

        public Sprite(Vector pos) : this("err", pos, true, true)
        {

        }

        public Sprite(string tex, Vector pos, bool isstatic, bool solid = false) : base(pos)
        {
            SetTexture(tex);
            this.isstatic = isstatic;
            this.solid = solid;

            SetPos(pos);
        }

        public Texture Tex { get; private set; }

        public void SetTexture(string name)
        {
            _texname = name;
            Tex = Cache.GetTexture(name);
        }
        public string GetTextureName()
        {
            return _texname;
        }

        public override void SetPos(Vector p)
        {
            base.SetPos(p);
            UpdateCollision();
        }

        /* remarks any nessesary tiles on the collision map for solidness */
        public void UpdateCollision()
        {
            if (!solid)
                return;

            World.map[(int) Math.Floor(pos.x), (int) Math.Floor(pos.y)].solid = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            World.RemoveEnt(this);
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(sprx);
            w.Write(Saveload.Str(_texname, 32));
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            sprx = r.ReadUInt32();
            SetTexture(new string(r.ReadChars(32)).Trim());
        }
    }
}