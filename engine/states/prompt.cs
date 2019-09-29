#region

using System.Drawing;
using System.IO;
using Quiver.display;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver.states
{
    public delegate void promptok();

    public delegate void promptsubmit(string input);

    public class prompt : IState
    {
        private readonly promptok _action;
        private readonly bool _hasno = false;
        private readonly promptok _actionno;
        private readonly bool _ispath = false;
        private readonly promptsubmit _submit;

        private readonly bool _textinput;
        private readonly string _title;
        private uint[,] _background;
        private uint _h;
        private string _message;
        private uint _w;

        private uint _x;
        private uint _y;

        public bool cursor;

        public prompt(string title, string message, promptok action)
        {
            _title = title;
            _message = message;
            _action = action;
            _textinput = false;
            cursor = false;
        }

        public prompt(string title, string message, promptok action, promptok actionno)
        {
            _title = title;
            _message = message;
            _action = action;
            _actionno = actionno;
            _hasno = true;
            _textinput = false;
            cursor = false;
        }

        public prompt(string title, string message, promptsubmit submit)
        {
            _title = title;
            _message = message;
            _submit = submit;
            _textinput = true;
            cursor = false;
        }

        void IState.Focus()
        {
        }

        void IState.Init()
        {
            _w = screen.width - 50;
            _h = screen.height / 3;
            _x = screen.width / 2 - _w / 2;
            _y = screen.height / 2 - _h / 2;

            _background = new uint[screen.width, screen.height];
            for (uint i = 0; i < screen.width * screen.height; i++)
            {
                var x = i % screen.width;
                var y = i / screen.width;
                _background[x, y] = (screen.GetPixel(x, y).ToUint() >> 1) & 8355711;
            }
        }

        void IState.Render()
        {
            for (uint i = 0; i < screen.width * screen.height; i++)
            {
                var x = i % screen.width;
                var y = i / screen.width;
                screen.SetPixel(x, y, _background[x, y]);
            }

            if (cursor)
                cache.GetTexture("gui/prompt").Draw(_x, _y, 0, 0, 110, 32);
            else
                cache.GetTexture("gui/prompt").Draw(_x, _y, 0, 32, 110, 32);

            Printctr(_title, 1, Color.White);

            if (!_textinput)
            {
                Printctr(_message, 12, Color.White);
            }
            else
            {
                gui.Draw(_x + 22, _y + 11, 66, 9, Color.White);
                Printctr(_message, 12, Color.Black);
            }
        }

        void IState.Update()
        {
            if (input.IsKeyPressed(Key.Escape))
                Doback();

            if (input.IsKeyPressed(Key.Right))
                cursor = false;
            if (input.IsKeyPressed(Key.Left))
                cursor = true;

            if (!_textinput)
            {
                if (input.IsKeyPressed(Key.Y))
                    Doyes();
                if (input.IsKeyPressed(Key.N))
                    Doback();
            }

            if (input.IsKeyPressed(Key.Enter))
            {
                if (cursor && _message.Length > 0)
                    Doyes();
                else
                    Doback();
            }

            if (input.IsKeyPressed(Key.BackSpace) && _message.Length > 0)
                _message = _message.Remove(_message.Length - 1);

            if (_textinput && _message.Length < 16 && (!_ispath || !input.inputstring.Contains(" ") &&
                                                       input.inputstring.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
                _message += input.inputstring;
        }

        public void Dispose()
        {
            /*
            _title = null;
            _message = null;
            _action = null;
            _background = null;
            */
        }

        public void Printctr(string s, uint ty, Color c)
        {
            gui.Write(s, (uint) (_x + (_w / 2 - s.Length * 4 / 2)), _y + ty, c);
        }

        public void Print(uint tx, uint ty, string s)
        {
            gui.Write(s, _x + tx, _y + ty);
        }

        private void Doyes()
        {
            if (!_textinput)
            {
                _action.Invoke();
                statemanager.GoBack();
            }
            else
            {
                _submit.Invoke(_message);
            }
        }

        private void Doback()
        {
            if (_hasno) _actionno.Invoke();
            else statemanager.GoBack();
        }
    }
}