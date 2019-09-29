#region

using System;
using Quiver.display;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;

#endregion

namespace game.weapons
{
    public class wRevolver : weapon
    {
        private static int _it;

        public wRevolver() : base(new animation(0, 3, false), 6, 90, 60, 20, 40, "sound/revolver/shoot",
            "sound/revolver/reload",
            "sound/revolver/empty")
        {
        }

        public override void Reload()
        {
            SetAnimation(new animation(8, 15, false));
            GetAnimation().Play();
            base.Reload();
        }

        public override void Draw(vector velocity)
        {
            float bobOscillate = 0;

            if (_it != -1) _it++;
            if (!ReferenceEquals(null, velocity))
            {
                var m = (float) Math.Sqrt(velocity.x * velocity.x +
                                          velocity.y * velocity.y);
                if (m != 0 && _it == -1) _it = 0;
                if (m == 0) _it = -1;

                if (_it != -1) _it++;
                bobOscillate = (float) Math.Sin(_it * m / 10 * (2 * Math.PI));
            }

            var wx = screen.width / 2 + 16;
            wx += (uint) (bobOscillate * 3);
            uint wy = 89 - 45;
            wy += (uint) (bobOscillate * 2);

            var i = world.Player.weapon.GetAnimFrame();
            var x = i % GetAnimation().framesPerLine;
            var y = i / GetAnimation().framesPerLine;
            DrawViewmodel(cache.GetTexture("gui/weapons/pistol"), wx, wy, (uint) x * 32, (uint) y * 48, 32, 48);
        }
    }
}