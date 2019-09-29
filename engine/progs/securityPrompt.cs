using System;
using System.Drawing;
using System.Reflection;
using Quiver.display;
using Quiver.game;
using Quiver.states;
using Quiver.system;

namespace Quiver.States
{
    class securityPrompt : IState
    {
        AssemblyName[] s;
        int i = 0;

        Action ok;
        Assembly assembly;
        dll d;

        public securityPrompt(Action ok, ref Assembly assembly, ref dll d)
        {
            this.ok = ok;
            this.assembly = assembly;
            s = assembly.GetReferencedAssemblies();
            this.d = d;

            i = -1;
        }

        void IState.Init()
        {

        }
        

        void IState.Render()
        {
            screen.Clear();
            PrintPrompt();
        }

        void IState.Update()
        {
            if (input.IsKeyPressed(OpenTK.Input.Key.Y))
            {
                if (i == s.Length - 1) ok.Invoke();
                else i++;
            }
            if (i == -1 && input.IsKeyPressed(OpenTK.Input.Key.S))
            {
                ok.Invoke();
            }
            if (input.IsKeyPressed(OpenTK.Input.Key.N))
            {
                engine.Exit();
            }
        }

        void PrintPrompt()
        {
            if (i == -1)
            {
                gui.WriteCentered("game modified", 10, Color.White);

                gui.WriteCentered("continue only if you are aware", 23, Color.Yellow);
                gui.WriteCentered("and trust the author!", 30, Color.Yellow);

                gui.WriteCentered("[Y] CONTINUE (EXPERT)", 59, Color.DarkGray);
                gui.WriteCentered("[S] SKIP (RISKY)", 66, Color.DarkGray);
                gui.WriteCentered("[N] CANCEL", 73, Color.White);
            }
            else
            {
                gui.WriteCentered("game requires '" + s[i].Name + "'", 10, Color.Yellow);

                gui.WriteCentered("continue loading?", 23, Color.White);
                gui.WriteCentered("select no if worried as", 30, Color.Red);
                gui.WriteCentered("it can do damage!", 37, Color.Red);

                gui.WriteCentered("[Y] YES [N] NO", 73, Color.White);
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
