using engine.system;
using SFML.Graphics;

namespace engine.display
{
    public class Texture : Image
    {
        public Texture(string name) : base(Filesystem.Open(name + ".png", tex: true))
        {
        }

        public Texture(uint width, uint height) : base(width, height)
        {
        }

        public void Draw(uint x, uint y, uint sx, uint sy, uint rw, uint ry)
        {
            for (uint tx = 0; tx < rw; tx++)
            for (uint ty = 0; ty < ry; ty++)
            {
                var c = GetPixel(sx + tx, sy + ty);
                if (c != Renderer.magicpink && x + tx < Screen.width && y + ty < Screen.height)
                    Screen.SetPixel(x + tx, y + ty, c);
            }
        }

        public void DrawStencil(uint x, uint y, uint sx, uint sy, uint rw, uint ry, Color c)
        {
            for (uint tx = 0; tx < rw; tx++)
            for (uint ty = 0; ty < ry; ty++)
                if (GetPixel(sx + tx, sy + ty) != Renderer.magicpink)
                    Screen.SetPixel(x + tx, y + ty, c);
        }

        public void Draw(uint x, uint y)
        {
            Draw(x, y, 0, 0, Size.X, Size.Y);
        }
    }
}