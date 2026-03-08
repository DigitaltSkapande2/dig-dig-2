using UnityEngine;
using System;

namespace DigDig2
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private SerializableTuple<float, AudioClip>[] audioClipCooldownOverrides;
        private AudioSource source;


        private void Start()
        {
            source = GetComponent<AudioSource>();
        }
    }
}