using System;
using DigDig2.Debugging;
using DigDig2.Util;
using UnityEngine;
using Random = System.Random;

namespace DigDig2.Audio
{
    [System.Serializable]
    public class AudioClipOverrideSetting
    {
        public float cooldown;
        public AudioClip audioClip;
        private float lastTimePlayed;

        public bool IsCoolingDown => Time.time - lastTimePlayed < cooldown;

        public void MarkPlayed()
        {
            lastTimePlayed = Time.time;
        }
    }
    
    [System.Serializable]
    public class AudioClipData
    {
        public AudioClip audioClip;
        [Range(0.1f, 1f)] public float volume = 1f;
        public Vector2 pitchRange = Vector2.zero;
    }
    
    
	[RequireComponent( typeof( AudioSource ) )]
	public class AudioManager : Singleton<AudioManager>
	{
		[SerializeField] private AudioClipOverrideSetting[ ] audioClipCooldownOverrides;
		private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>( ); 
        }

        public void PlaySound(AudioClipData audioClipData)
        {

            foreach (AudioClipOverrideSetting audioClipCooldownOverride in audioClipCooldownOverrides)
            {
                if (audioClipCooldownOverride.audioClip == audioClipData.audioClip)
                {
                    if (audioClipCooldownOverride.IsCoolingDown)
                    {
                        return;
                    }

                    audioClipCooldownOverride.MarkPlayed();
                    break;
                }
            }


            if (audioClipData.pitchRange == Vector2.zero)
            {
                source.PlayOneShot(audioClipData.audioClip, audioClipData.volume);
            }
            else
            {
                var pitchSource = new GameObject().AddComponent<AudioSource>();
                pitchSource.pitch = UnityEngine.Random.Range(audioClipData.pitchRange.x, audioClipData.pitchRange.y);
                pitchSource.PlayOneShot(audioClipData.audioClip, audioClipData.volume);
                Destroy(pitchSource, 20f);
            }
        }

		public void SetPlaybackVolume( float linearVolume )
		{
			source.volume = linearVolume;
		}
	}
}
