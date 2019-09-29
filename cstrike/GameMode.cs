#region

using Quiver;
using Quiver.display;
using Quiver.game;
using Quiver.system;
using game.states;
using System.Drawing;

#endregion

namespace game
{
    internal class gameMode : gmbase
    {
        private int _prevC;
        private byte _prevH;

        public override void Start()
        {
            base.Start();
            ResetRgb();
        }

        public override void Tick()
        {
            base.Tick();
            if (world.Player.health == 0)
            {
                statemanager.SetState(new menu());
                End();
            }

            if (world.Player.health < _prevH && world.Player.health < 100) rgbDevice.SetAll(255, 0, 0);
            _prevH = world.Player.health;

            if (world.Player.weapon.clip < _prevC) rgbDevice.SetAll(255, 255, 0);
            _prevC = world.Player.weapon.clip;

            if (rgbDevice.hasChanged && Quiver.engine.frame % 7 == 0)
                ResetRgb();
        }

        private void ResetRgb()
        {
            rgbDevice.hasChanged = false;
            rgbDevice.SetAll(0, 0, 0);

            foreach (var b in cmd.binds.Keys) rgbDevice.SetBind(b, 0, 255, 0);
        }

        public override void DrawHud()
        {
            cache.GetTexture("gui/radar").Draw(4, 2);

            gui.Write("&", 5, 82, Color.DarkOrange);
            gui.Write(world.Player.health, 11, 82, Color.DarkOrange);

            gui.Write(0, 45, 82, Color.DarkOrange);

            //Cache.GetTexture("gui/icon_pistol").Draw(110, 82);
            DrawDollarSign(130, 75);
            gui.Write("800", 139, 75, Color.DarkOrange);

            gui.Write(world.Player.weapon.clip, 137, 82, Color.DarkOrange);
            gui.Write(world.Player.weapon.nonclip, 147, 82, Color.DarkOrange);

            screen.SetPixel(screen.width / 2, screen.height / 2, Color.Green);
        }

        void DrawDollarSign(uint x, uint y)
        {
            gui.Write("S", x, y, Color.DarkOrange);
            screen.SetPixel(x + 1, y, Color.DarkOrange);
            screen.SetPixel(x + 1, y+2, Color.DarkOrange);
            screen.SetPixel(x + 1, y+4, Color.DarkOrange);
            screen.SetPixel(x + 1, y+6, Color.DarkOrange);
        }

        public override void End()
        {
            base.End();
        }
    }
}