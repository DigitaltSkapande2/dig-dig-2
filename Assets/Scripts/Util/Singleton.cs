using UnityEngine;

namespace DigDig2.Util
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;

		[Header( "Singleton Settings" )]
		[SerializeField] private bool destroyOnLoad = true;

		public static T Instance
		{
			get
			{
				if ( instance ) return instance;

				T potentialFoundInstance = FindFirstObjectByType<T>( );
				if ( !potentialFoundInstance )
				{
					Debug.LogError( $"No singleton of type {typeof( T ).Name} has been initialized." );
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
			Debug.Log( "Set instance of " + gameObject.name );
			if ( destroyOnLoad ) DontDestroyOnLoad( gameObject );
		}
	}
}
