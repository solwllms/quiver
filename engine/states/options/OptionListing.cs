#region

using Quiver.display;

#endregion

namespace Quiver.states.options
{
    public class optionListing
    {
        public string label;
        public bool selectable = true;

        public optionListing(string label)
        {
            this.label = label;
        }

        public virtual void Draw(bool hover, uint x, uint y)
        {
            gui.Draw(x - 1, y, 119, 7, gui.back);
            gui.Write(label, x, y, hover ? gui.lighter : gui.darker);
        }

        public virtual void Tick()
        {
        }
    }
}