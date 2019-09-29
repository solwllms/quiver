#region

using System;
using System.IO;
using Quiver.display;
using Quiver.system;

#endregion

namespace Quiver.game.types
{
    public class sprite : ent
    {
        private string _texname;
        public bool fetchignore = false;

        public bool solid;

        public uint sprheight = 16;
        public uint sprwidth = 16;
        public uint sprx;

        public sprite(vector pos) : this("err", pos, true, true)
        {
        }

        public sprite(string tex, vector pos, bool isstatic, bool solid = false) : base(pos)
        {
            SetTexture(tex);
            this.isstatic = isstatic;
            this.solid = solid;

            SetPos(pos);
        }

        public texture Tex { get; private set; }

        public void SetTexture(string name)
        {
            _texname = name;
            Tex = cache.GetTexture(name);
        }

        public string GetTextureName()
        {
            return _texname;
        }

        public override void SetPos(vector p)
        {
            base.SetPos(p);
            UpdateCollision();
        }

        /* remarks any nessesary tiles on the collision map for solidness */
        public void UpdateCollision()
        {
            if (!solid)
                return;

            world.map[(int) Math.Floor(pos.x), (int) Math.Floor(pos.y)].solid = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            world.RemoveEnt(this);
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(sprx);
            w.Write(saveload.Str(_texname, 32));
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            sprx = r.ReadUInt32();
            SetTexture(new string(r.ReadChars(32)).Trim());
        }
    }
}