using System;

using UnityEngine.UIElements;

namespace DigDig2.UI.UxmlElements
{
	[UxmlElement] public partial class SubmittableListView : ListView
	{
		private bool mouseDown;
		public event Action Submit;

		protected override void HandleEventTrickleDown( EventBase evt )
		{
			base.HandleEventTrickleDown( evt );

			if ( evt.eventTypeId == NavigationSubmitEvent.TypeId( ) || evt.eventTypeId == MouseUpEvent.TypeId( ) && mouseDown )
			{
				Submit?.Invoke( );
				mouseDown = false;
			} else if ( evt.eventTypeId == MouseDownEvent.TypeId( ) )
				mouseDown = true;
			else if ( evt.eventTypeId == MouseLeaveEvent.TypeId( ) ) mouseDown = false;
		}
	}
}
