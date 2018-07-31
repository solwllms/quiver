namespace engine.states.options
{
    public class OptionGap : OptionListing
    {
        public OptionGap() : base("")
        {
            selectable = false;
        }

        public override void Draw(bool hover, uint x, uint y)
        {
        }

        public override void Tick()
        {
        }
    }
}