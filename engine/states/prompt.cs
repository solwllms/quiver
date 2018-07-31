using System.IO;
using engine.display;
using engine.system;
using SFML.Graphics;
using SFML.Window;

namespace engine.states
{
    public delegate void Promptok();

    public delegate void Promptsubmit(string input);

    public class Prompt : IState
    {
        private Promptok _action;
        private uint[,] _background;

        public bool cursor;
        private uint _h;

        private readonly bool _ispath = false;
        private string _message;
        private readonly Promptsubmit _submit;

        private readonly bool _textinput;
        private string _title;
        private uint _w;

        private uint _x;
        private uint _y;

        public Prompt(string title, string message, Promptok action)
        {
            this._title = title;
            this._message = message;
            this._action = action;
            _textinput = false;
            cursor = false;
        }

        public Prompt(string title, string message, Promptsubmit submit)
        {
            this._title = title;
            this._message = message;
            this._submit = submit;
            _textinput = true;
            cursor = false;
        }

        void IState.Init()
        {
            _w = Screen.width - 50;
            _h = Screen.height / 3;
            _x = Screen.width / 2 - _w / 2;
            _y = Screen.height / 2 - _h / 2;

            _background = new uint[Screen.width, Screen.height];
            for (uint i = 0; i < Screen.width * Screen.height; i++)
            {
                var x = i % Screen.width;
                var y = i / Screen.width;
                _background[x, y] = (Screen.GetPixel(x, y) >> 1) & 8355711;
            }
        }

        void IState.Render()
        {
            for (uint i = 0; i < Screen.width * Screen.height; i++)
            {
                var x = i % Screen.width;
                var y = i / Screen.width;
                Screen.SetPixel(x, y, _background[x, y]);
            }

            if (cursor)
                Cache.GetTexture("gui/prompt").Draw(_x, _y, 0, 0, 110, 32);
            else
                Cache.GetTexture("gui/prompt").Draw(_x, _y, 0, 32, 110, 32);

            Printctr(_title, 1, Color.White);

            if (!_textinput)
            {
                Printctr(_message, 12, Color.White);
            }
            else
            {
                Gui.Draw(_x + 22, _y + 11, 66, 9, Color.White);
                Printctr(_message, 12, Color.Black);
            }
        }

        void IState.Update()
        {
            if (Input.IsKeyPressed(Keyboard.Key.Escape))
                Doback();

            if (Input.IsKeyPressed(Keyboard.Key.Right))
                cursor = false;
            if (Input.IsKeyPressed(Keyboard.Key.Left))
                cursor = true;

            if (!_textinput)
            {
                if (Input.IsKeyPressed(Keyboard.Key.Y))
                    Doyes();
                if (Input.IsKeyPressed(Keyboard.Key.N))
                    Doback();
            }

            if (Input.IsKeyPressed(Keyboard.Key.Return))
            {
                if (cursor && _message.Length > 0)
                    Doyes();
                else
                    Doback();
            }

            if (Input.IsKeyPressed(Keyboard.Key.BackSpace) && _message.Length > 0)
                _message = _message.Remove(_message.Length - 1);

            if (_textinput && _message.Length < 16 && (!_ispath || !Input.inputstring.Contains(" ") &&
                                                     Input.inputstring.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
                _message += Input.inputstring;
        }

        public void Dispose()
        {
            _title = null;
            _message = null;
            _action = null;
            _background = null;
        }

        public void Printctr(string s, uint ty, Color c)
        {
            Gui.Write(s, (uint) (_x + (_w / 2 - s.Length * 4 / 2)), _y + ty, c);
        }

        public void Print(uint tx, uint ty, string s)
        {
            Gui.Write(s, _x + tx, _y + ty);
        }

        private void Doyes()
        {
            if (!_textinput)
            {
                _action.Invoke();
                Statemanager.GoBack();
            }
            else
            {
                _submit.Invoke(_message);
            }
        }

        private void Doback()
        {
            Statemanager.GoBack();
        }
    }
}