namespace DigDig2
{
    public interface ISaveable
    {
        object CollectData();
        void RestoreState(object dataObject);
    }
}
