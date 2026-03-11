namespace DigDig2.UI.Events
{
	public class NavigatorDescendantOpenedEvent : NavigatorEventBase<NavigatorDescendantOpenedEvent>
	{
		static NavigatorDescendantOpenedEvent( ) { SetCreateFunction( ( ) => new( ) ); }
	}
}
