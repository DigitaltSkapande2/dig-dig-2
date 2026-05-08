using System.Collections.Generic;
using System.IO;
using System.Linq;

using DigDig2.Debugging;

using Newtonsoft.Json;

using UnityEngine;

namespace DigDig2.SaveSystem
{
	public static class FileSystem
	{
		public static string GetDataPath( ) => Application.persistentDataPath;

		public static void WriteDataToFile( string filePath, object data )
		{
			#if !PLATFORM_WEBGL
			string extension = Path.GetExtension( filePath );
			string dataString = "";
			switch ( extension )
			{
				case ".json":
					dataString = JsonConvert.SerializeObject( data, Formatting.Indented );
					BetterDebug.Log( dataString );
					break;
			}

			File.WriteAllText( filePath, dataString );

			#endif
		}

		public static T ReadDataFromFile<T>( string filePath )
		{
		#if !PLATFORM_WEBGL
			string extension = Path.GetExtension( filePath );
			string dataString = File.ReadAllText( filePath );

			switch ( extension )
			{
				case ".json":
					BetterDebug.Log( "Reading: " + JsonConvert.SerializeObject( JsonConvert.DeserializeObject<T>( dataString ) ) );
					return JsonConvert.DeserializeObject<T>( dataString );
				default: return default;
			}
			#else

			return default;

			#endif
		}

		public static List<string> GetFilesInDirectory( string directoryPath )
		{
#if !PLATFORM_WEBGL
			return Directory.GetFiles(directoryPath).ToList();
#else
			return new();
#endif
		}
	}
}