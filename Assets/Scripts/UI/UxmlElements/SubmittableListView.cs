using System;
using UnityEngine.UIElements;

namespace DigDig2.UIElements 
{
    [UxmlElement] public partial class SubmittableListView : ListView
    {
        public event Action submit;

        protected override void HandleEventTrickleDown(EventBase evt)
        {
            base.HandleEventTrickleDown(evt);

            if (evt.eventTypeId == NavigationSubmitEvent.TypeId())
            {
                submit.Invoke();
            }
        }
    }
}