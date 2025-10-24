using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections;
using System;


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
            [SerializeField] public AnimationCurve curve;
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
        private Vignette defaultVignette;

        private LensDistortion lensDistortion;
        private MotionBlur motionBlur;
        private ColorAdjustments colorAdjustments;


        // ScreenShake Effector Refeence
        private CinemaCamera.ScreenShakeEffector screenShakeEffector;

        void Start()
        {
            screenShakeEffector = gameObject.AddComponent<CinemaCamera.ScreenShakeEffector>();
            InitializeVignette();
        }




        #region ScreenShake

        public void PlayScreenShake(EffectIntensity intensity = EffectIntensity.mid)
        {
            screenShakeEffector.Shake(screenShakeIntensity.GetValue(intensity));
        }

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
            // Store the default vignette settings
            defaultVignette = ScriptableObject.CreateInstance<Vignette>();
            defaultVignette.intensity.value = vignette.intensity.value;
            defaultVignette.color.value = vignette.color.value;

            // Set override states
            vignette.color.overrideState = true;
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;
        }

        public void PlayVignettePulse(EffectIntensity intensity = EffectIntensity.mid, Color color = default)
        {
            StartCoroutine(CurvePulseRoutine(vignetteIntensity.curve, vignetteDuration.GetValue(intensity), (float curveValue) =>
            {
                print("curveValue: " + curveValue);
                float targetIntensity = vignetteIntensity.GetValue(intensity);
                vignette.color.value = Color.Lerp(defaultVignette.color.value, color == default ? Color.black : color, curveValue * 2);
                vignette.intensity.value = (targetIntensity * (1-defaultVignette.intensity.value) * curveValue) + defaultVignette.intensity.value;

            }, () => {
                vignette.intensity.value = defaultVignette.intensity.value;
                vignette.color.value = defaultVignette.color.value;
            }));
        }

        #endregion

        #region Chromatic Aberration



        #endregion



        private IEnumerator CurvePulseRoutine(AnimationCurve curve, float duration, Action<float> action, Action onComplete = null)
        {
            float progress = 0f;
            while (progress < duration)
            {
                float curveValue = curve.Evaluate(progress / duration);
                action?.Invoke(curveValue);

                progress += Time.deltaTime;
                yield return null; // Skip to next frame
            }

            onComplete?.Invoke();

            yield break;
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(EffectManager))]
    public class EffectManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EffectManager effectManager = (EffectManager)target;

            if (GUILayout.Button("Play ScreenShake"))
            {
                effectManager.PlayScreenShake(EffectIntensity.mid);
            }

            if (GUILayout.Button("Play Vignette Pulse"))
            {
                effectManager.PlayVignettePulse(EffectIntensity.mid, Color.red);
            }

        }
    }

    #endif
}