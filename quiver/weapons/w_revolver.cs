using System;
using engine.display;
using engine.game;
using engine.game.types;
using engine.system;

namespace game.weapons
{
    public class WRevolver : Weapon
    {
        public WRevolver() : base(new Animation(0, 3, false), 6 , 90, 60, 20, 40, "sound/revolver/shoot", "sound/revolver/reload",
            "sound/revolver/empty")
        {
        }

        public override void Reload()
        {
            SetAnimation(new Animation(8, 15, false));
            GetAnimation().Play();
            base.Reload();
        }

        static int it = 0;
        public override void Draw(Vector velocity)
        {
            float bobOscillate = 0;

            if (it != -1) { it++; }
            if (!object.ReferenceEquals(null, velocity))
            {
                float m = (float)(Math.Sqrt(velocity.x * velocity.x +
                                            velocity.y * velocity.y));
                if (m != 0 && it == -1) it = 0;
                if (m == 0) it = -1;

                if (it != -1) { it++; }
                bobOscillate = (float)Math.Sin((float)(it * m) / 10 * (2 * Math.PI));
            }
            
            uint wx = (Screen.width / 2) + 16;
            wx += (uint)(bobOscillate * 3);
            uint wy = (89 - 45);
            wy += (uint)(bobOscillate * 2);

            int i = World.Player.weapon.GetAnimFrame();
            int x = i % GetAnimation().framesPerLine;
            int y = i / GetAnimation().framesPerLine;
            Cache.GetTexture("gui/weapons/pistol").Draw(wx, wy, (uint)x * 32, (uint)y * 48, 32, 48);
        }
    }
}