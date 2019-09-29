namespace Quiver.display
{
    public class animation
    {
        private readonly int _end;

        private readonly int _start;
        public int currrent;
        public int framesPerLine = 8;
        public bool loop;

        public bool playing;
        public string texture = "";

        public animation(int start, int end, bool loop)
        {
            this._start = start;
            this._end = end;
            currrent = start;

            this.loop = loop;
        }

        public animation(string texture, int start, int end, bool loop)
        {
            this.texture = texture;
            this._start = start;
            this._end = end;
            currrent = start;

            this.loop = loop;
        }

        public void Play()
        {
            playing = true;
            currrent = _start;
        }

        public void Resume()
        {
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void Step()
        {
            if (!playing) return;

            if (currrent == _end)
            {
                currrent = _start;
                if (loop != true) Stop();
            }
            else
            {
                currrent++;
            }
        }

        public bool IsDone()
        {
            return currrent > _end && loop != true;
        }
    }
}