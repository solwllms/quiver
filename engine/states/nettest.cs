using Quiver.Network;
using OpenTK.Input;
using Quiver.Audio;
using Quiver.display;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiver.system
{
    class nettest : IState
    {
        private int state = -1;
        private int _dots = 0;

        void IState.Init()
        {
            audio.ClearSounds();
        }

        void IState.Render()
        {
            screen.Clear();
            gui.Write("network test", 0, 0, Color.Gray);
            if (state == -1)
            {
                gui.Write("Q - host", 0, 8, Color.White);
                gui.Write("W - connect", 0, 16, Color.White);
            }
            else if (state == 0)
            {
                if (state == 0)
                    gui.Write("server running", 0, 8, Color.Green);
                else if (state == 1)
                    gui.Write("connected", 0, 8, Color.Green);
                gui.Write("ESC - stop", 0, 16, Color.White);
            }

            DrawDots();
        }

        void DrawDots()
        {
            if (_dots >= 2) screen.SetPixel(149, 82, Color.White);
            if (_dots >= 1) screen.SetPixel(147, 82, Color.White);
            if (_dots >= 0) screen.SetPixel(145, 82, Color.White);
        }

        void IState.Update()
        {
            if (engine.frame % 30 == 0) _dots = (_dots + 1) % 3;
            if (engine.frame % 15 == 0)
            {
                if (state == 0) n_server.Poll();
                else if (state == 1) n_client.Poll();
            }

            if (state == -1)
            {
                if (input.IsKeyPressed(Key.Q))
                {
                    game.game.HostNewGame("maps/E1M1.lvl");
                    //n_server.Start();
                    state = 0;
                }
                else if (input.IsKeyPressed(Key.W))
                {
                    game.game.ConnectToGame("localhost");
                    //n_client.Connect();
                    state = 1;
                }
            }
            else
            {
                if (input.IsKeyPressed(Key.Escape))
                {
                    if (state == 0) n_server.Stop();
                    else if (state == 1) n_client.Disconnect();
                    state = -1;
                }
            }
        }

        void IDisposable.Dispose()
        {

        }

        void IState.Focus()
        {

        }
    }
}
