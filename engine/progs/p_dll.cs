using engine.game;
using engine.system;

namespace engine.progs
{
    public class Dll
    {
        public string desc;
        public string dev;
        public string title;
        public string version;

        public Dll(string title, string dev, string version, string desc)
        {
            this.title = title;
            this.dev = dev;
            this.version = version;
            this.desc = desc;
        }

        public virtual void Init()
        {
        }

        public virtual void Shutdown()
        {
        }

        public virtual IState GetMenuState()
        {
            return null;
        }

        public virtual IState GetInitialState()
        {
            return null;
        }

        public virtual Gmbase GetGamemode()
        {
            return null;
        }
    }
}