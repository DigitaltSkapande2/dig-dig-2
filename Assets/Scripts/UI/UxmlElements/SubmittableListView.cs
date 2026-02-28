using System;
using UnityEngine.UIElements;

namespace DigDig2.UIElements 
{
    [UxmlElement] public partial class SubmittableListView : ListView
    {
        public event Action submit;

        private bool mouseDown = false;

        protected override void HandleEventTrickleDown(EventBase evt)
        {
            base.HandleEventTrickleDown(evt);

            if (evt.eventTypeId == NavigationSubmitEvent.TypeId() || (evt.eventTypeId == MouseUpEvent.TypeId() && mouseDown))
            {
                submit.Invoke();
                mouseDown = false;
            }
            else if (evt.eventTypeId == MouseDownEvent.TypeId())
            {
                mouseDown = true;
            }
            else if (evt.eventTypeId == MouseLeaveEvent.TypeId())
            {
                mouseDown = false;
            }
        }
    }
}