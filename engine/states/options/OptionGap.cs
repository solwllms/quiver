namespace Quiver.states.options
{
    public class optionGap : optionListing
    {
        public optionGap() : base("")
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