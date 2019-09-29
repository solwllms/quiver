#region

using Quiver.system;
using System.Drawing;

#endregion

namespace Quiver.game.types
{
    public delegate void mapEvent(mapcell cell);

    public class mapcell
    {
        public byte ceiltex;

        //public Color color;
        public Color emission = Color.Black;
        public byte floortex;
        public bool interactable;

        public string onInteract;
        public string onShot;
        public string onTouch;
        public string onWalk;

        public vector pos;

        public string shootex;
        public bool solid;

        public bool wall;
        public byte walltex;

        public mapcell(vector position) : this(position, "textures/floor", "textures/floor", "textures/floor")
        {
        }

        public mapcell(vector position, string walltex, string floortex, string ceiltex)
        {
            pos = position;

            SetWalltex(walltex);
            SetFloortex(floortex);
            SetCeiltex(ceiltex);
        }

        public void SetWalltex(string t)
        {
            walltex = world.GetTextureId(t);
        }

        public void SetFloortex(string t)
        {
            floortex = world.GetTextureId(t);
        }

        public void SetCeiltex(string t)
        {
            ceiltex = world.GetTextureId(t);
        }

        public void OnShot()
        {
            if (!string.IsNullOrEmpty(shootex))
                SetWalltex(shootex);

            if (!string.IsNullOrEmpty(onShot))
                progs.CallMapEvent(onShot, this);
        }
    }
}