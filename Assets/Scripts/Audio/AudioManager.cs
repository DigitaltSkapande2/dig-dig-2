using DigDig2.Debugging;
using DigDig2.Util;
using UnityEngine;

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
    
    
	[RequireComponent( typeof( AudioSource ) )]
	public class AudioManager : Singleton<AudioManager>
	{
		[SerializeField] private AudioClipOverrideSetting[ ] audioClipCooldownOverrides;
		private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>( ); 
        }

        public void PlaySound(AudioClip audioClip)
        {

            foreach (AudioClipOverrideSetting audioClipCooldownOverride in audioClipCooldownOverrides)
            {
                if (audioClipCooldownOverride.audioClip == audioClip)
                {
                    if (audioClipCooldownOverride.IsCoolingDown)
                    {
                        return;
                    }

                    audioClipCooldownOverride.MarkPlayed();
                    break;
                }
            }
            
            
            BetterDebug.Log("AAAAAAAAA " + audioClip.name);
            source.PlayOneShot(audioClip);
            
        }

		public void SetPlaybackVolume( float linearVolume )
		{
			source.volume = linearVolume;
		}
	}
}
