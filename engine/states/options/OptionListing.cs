using engine.display;
using SFML.Graphics;

namespace engine.states.options
{
    public class OptionListing
    {
        public string label;
        public bool selectable = true;

        public OptionListing(string label)
        {
            this.label = label;
        }

        public virtual void Draw(bool hover, uint x, uint y)
        {
            Gui.Draw(x - 1, y, 119, 7, Gui.darker);
            Gui.Write(label, x, y, hover ? Color.White : Gui.lighter);
        }

        public virtual void Tick()
        {
        }
    }
}