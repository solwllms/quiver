#region

using System;
using System.Collections.Generic;

#endregion

namespace Quiver.system
{
    public class statemanager
    {
        public static Stack<IState> history = new Stack<IState>();
        public static IState current;

        public static bool Wasgame => history.Peek()?.GetType() == typeof(states.game);

        public static bool Isgame => history.Peek()?.GetType() == typeof(states.game);

        public static void SetState(IState s, bool resethistory = false)
        {
            if (!resethistory)
                history.Push(current);
            else
                history.Clear();
            current = s;
            current.Init();
            current.Focus();
        }

        public static void GoBack()
        {
            // no longer forcefully disposing previous
            //current.Dispose();
            if (history.Count > 0)
                current = history.Pop();
            current.Focus();
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
        void Focus();
        void Update();
        void Render();
    }
}