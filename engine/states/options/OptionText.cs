#region

using Quiver.display;
using System.Drawing;

#endregion

namespace Quiver.states.options
{
    public class optionText : optionListing
    {
        private readonly string _text;
        public Color col = Color.Black;

        public optionText(string text) : base("")
        {
            _text = text;
            selectable = false;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            gui.Write(_text, x, y, col);
        }

        public override void Tick()
        {
        }
    }
}