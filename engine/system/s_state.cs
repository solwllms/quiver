using System;
using System.Collections.Generic;

namespace engine.system
{
    public class Statemanager
    {
        public static Stack<IState> history = new Stack<IState>();
        public static IState current;

        public static bool Wasgame => history.Peek()?.GetType() == typeof(states.Game);

        public static bool Isgame => history.Peek()?.GetType() == typeof(states.Game);

        public static void SetState(IState s, bool resethistory = false)
        {
            if (!resethistory)
                history.Push(current);
            else
                history.Clear();
            current = s;
            current.Init();
        }

        public static void GoBack()
        {
            // no longer forcefully disposing previous
            //current.Dispose();
            if(history.Count > 0)
                current = history.Pop();
            //current.init();
        }

        public static void ClearHistory()
        {
            foreach (var s in history) s.Dispose();
            history.Clear();
        }
    }

    public interface IState : IDisposable
    {
        void Init();
        void Update();
        void Render();
    }
}