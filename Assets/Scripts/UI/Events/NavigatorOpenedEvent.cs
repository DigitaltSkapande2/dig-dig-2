namespace DigDig2.UIElements
{
    public class NavigatorOpenedEvent : NavigatorEventBase<NavigatorOpenedEvent>
    {
        static NavigatorOpenedEvent()
        {
            SetCreateFunction(() => new NavigatorOpenedEvent());
        }
    }
}