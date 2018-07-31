using System;
using System.IO;
using engine.system;

namespace engine.game.types
{
    public class Dirsprite : Sprite
    {
        protected bool dodirectional = true;

        public Dirsprite(string tex, Vector pos, bool isstatic, bool solid = false) : base(tex, pos, isstatic, solid)
        {
            this.isstatic = isstatic;
        }

        public override void Tick()
        {
            base.Tick();
            //logger.WriteLine(vis.CanSeePlayer(this, 130, 5));

            if (dodirectional)
            {
                var a = ((World.Player.pos - pos).Angle() - angle) % 360;
                sprx = (uint) Math.Abs((int) a + 180) / 45;
            }
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(dodirectional);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            dodirectional = r.ReadBoolean();
        }
    }
}