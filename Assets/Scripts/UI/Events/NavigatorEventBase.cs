using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DigDig2.UIElements
{
    public abstract class NavigatorEventBase<T> : EventBase<T>, INavigatorEvent where T : NavigatorEventBase<T>, new()
    {
        public string nodeName;
        public Dictionary<string, Dictionary<string, string>> arguments;
        
        protected NavigatorEventBase()
        {
            LocalInit();
        }

        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        private void LocalInit()
        {
            bubbles = false;
            tricklesDown = false;
        }
    }

    public interface INavigatorEvent {}
}