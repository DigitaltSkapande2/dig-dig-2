using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
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
            [SerializeField] AnimationCurve curve;
            [SerializeField] T lowValue;
            [SerializeField] T midValue;
            [SerializeField] T highValue;

            public EffectValueIntensityDecoder(T lowValue, T midValue, T highValue)
            {
                this.curve = AnimationCurve.EaseInOut(0, 1, 1, 1);
                this.lowValue = lowValue;
                this.midValue = midValue;
                this.highValue = highValue;
            }

            public T GetValue(EffectIntensity intensity)
            {
                return intensity == EffectIntensity.low ? lowValue :
                        intensity == EffectIntensity.mid ? midValue :
                        highValue;
            }
        }

        [SerializeField] Volume volume;

        [Header("Screen Shake")]
        [SerializeField] EffectValueIntensityDecoder<float> screenShakeIntensity = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.9f);
        [SerializeField] EffectValueIntensityDecoder<float> screenShakeDuration = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.5f);
        [Header("Chromatic Aberration")]
        [SerializeField] EffectValueIntensityDecoder<float> chromaticAberrationIntensity = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.9f);
        [SerializeField] EffectValueIntensityDecoder<float> chromaticAberrationDuration = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.5f);
        [Header("Bloom")]
        [SerializeField] EffectValueIntensityDecoder<float> bloomIntensity = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.9f);
        [SerializeField] EffectValueIntensityDecoder<float> bloomDuration = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.5f);
        [Header("Vignette")]
        [SerializeField] EffectValueIntensityDecoder<float> vignetteIntensity = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.9f);
        [SerializeField] EffectValueIntensityDecoder<float> vignetteDuration = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.5f);


        // Private effect references
        private ChromaticAberration chromaticAberration;
        private Bloom bloom;
        private Vignette vignette;

        private LensDistortion lensDistortion;
        private MotionBlur motionBlur;
        private ColorAdjustments colorAdjustments;


        void Start()
        {

        }


        public void PlayScreenShake()
        {

        }

        #region ScreenShake



        #endregion

        #region Bloom



        #endregion

        #region Vignette

        void InitializeVignette()
        {
            // Ensure the volume has a Vignette effect
            if (volume.profile.TryGet(out vignette) == false)
            {
                vignette = volume.profile.Add<Vignette>();
                vignette.active = true;
            }

            vignette.color.overrideState = true;
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;
        }

        #endregion

        #region Chromatic Aberration



        #endregion
    }
}