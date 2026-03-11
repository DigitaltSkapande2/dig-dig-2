namespace DigDig2.SaveSystem {
	public interface ISaveable {
		object CollectData( );
		void RestoreState( object dataObject );
	}
}
