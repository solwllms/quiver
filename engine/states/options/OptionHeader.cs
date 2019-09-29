#region

using Quiver.display;
using System.Drawing;

#endregion

namespace Quiver.states.options
{
    public class optionHeader : optionListing
    {
        public optionHeader(string label) : base(label)
        {
            selectable = false;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            gui.Write(label, x + 4, y, Color.White);
        }

        public override void Tick()
        {
        }
    }
}