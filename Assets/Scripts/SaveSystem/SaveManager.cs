using System.Collections.Generic;
using System.IO;
using System.Linq;

using DigDig2.Util;
using DigDig2.Debugging;

using UnityEngine;

namespace DigDig2.SaveSystem
{
	public class SaveManager : Singleton<SaveManager>
	{
		private const string SAVES_DIRECTORY_NAME = "saves";
		private const string SAVE_FILE_EXTENSION = ".json";
		private const string NEW_SAVE_PREFIX = "Save";

		private readonly Dictionary<string, ISaveable> registeredSavables = new( );
		private GameSave loadedGameSave;
		
		public bool isMultiplayer;

		private bool HasLoadedSave
		{
			get => loadedGameSave != null;
		}

		private new void Awake( )
		{
			base.Awake( );
			string saveDirectoryPath = GetSavesDirectoryPath( );
			BetterDebug.Log( $"Saving files in: {saveDirectoryPath}" );
			if ( !Directory.Exists( saveDirectoryPath ) ) Directory.CreateDirectory( saveDirectoryPath );
		}

		public class GameSave
		{
			public string saveName;
			public Dictionary<string, object> stateData;
			public string version;
		}

		#region Directory Management

		public string GetSavesDirectoryPath( ) => Path.Join( FileSystem.GetDataPath( ), SAVES_DIRECTORY_NAME );

		public string GetSaveFilePathFromName( string saveName ) => Path.Join( GetSavesDirectoryPath( ), saveName + SAVE_FILE_EXTENSION );

		public List<string> GetSaveFiles( ) => FileSystem.GetFilesInDirectory( GetSavesDirectoryPath( ) );

		public List<string> GetSaveFileSaveNames( )
		{
			List<string> saveFileNames = new( );
			List<string> saveFiles = GetSaveFiles( );
			foreach ( string saveFilePath in saveFiles )
			{
				GameSave saveFileData = FileSystem.ReadDataFromFile<GameSave>( saveFilePath );
				saveFileNames.Add( saveFileData.saveName );
			}

			return saveFileNames;
		}

		#endregion

		#region Save Creation

		public void CreateNewSave( bool isMultiplayer = false, string saveName = "" )
		{
			if ( saveName == string.Empty ) saveName = GetNextFreeSaveName( );

            this.isMultiplayer = isMultiplayer;

			loadedGameSave = new( )
			{
				saveName = saveName,
				version = Application.version,
				stateData = new( )
			};
		}

		public string GetNextFreeSaveName( ) => $"{NEW_SAVE_PREFIX} {GetSaveFiles( ).Count + 1}";

		#endregion

		#region Saving

		public void SaveAllAndWriteToFile( string saveName = "" )
		{
			SaveAll( );
			WriteSaveToFile( );
		}

		public void WriteSaveToFile( string saveName = "" )
		{
			loadedGameSave.saveName = saveName == string.Empty ? loadedGameSave.saveName : saveName;
			loadedGameSave.version = Application.version;
			FileSystem.WriteDataToFile( GetSaveFilePathFromName( loadedGameSave.saveName ), loadedGameSave );
			BetterDebug.Log( $"Wrote save file \"{loadedGameSave.saveName}\" to disk, State data = {loadedGameSave.stateData.Count} entries" );
		}

		public void SaveAll( )
		{
            foreach (KeyValuePair<string, ISaveable> saveablePair in registeredSavables)
            {
                BetterDebug.Log($"Saving key [{saveablePair}] with Savable named [{saveablePair.Value}]");
                WriteToSaveData( saveablePair.Key, saveablePair.Value.CollectData( ) );
            }
		}

		public void WriteToSaveData( string uniqueName, object data )
		{
			loadedGameSave.stateData[ uniqueName ] = data;
			BetterDebug.Log( $"wrote save data for key \"{uniqueName}\"" );
		}

		#endregion

		#region Loading

		public GameSave ReadSaveFile( string saveName ) => FileSystem.ReadDataFromFile<GameSave>( GetSaveFilePathFromName( saveName ) );

		public bool LoadSave( GameSave gameSave,  bool isMultiplayer = false )
		{
			if ( gameSave == null )
			{
				BetterDebug.Log( "TRYING TO LOAD NULL GAMESAVE", LogSeverity.Error );
				return false;
			}

            this.isMultiplayer = isMultiplayer;
			loadedGameSave = gameSave;
			BetterDebug.Log( $"Loaded save of name {gameSave.saveName}" );

			return true;
		}

		public bool LoadSave( string saveName,  bool isMultiplayer = false )
		{
			GameSave gameSave = ReadSaveFile( saveName );
			if ( gameSave != null ) return LoadSave( gameSave, isMultiplayer );

			BetterDebug.Log( $"Trying to Load Save of unknown name: {saveName}", LogSeverity.Warning );
			return false;
		}

		public void UnloadCurrentlyLoadedSave( ) { loadedGameSave = null; }

		#endregion

		#region ISaveable Interaction

        public void GarbageCollectISavables()
        {
            foreach (KeyValuePair<string, ISaveable> keyValuePair in registeredSavables)
            {
                if (keyValuePair.Value == null)
                {
                    registeredSavables.Remove(keyValuePair.Key);
                }
            }
        }
        
		public void Reset( )
		{
			registeredSavables.Clear( );
		}

		public void RegisterSavable( string uniqueName, ISaveable saveable, bool restoreOnRegister = true )
		{
            if (registeredSavables.ContainsKey(uniqueName))
            {
                if (registeredSavables[uniqueName] != null)
                {
                    BetterDebug.Log( $"Trying to register Savable with already registered uniqueName: {uniqueName}, aborting", LogSeverity.Warning );
                }
                else
                {
                    registeredSavables.Remove(uniqueName);
                }
            }
			else
            {
                BetterDebug.Log($"Registered NEW Savable with uniqueName \"{uniqueName}\"");
            }
            
			registeredSavables.Add( uniqueName, saveable );

			if ( restoreOnRegister ) RestoreISavable( uniqueName, saveable );
		}

		public void RestoreISavable( string uniqueName, ISaveable saveable = null )
		{
			if ( !HasLoadedSave )
			{
				BetterDebug.Log( "Trying to restore Savable state with no save loaded, be sure to load a save first!", LogSeverity.Error );
				return;
			}

			if ( saveable == null )
			{
				if ( registeredSavables.Keys.Contains( uniqueName ) )
					saveable = registeredSavables[ uniqueName ];
				else
					return;
			}

			if ( loadedGameSave.stateData.Keys.Contains( uniqueName ) )
			{
				saveable.RestoreState( loadedGameSave.stateData[ uniqueName ] );
				BetterDebug.Log( $"Restored state for Savable with uniqueName \"{uniqueName}\"" );
			}
			else
			{
				saveable.RestoreState( null );
				BetterDebug.Log( $"Restored state NULL for Savable with uniqueName \"{uniqueName}\"" );
			}
		}

		#endregion
	}
}
