#region

using System;
using System.IO;
using Quiver.Audio;
using Quiver.display;
using Quiver.game.types;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.game
{
    public class player : dirsprite
    {
        public static cvar cvarFov = new cvar("fov", "66", false, cheat: true);
        public static cvar cvarMovespeed = new cvar("movspeed", "0.05", false, cheat: true);
        public static cvar cvarRotspeed = new cvar("rotspeed", "0.05", false, cheat: true);
        //public static cvar cvarNofollow = new cvar("nofollow", "0", false, cheat: true, toggle: true);

        public weapon weapon;
        public vector dir = new vector(-1, 0);

        private bool _localPlayer;
        public bool isLocalPlayer
        {
            get
            {
                return _localPlayer;
            }
            set
            {
                _localPlayer = value;
                visible = !_localPlayer;
            }
        }

        private float _move;
        private float _turn;
        private float _strafe;

        public player(vector pos) : base("sprites/robo_rot", pos, false)
        {
            health = 100;
            collisionerror = 0;
            isLocalPlayer = true;

            sprwidth = 8;

            try
            {
                weapon = progs.CreateWeapon(0);
            }
            catch
            {
                weapon = new weapon(null, 0, 0, 0, 0, 0, "", "", "");
            }
        }

        public static void Initcmds()
        {
            cmd.Register(new command("+forward", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Move(cvarMovespeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-forward", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Move(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.W, "+forward");

            cmd.Register(new command("+strafeleft", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Strafe(cvarMovespeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-strafeleft", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Strafe(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.Q, "+strafeleft");

            cmd.Register(new command("+straferight", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Strafe(-cvarMovespeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-straferight", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Strafe(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.E, "+straferight");

            cmd.Register(new command("+back", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Move(-cvarMovespeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-back", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Move(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.S, "+back");

            cmd.Register(new command("+left", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Turn(cvarRotspeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-left", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Turn(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.A, "+left");

            cmd.Register(new command("+right", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Turn(-cvarRotspeed.Valuef());
                return true;
            }, record: true, sendToServer: true));
            cmd.Register(new command("-right", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Turn(0);
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.D, "+right");

            cmd.Register(new command("use", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Use();
                return true;
            }, record: true, sendToServer: true));
            cmd.Bind(Key.F, "use");

            cmd.Register(new command("fire", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Fire();
                return true;
            }, record: true));
            cmd.Bind(Key.Space, "fire");

            cmd.Register(new command("reload", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id)?.Reload();
                return true;
            }, record: true));
            cmd.Bind(Key.R, "reload");

            cmd.Register(new command("kill", delegate (int id, string[] param)
            {
                game.GetPlayerEnt(id).health = 0;
                return true;
            }, record: true));

            cmd.Register(new command("refresh", delegate (int id, string[] param)
            {
                cache.ClearAll();
                return true;
            }));
        }

        public override void Tick()
        {
            var ps = pos;
            base.Tick();
            weapon.Tick();

            velocity.SetTo(0, 0);
            Turntick();
            Movetick();

            if (_move != 0 && engine.frame % 20 == 0)
                audio.PlaySound("sound/player/step" + engine.random.Next(1, 4));
        }

        public void Turntick()
        {
            angle += _turn;
            dir.x = (float) Math.Cos(angle);
            dir.y = (float) Math.Sin(angle);

            if (isLocalPlayer)
            {
                float fov = cvarFov.Valuef() / 100;
                renderer.camPlane.x = dir.y * (fov * 1.33f);
                renderer.camPlane.y = -dir.x * (fov * 1.33f);
            }
        }

        public void Movetick()
        {
            if (!world.map[(int) (pos.x + dir.x * _move), (int) pos.y].solid)
                pos.x += dir.x * _move;
            if (!world.map[(int) pos.x, (int) (pos.y + dir.y * _move)].solid)
                pos.y += dir.y * _move;

            vector strafeDir = new vector(-dir.y, dir.x);
            if (!world.map[(int)(pos.x + strafeDir.x * _strafe), (int)pos.y].solid)
                pos.x += strafeDir.x * _strafe;
            if (!world.map[(int)pos.x, (int)(pos.y + strafeDir.y * _strafe)].solid)
                pos.y += strafeDir.y * _strafe;
        }

        public override void DoDamage(byte damage)
        {
            base.DoDamage(damage);

            audio.PlaySound("sound/player/hurt");
        }

        public void Turn(float speed)
        {
            _turn = speed;
        }
        public void Move(float speed)
        {
            _move = speed;
        }
        public void Strafe(float speed)
        {
            _strafe = speed;
        }

        public void Use()
        {
            if (renderer.GetCenterMapCell().interactable && renderer.GetCenterCell().dist < 1.5f)
                progs.CallMapEvent(renderer.GetCenterMapCell().onInteract, renderer.GetCenterMapCell());
        }

        public void Fire()
        {
            weapon.Fire();
        }

        public void Reload()
        {
            weapon.Reload();
        }

        public ent GetLookatEntity()
        {
            if (renderer.GetCenterSprite().dist != -1 && renderer.GetCenterSprite().index < world.sprites.Length)
                return world.sprites[renderer.GetCenterSprite().index];
            return null;
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write((int) world.clock.Elapsed.TotalSeconds);
            weapon.DoParseSave(ref w);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            world.startingSec = r.ReadInt32();
            weapon.DoParseLoad(ref r);
        }
    }
}