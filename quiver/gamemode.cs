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
                statemanager.SetState(new gameover());
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
            gui.Write("&", 5, 82);
            gui.Write(world.Player.health, 11, 82, world.Player.health > 15 ? Color.White : Color.Red);
            gui.Write("^" + world.GetPlaythruTime(), 45, 82);

            //Cache.GetTexture("gui/icon_pistol").Draw(110, 82);
            gui.Write(world.Player.weapon.clip, 130, 82);
            gui.Write(world.Player.weapon.nonclip, 140, 82);

            screen.SetPixel(screen.width / 2, screen.height / 2, Color.White);

            if (renderer.GetCenterMapCell().interactable && renderer.GetCenterCell().dist < 1.5f)
                gui.PromptBottom("[" + input.GetKeyName(cmd.Getbind("use")) + "] " + lang.Get("$game.use"));

            if (world.Player.weapon.reloadMsgTime != -1)
                gui.Write(lang.Get("$game.reload"), 110, (uint) (50 + world.Player.weapon.reloadMsgTime),
                    20 / (world.Player.weapon.reloadMsgTime + 1) + 1);
        }

        public override void End()
        {
            base.End();
        }
    }
}