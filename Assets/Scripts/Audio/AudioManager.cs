using DigDig2.Util;

using UnityEngine;

namespace DigDig2.Audio {
	[RequireComponent( typeof( AudioSource ) )]
	public class AudioManager : Singleton<AudioManager> {
		[SerializeField] private SerializableTuple<float, AudioClip>[ ] audioClipCooldownOverrides;
		private AudioSource source;

		private void Start( ) { source = GetComponent<AudioSource>( ); }
	}
}
