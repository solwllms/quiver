using engine.display;

namespace engine.states.options
{
    public class OptionHeader : OptionListing
    {
        public OptionHeader(string label) : base(label)
        {
            selectable = false;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            Gui.Write(label, x + 4, y, Gui.darker);
        }

        public override void Tick()
        {
        }
    }
}