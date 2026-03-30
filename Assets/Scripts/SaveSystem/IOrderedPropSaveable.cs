namespace DigDig2.SaveSystem
{
    public interface IOrderedPropSaveable
    {
        public void OnLoaded(int myId, int activeId, OrderedPropSaver propSaver);
    }
}