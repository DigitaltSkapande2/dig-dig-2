using DigDig2.Debugging;

using UnityEngine;

namespace DigDig2.Util
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;

		[Header( "Singleton Settings" )]
		[SerializeField] private bool dontDestroyOnLoad = true;

		public static T Instance
		{
			get
			{
				if ( instance ) return instance;

				T potentialFoundInstance = FindFirstObjectByType<T>( );
				if ( !potentialFoundInstance )
				{
					BetterDebug.Log( $"Attempted to get instance of singleton \"{typeof( T ).Name}\" but none has been initialized.", LogSeverity.Error );
					return null;
				}

				instance = potentialFoundInstance;

				return instance;
			}

			private set => instance = value;
		}

		protected virtual void Awake( )
		{
			if ( instance && instance != this )
			{
				Destroy( gameObject );
				return;
			}

			Instance = this as T;
			BetterDebug.Log( "Set instance of " + gameObject.name );
			if ( dontDestroyOnLoad ) DontDestroyOnLoad( gameObject );
		}
	}
}
