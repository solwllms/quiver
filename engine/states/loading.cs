using System;
using System.Drawing;
using Quiver.display;
using Quiver.game;
using Quiver.system;

namespace Quiver.States
{
    class loading : IState
    {
        private bool _fresh;

        private string[] _lines;
        private string _msg = "";
        private int _dots = 0;

        public loading(bool fresh)
        {
            this._fresh = fresh;

        }
        
        void IState.Init()
        {
            _lines = gui.GetTruncatedLines(lang.Get("$loading.tip" + engine.random.Next(0, 9)), 22);
        }

        void IState.Render()
        {
            cache.GetTexture("gui/background2").Draw(0, 0);

            _msg = lang.Get("$loading.loading") + " " + level.name;
            gui.Write(_msg, (uint) (screen.width - 10 - (_msg.Length * 4)), 8, gui.lighter);

            gui.Write(lang.Get("$loading.tip"), 10, (screen.height / 2) - 7, Color.White);
            for (int i = 0; i < _lines.Length; i++)
                gui.Write(_lines[i], 10, (screen.height / 2) + (uint)(i * 7), gui.lighter);

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

            if(level.doneLoading) statemanager.SetState(new states.game_state(_fresh));
        }

        void IDisposable.Dispose()
        {

        }

        void IState.Focus()
        {

        }
    }
}
