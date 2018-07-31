using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.system
{
    public partial class Input
    {
        public enum Key
        {
            Unknown = -1,
            A = 0,
            B = 1,
            C = 2,
            D = 3,
            E = 4,
            F = 5,
            G = 6,
            H = 7,
            I = 8,
            J = 9,
            K = 10, // 0x0000000A
            L = 11, // 0x0000000B
            M = 12, // 0x0000000C
            N = 13, // 0x0000000D
            O = 14, // 0x0000000E
            P = 15, // 0x0000000F
            Q = 16, // 0x00000010
            R = 17, // 0x00000011
            S = 18, // 0x00000012
            T = 19, // 0x00000013
            U = 20, // 0x00000014
            V = 21, // 0x00000015
            W = 22, // 0x00000016
            X = 23, // 0x00000017
            Y = 24, // 0x00000018
            Z = 25, // 0x00000019
            Num0 = 26, // 0x0000001A
            Num1 = 27, // 0x0000001B
            Num2 = 28, // 0x0000001C
            Num3 = 29, // 0x0000001D
            Num4 = 30, // 0x0000001E
            Num5 = 31, // 0x0000001F
            Num6 = 32, // 0x00000020
            Num7 = 33, // 0x00000021
            Num8 = 34, // 0x00000022
            Num9 = 35, // 0x00000023
            Escape = 36, // 0x00000024
            LControl = 37, // 0x00000025
            LShift = 38, // 0x00000026
            LAlt = 39, // 0x00000027
            LSystem = 40, // 0x00000028
            RControl = 41, // 0x00000029
            RShift = 42, // 0x0000002A
            RAlt = 43, // 0x0000002B
            RSystem = 44, // 0x0000002C
            Menu = 45, // 0x0000002D
            LBracket = 46, // 0x0000002E
            RBracket = 47, // 0x0000002F
            SemiColon = 48, // 0x00000030
            Comma = 49, // 0x00000031
            Period = 50, // 0x00000032
            Quote = 51, // 0x00000033
            Slash = 52, // 0x00000034
            BackSlash = 53, // 0x00000035
            Tilde = 54, // 0x00000036
            Equal = 55, // 0x00000037
            Dash = 56, // 0x00000038
            Space = 57, // 0x00000039
            Return = 58, // 0x0000003A
            BackSpace = 59, // 0x0000003B
            Tab = 60, // 0x0000003C
            PageUp = 61, // 0x0000003D
            PageDown = 62, // 0x0000003E
            End = 63, // 0x0000003F
            Home = 64, // 0x00000040
            Insert = 65, // 0x00000041
            Delete = 66, // 0x00000042
            Add = 67, // 0x00000043
            Subtract = 68, // 0x00000044
            Multiply = 69, // 0x00000045
            Divide = 70, // 0x00000046
            Left = 71, // 0x00000047
            Right = 72, // 0x00000048
            Up = 73, // 0x00000049
            Down = 74, // 0x0000004A
            Numpad0 = 75, // 0x0000004B
            Numpad1 = 76, // 0x0000004C
            Numpad2 = 77, // 0x0000004D
            Numpad3 = 78, // 0x0000004E
            Numpad4 = 79, // 0x0000004F
            Numpad5 = 80, // 0x00000050
            Numpad6 = 81, // 0x00000051
            Numpad7 = 82, // 0x00000052
            Numpad8 = 83, // 0x00000053
            Numpad9 = 84, // 0x00000054
            F1 = 85, // 0x00000055
            F2 = 86, // 0x00000056
            F3 = 87, // 0x00000057
            F4 = 88, // 0x00000058
            F5 = 89, // 0x00000059
            F6 = 90, // 0x0000005A
            F7 = 91, // 0x0000005B
            F8 = 92, // 0x0000005C
            F9 = 93, // 0x0000005D
            F10 = 94, // 0x0000005E
            F11 = 95, // 0x0000005F
            F12 = 96, // 0x00000060
            F13 = 97, // 0x00000061
            F14 = 98, // 0x00000062
            F15 = 99, // 0x00000063
            Pause = 100, // 0x00000064
            KeyCount = 101, // 0x00000065
        }
    }
}
