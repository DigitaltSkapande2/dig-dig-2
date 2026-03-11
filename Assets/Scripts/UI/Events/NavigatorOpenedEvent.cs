namespace DigDig2.UI.Events
{
	public class NavigatorOpenedEvent : NavigatorEventBase<NavigatorOpenedEvent>
	{
		static NavigatorOpenedEvent( ) { SetCreateFunction( ( ) => new( ) ); }
	}
}
