using System;
using System.IO;
using engine.display;
using engine.game.types;
using engine.progs;
using engine.system;
using SFML.System;
using SFML.Window;
// ReSharper disable ObjectCreationAsStatement

namespace engine.game
{
    public class Player : Ent
    {
        private float _move;

        private float _turn;

        public Weapon weapon;

        public Player(Vector pos) : base(pos)
        {
            health = 100;
            collisionerror = 0;

            weapon = progs.Progs.CreateWeapon(0);
        }

        public static void Initcmds()
        {
            Cmd.Register("+forward", new Command(delegate
            {
                World.Player?.Move(Cmd.GetValuef("movspeed"));
                return true;
            }, record: true));
            Cmd.Register("-forward", new Command(delegate
            {
                World.Player?.Move(0);
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.Up, "+forward");

            Cmd.Register("+back", new Command(delegate
            {
                World.Player?.Move(-Cmd.GetValuef("movspeed"));
                return true;
            }, record: true));
            Cmd.Register("-back", new Command(delegate
            {
                World.Player?.Move(0);
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.Down, "+back");

            Cmd.Register("+left", new Command(delegate
            {
                World.Player?.Turn(Cmd.GetValuef("movspeed"));
                return true;
            }, record: true));
            Cmd.Register("-left", new Command(delegate
            {
                World.Player?.Turn(0);
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.Left, "+left");

            Cmd.Register("+right", new Command(delegate
            {
                World.Player?.Turn(-Cmd.GetValuef("movspeed"));
                return true;
            }, record: true));
            Cmd.Register("-right", new Command(delegate
            {
                World.Player?.Turn(0);
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.Right, "+right");

            Cmd.Register("use", new Command(delegate
            {
                World.Player?.Use();
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.E, "use");

            Cmd.Register("fire", new Command(delegate
            {
                World.Player?.Fire();
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.Space, "fire");

            Cmd.Register("reload", new Command(delegate
            {
                World.Player?.Reload();
                return true;
            }, record: true));
            Cmd.Bind(Keyboard.Key.R, "reload");

            Cmd.Register("kill", new Command(delegate
            {
                World.Player.health = 0;
                return true;
            }, record: true));

            Cmd.Register("refresh", new Command(delegate
            {
                Cache.ClearAll();
                return true;
            }));


            new Cvar("movspeed", "0.05", false, cheat: true);
            new Cvar("rotspeed", "1", false, cheat: true);

            new Cvar("nofollow", "0", false, cheat: true, toggle: true);
        }

        public override void Tick()
        {
            var ps = pos;
            base.Tick();
            weapon.Tick();

            velocity.SetTo(0, 0);
            Movetick();
            Turntick();

            if (_move != 0 && Engine.frame % 20 == 0)
                Audio.PlaySound2D("sound/player/step" + system.Engine.random.Next(1, 4));
        }

        public void Turntick()
        {
            angle += _turn;
            Renderer.dirX = Math.Cos(angle);
            Renderer.dirY = Math.Sin(angle);
            Renderer.planeX = Renderer.dirY * (0.66f * 1.33f);
            Renderer.planeY = -Renderer.dirX * (0.66f * 1.33f);
        }

        public void Movetick()
        {
            if (!World.map[(int) (pos.x + Renderer.dirX * _move), (int) pos.y].solid)
                pos.x += (float) Renderer.dirX * _move;
            if (!World.map[(int) pos.x, (int) (pos.y + Renderer.dirY * _move)].solid)
                pos.y += (float) Renderer.dirY * _move;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);

            Audio.PlaySound2D("sound/player/hurt");
        }

        public void Turn(float speed)
        {
            _turn = speed;
        }

        public void Move(float speed)
        {
            _move = speed;
        }

        public void Use()
        {
            if (Renderer.GetLookedAt().interactable && Renderer.centercell.dist < 1.5f)
                Progs.CallMapEvent(Renderer.GetLookedAt().onInteract, Renderer.GetLookedAt());
        }

        public void Fire()
        {
            weapon.Fire();
        }

        public void Reload()
        {
            weapon.Reload();
        }

        public Ent GetLookatEntity()
        {
            if (Renderer.centersprite.dist != -1 && Renderer.centersprite.index < World.sprites.Length)
                return World.sprites[Renderer.centersprite.index];
            return null;
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            Time.FromSeconds(90);
            new Clock();
            w.Write((int) World.clock.ElapsedTime.AsSeconds());
            weapon.DoParseSave(ref w);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            World.startingSec = r.ReadInt32();
            weapon.DoParseLoad(ref r);
        }
    }
}