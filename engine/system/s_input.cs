using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace engine.system
{
    public partial class Input
    {
        private static List<Keyboard.Key> _wasDown;
        public static string inputstring;
        public static Vector mousepos;

        public static bool mouselock = true;
        protected static bool mouselocked = true;

        public static void Init()
        {
            _wasDown = new List<Keyboard.Key>();
            inputstring = "";
            mousepos = new Vector(0, 0);
        }

        public static void Update()
        {
            var tempdown = new List<Keyboard.Key>();

            inputstring = "";
            foreach (Keyboard.Key key in Enum.GetValues(typeof(Keyboard.Key)))
            {
                if (IsKeyPressed(key) && Engine.HasFocus())
                {
                    var v = (int)key;

                    if (key == Keyboard.Key.Space)
                        inputstring += " ";

                    if (key == Keyboard.Key.Quote)
                        inputstring += '"';
                    if (key == Keyboard.Key.LBracket)
                        inputstring += '(';
                    if (key == Keyboard.Key.RBracket)
                        inputstring += ')';
                    if (key == Keyboard.Key.Dash)
                        inputstring += '_';

                    if (v < 26)
                        inputstring += (char) (v + 65);
                    if (v >= 26 && v <= 35)
                        inputstring += (char) (v + 22);
                }

                if (Engine.IsKeyPressed(key)) tempdown.Add(key);
            }

            var mp = GetMousePos();
            mousepos = new Vector((int) Engine.windowWidth / 2 - mp.x, (int) Engine.windowHeight / 2 - mp.y);

            if (mouselocked && !mouselock) mouselocked = false;
            if (!mouselocked && mouselock && Engine.HasFocus()) mouselocked = mouselock;

            if (mouselocked)
            {
                SetMousePos(new Vector((int)Engine.windowWidth / 2, (int)Engine.windowHeight / 2));
            }
            SetMouseVisible(!mouselocked);

            _wasDown = tempdown;
        }

        public static void MouseReturn()
        {
            mouselocked = mouselock;
        }
        public static void MouseLost()
        {
            mouselocked = false;
        }
        public static bool MouseLocked()
        {
            return mouselocked;
        }

        public static bool IsKeyPressed(Keyboard.Key key)
        {
            return !_wasDown.Contains(key) && Engine.IsKeyPressed(key);
        }

        public static bool IsKey(Keyboard.Key key)
        {
            return Engine.IsKeyPressed(key);
        }

        public static bool IsKeyReleased(Keyboard.Key key)
        {
            return _wasDown.Contains(key) && !Engine.IsKeyPressed(key);
        }

        public static string GetKeyName(Keyboard.Key key)
        {
            return key.ToString();
        }

        public static bool FindKey(string s, out Keyboard.Key k)
        {
            try
            {
                k = (Keyboard.Key) Enum.Parse(typeof(Keyboard.Key), Enum.GetNames(typeof(Keyboard.Key)).First(x => x.ToLower() == s.ToLower()));
                return true;
            }
            catch
            {
                k = Keyboard.Key.Unknown;
                return false;
            }
        }

        public static void SetMouseVisible(bool v)
        {
            Engine.window.SetMouseCursorVisible(v);
        }
        public static void SetMousePos(Vector vector)
        {
            SFML.System.Vector2i t = new SFML.System.Vector2i((int)vector.x, (int)vector.y);
            Mouse.SetPosition(t, Engine.window);
        }
        public static Vector GetMousePos()
        {
            var m = Mouse.GetPosition(Engine.window);
            return new Vector(m.X, m.Y);
        }
    }
}