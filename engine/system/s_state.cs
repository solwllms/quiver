#region

using Quiver.display;
using System;
using System.Collections.Generic;

#endregion

namespace Quiver.system
{
    public class statemanager
    {
        public static Stack<IState> history = new Stack<IState>();
        private static IState current;

        private static transition transition;

        public static bool Wasgame => history.Peek()?.GetType() == typeof(states.game_state);

        public static bool Isgame => history.Peek()?.GetType() == typeof(states.game_state);

        /*
            State logic
        */
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
        public static Type GetCurrentType()
        {
            return current.GetType();
        }

        /*
            History logic
        */

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

        /*
            State event logic
        */

        public static void CurrentInit() {
            current.Init();
        }
        public static void CurrentFocus() {
            current.Focus();
        }
        public static void CurrentUpdate() {
            if (transition != null)
            {
                transition.Tick();
                if (transition.IsDone()) transition = null;
            }

            current.Update();
        }
        public static void CurrentRender() {
            current.Render();
            if (transition != null) transition.Draw();
        }

        /*
            Transition logic
        */
        public static void SetTransition(transition t)
        {
            transition = t;
        }
        public static void ClearTransition(transition t)
        {
            transition = null;
        }
        public static bool IsTransitionFairlyDone()
        {
            return transition == null || transition.IsFairlyDone();
        }
        public static bool IsTransitionDone()
        {
            return transition == null || transition.IsDone();
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