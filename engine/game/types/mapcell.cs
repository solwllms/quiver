using engine.display;
using engine.progs;
using engine.system;
using SFML.Graphics;

namespace engine.game.types
{
    public delegate void MapEvent(Mapcell cell);

    public class Mapcell
    {
        public byte ceiltex;
        public Color color;
        public byte floortex;
        public bool interactable;

        public string onInteract;
        public string onTouch;
        public string onWalk;
        public string onShot;

        public Vector pos;
        public bool solid;

        public bool wall;

        public string shootex;
        public byte walltex;

        public Mapcell(Vector position) : this(position, "textures/floor", "textures/floor", "textures/floor")
        {
        }

        public Mapcell(Vector position, string walltex, string floortex, string ceiltex)
        {
            pos = position;

            SetWalltex(walltex);
            SetFloortex(floortex);
            SetCeiltex(ceiltex);

            color = Color.White;
        }

        public void SetWalltex(string t)
        {
            walltex = World.GetTextureId(t);
        }

        public void SetFloortex(string t)
        {
            floortex = World.GetTextureId(t);
        }

        public void SetCeiltex(string t)
        {
            ceiltex = World.GetTextureId(t);
        }

        public void OnShot()
        {
            if (!string.IsNullOrEmpty(shootex))
                SetWalltex(shootex);

            if (!string.IsNullOrEmpty(onShot))
                Progs.CallMapEvent(onShot, this);
        }
    }
}