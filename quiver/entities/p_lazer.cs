using engine.game.types;
using engine.system;
using SFML.Audio;
using SFML.System;

namespace game.entities
{
    internal class p_Lazer : Projectile
    {
        private Sound s;
        
        public p_Lazer(Vector pos, Vector dir) : base("sprites/lazer", "", pos, dir, 50, 6, 9)
        {
            s = Audio.PlaySound3D("sound/robo/lazer_move", pos, 2);
        }

        public override void Tick()
        {
            base.Tick();
            try
            {
                if (s != null) s.Position = new Vector3f(pos.x, 0, pos.y);
            }
            catch { }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            s.Stop();
            s = null;
        }
    }
}