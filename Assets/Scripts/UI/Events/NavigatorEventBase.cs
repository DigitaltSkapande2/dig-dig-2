using System.Collections.Generic;

using UnityEngine.UIElements;

namespace DigDig2.UI.Events
{
	public abstract class NavigatorEventBase<T> : EventBase<T>, INavigatorEvent where T : NavigatorEventBase<T>, new( )
	{
		public Dictionary<string, Dictionary<string, string>> arguments;
		public string nodeName;

		protected NavigatorEventBase( ) { LocalInit( ); }

		protected override void Init( )
		{
			base.Init( );
			LocalInit( );
		}

		private void LocalInit( )
		{
			bubbles = false;
			tricklesDown = false;
		}
	}

	public interface INavigatorEvent
	{ }
}
