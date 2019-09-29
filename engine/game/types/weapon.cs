#region

using System.IO;
using Quiver.Audio;
using Quiver.display;
using Quiver.system;

#endregion

namespace Quiver.game.types
{
    public class weapon : saveable
    {
        private readonly byte _damage;

        private readonly animation _fireAnim;
        private readonly int _reloadcd;

        private readonly int _shootcd;
        private animation _anim;
        private int _cooldown;
        private string _sfxEmpty;
        private string _sfxReload;

        private string _sfxShoot;
        public int clip;
        public int maxclip;
        public int nonclip;
        public int reloadMsgTime = -1;

        public weapon(animation fireAnim, int maxclip, int totalammo, byte damage, int shootcd, int reloadcd,
            string sfxShoot,
            string sfxReload, string sfxEmpty)
        {
            _fireAnim = fireAnim;
            _anim = fireAnim;
            _sfxShoot = sfxShoot;
            _sfxReload = sfxReload;
            _sfxEmpty = sfxEmpty;

            _damage = damage;

            _shootcd = shootcd;
            _reloadcd = reloadcd;

            this.maxclip = maxclip;
            nonclip = totalammo;
            Reload();
        }

        public void SetAnimation(animation anim)
        {
            _anim = anim;
        }

        public animation GetAnimation()
        {
            return _anim;
        }

        public virtual void Tick()
        {
            if (engine.frame % 5 == 0) _anim?.Step();

            _cooldown = (_cooldown - 1).Clamp(0, 200);
            if (reloadMsgTime != -1) reloadMsgTime = (reloadMsgTime - 1).Clamp(-1, 200);
        }

        public int GetAnimFrame()
        {
            if (_anim == null) return -1;
            return _anim.currrent;
        }

        public virtual void Reload()
        {
            if (nonclip == 0)
            {
                audio.PlaySound("sound/revolver/empty", 70);
                return;
            }

            audio.PlaySound("sound/revolver/reload", 70);
            //if(doGui) reloadMsgTime = 20;
            _cooldown = _reloadcd;
            nonclip = (nonclip - (maxclip - clip)).Clamp(0, 255);
            if (nonclip > maxclip)
                clip = maxclip;
        }

        public virtual void Fire()
        {
            if (_cooldown == 0)
            {
                if (clip == 0)
                {
                    Reload();
                    return;
                }

                _anim = _fireAnim;

                _cooldown = _shootcd;
                audio.PlaySound("sound/revolver/shoot", 70);
                clip = (clip - 1).Clamp(0, 255);
                _anim = _fireAnim;
                _anim.Play();

                var e = world.Player.GetLookatEntity();
                if (e != null && e.GetState() == livestate.Alive) e.DoDamage(_damage);

                var c = renderer.GetCenterMapCell();
                if(c != null) c.OnShot();
            }
        }

        public virtual void Draw(vector velocity)
        {
        }

        public void DrawViewmodel(texture t, uint x, uint y, uint sx, uint sy, uint rw, uint ry)
        {
            t.Draw(x, y, sx, sy, rw, ry,
                level.lightmap[(int) (world.Player.pos.x * renderer.TEXSIZE),
                    (int) (world.Player.pos.y * renderer.TEXSIZE)]);
        }

        public override void DoParseSave(ref BinaryWriter w)
        {
            base.DoParseSave(ref w);
            w.Write(_cooldown);
            w.Write(clip);
            w.Write(nonclip);
        }

        public override void DoParseLoad(ref BinaryReader r)
        {
            base.DoParseLoad(ref r);
            _cooldown = r.ReadInt32();
            clip = r.ReadInt32();
            nonclip = r.ReadInt32();
        }
    }
}