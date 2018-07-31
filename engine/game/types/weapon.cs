using System.IO;
using engine.display;
using engine.system;

namespace engine.game.types
{
    public class Weapon : Saveable
    {
        public int clip;
        private int _cooldown;

        private Animation _fireAnim;
        private Animation _anim;

        private readonly byte _damage;
        public int maxclip;
        public int nonclip;
        private readonly int _reloadcd;
        public int reloadMsgTime = -1;
        private string _sfxEmpty;
        private string _sfxReload;

        private string _sfxShoot;

        private readonly int _shootcd;

        public Weapon(Animation fireAnim, int maxclip, int totalammo, byte damage, int shootcd, int reloadcd, string sfxShoot,
            string sfxReload, string sfxEmpty)
        {
            _fireAnim = fireAnim;
            _anim = fireAnim;
            this._sfxShoot = sfxShoot;
            this._sfxReload = sfxReload;
            this._sfxEmpty = sfxEmpty;

            this._damage = damage;

            this._shootcd = shootcd;
            this._reloadcd = reloadcd;

            this.maxclip = maxclip;
            nonclip = totalammo;
            Reload();
        }

        public void SetAnimation(Animation anim)
        {
            this._anim = anim;
        }
        public Animation GetAnimation()
        {
            return _anim;
        }

        public virtual void Tick()
        {
            if (Engine.frame % 5 == 0) _anim.Step();

            _cooldown = (_cooldown - 1).Clamp(0, 200);
            if (reloadMsgTime != -1) reloadMsgTime = (reloadMsgTime - 1).Clamp(-1, 200);
        }

        public int GetAnimFrame()
        {
            return _anim.currrent;
        }

        public virtual void Reload()
        {
            if (nonclip == 0)
            {
                Audio.PlaySound2D("sound/revolver/empty", 70);
                return;
            }

            Audio.PlaySound2D("sound/revolver/reload", 70);
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
                Audio.PlaySound2D("sound/revolver/shoot", 70);
                clip = (clip - 1).Clamp(0, 255);
                _anim = _fireAnim;
                _anim.Play();

                var e = World.Player.GetLookatEntity();
                if (e != null && e.GetState() == Livestate.Alive) e.DoDamage(_damage);

                var c = Renderer.GetLookedAt();
                c?.OnShot();
            }
        }

        public virtual void Draw(Vector velocity)
        { }

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