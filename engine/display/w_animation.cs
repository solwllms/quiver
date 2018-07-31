namespace engine.display
{
    public class Animation
    {
        public string texture = "";
        public int framesPerLine = 8;
        public int currrent = 0;

        private int start = 0;
        private int end = 0;

        public bool playing = false;
        public bool loop = false;

        public Animation(int start, int end, bool loop)
        {
            this.start = start;
            this.end = end;
            currrent = start;

            this.loop = loop;
        }

        public Animation(string texture, int start, int end, bool loop)
        {
            this.texture = texture;
            this.start = start;
            this.end = end;
            currrent = start;

            this.loop = loop;
        }

        public void Play()
        {
            playing = true;
            currrent = start;
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

            if (currrent == end)
            {
                currrent = start;
                if (loop != true) Stop();
            }
            else currrent++;
        }

        public bool IsDone()
        {
            return currrent > end && loop != true;
        }
    }
}
