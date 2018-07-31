using engine.display;
using SFML.Graphics;

namespace engine.states.options
{
    public class OptionText : OptionListing
    {
        private readonly string _text;
        public Color col = Color.Black;

        public OptionText(string text) : base("")
        {
            this._text = text;
            selectable = false;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
            Gui.Write(_text, x, y, col);
        }

        public override void Tick()
        {
        }
    }
}