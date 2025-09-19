using System.Collections.Generic;
using Edgegap.Editor.Api.Models.Results;
using UnityEngine;


namespace DigDig2.Effects
{
    public enum EffectIntensity
    {
        low,
        mid,
        high
    }

    public class EffectManager : Singleton<EffectManager>
    {
        [System.Serializable]
        private struct EffectValueIntensityDecoder<T>
        {
            [SerializeField] T lowValue;
            [SerializeField] T midValue;
            [SerializeField] T highValue;

            public T GetValue(EffectIntensity intensity)
            {
                return intensity == EffectIntensity.low ? lowValue :
                        intensity == EffectIntensity.mid ? midValue :
                        highValue ;
            }
        }

        public void PlayScreenShake()
        {

        }
    }
}