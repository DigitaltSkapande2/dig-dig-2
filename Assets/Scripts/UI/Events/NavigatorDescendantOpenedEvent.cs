namespace DigDig2.UIElements
{
    public class NavigatorDescendantOpenedEvent : NavigatorEventBase<NavigatorDescendantOpenedEvent>
    {
        static NavigatorDescendantOpenedEvent()
        {
            SetCreateFunction(() => new NavigatorDescendantOpenedEvent());
        }
    }
}