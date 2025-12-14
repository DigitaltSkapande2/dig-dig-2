using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using NUnit.Framework;


#if UNITY_EDITOR
using UnityEditor;
#endif


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

            public EffectValueIntensityDecoder(T lowValue, T midValue, T highValue)
            {
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

        [System.Serializable]
        public struct CurveEffect
        {
            private struct EffectInstanceData 
            {
                public float durrationPassed;
                public float duration;
                public float intensity;

                public EffectInstanceData(float duration, float intensity) {
                    this.durrationPassed = 0;
                    this.duration = duration;
                    this.intensity = intensity;
                }
            }
            
            [SerializeField] AnimationCurve curve;
            [SerializeField] EffectValueIntensityDecoder<float> intensityDecoder;
            [SerializeField] EffectValueIntensityDecoder<float> durationDecoder;

            Action<float, float> onUpdate;
            Action onStop;

            List<EffectInstanceData> effectInstances;

            public CurveEffect(bool _) {
                this.onUpdate = null;
                this.onStop = null;

                intensityDecoder = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.6f);
                durationDecoder = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.6f);
                curve = AnimationCurve.Constant(0, 1, 1);
                effectInstances = new List<EffectInstanceData>();
            }

            public void SetCallbacks(UnityEvent updateEvent, Action<float, float> onUpdate, Action onStop = null) {
                this.onUpdate = onUpdate;
                this.onStop = onStop;
            }

            public void UpdateEffect()
            {
                for (int i = effectInstances.Count-1; i > -1; i--)
                {
                    EffectInstanceData effect = effectInstances[i];
                    effect.durrationPassed += Time.unscaledDeltaTime;
                    if (effect.durrationPassed > effect.duration) {
                        effectInstances.RemoveAt(i);
                        continue;
                    }

                    float curveValue = curve.Evaluate(effect.durrationPassed / effect.duration);
                    onUpdate?.Invoke(curveValue, effect.intensity);

                    effectInstances[i] = effect;
                }
            }

            public void PlayEffect(EffectIntensity intensity = EffectIntensity.mid, Action<float, float> onUpdate = null, Action onStop = null)
            {
                if (onUpdate != null) this.onUpdate = onUpdate;
                if (onStop != null) this.onStop = onStop;
                effectInstances.Add(new EffectInstanceData(
                    durationDecoder.GetValue(intensity),
                    intensityDecoder.GetValue(intensity)
                ));
            }
        }


        [SerializeField] Volume volume;
        [Header("--- EFFECTS ------")]
        [SerializeField] EffectValueIntensityDecoder<float> screenShakeIntensity = new EffectValueIntensityDecoder<float>(0.1f, 0.3f, 0.6f);
        [SerializeField] CurveEffect timeSlowEffect;
        [SerializeField] CurveEffect zoomBounceEffect;
        [SerializeField] CurveEffect chromaticAberrationEffect;
        [SerializeField] CurveEffect bloomEffect;
        [SerializeField] CurveEffect vignettePulseEffect;


        // Private effect references
        private ChromaticAberration chromaticAberration;
        private Bloom bloom;
        private Vignette vignette;
        private Vignette defaultVignette;

        private LensDistortion lensDistortion;
        private MotionBlur motionBlur;
        private ColorAdjustments colorAdjustments;

        UnityEvent effectUpdate;

        private CinemaCamera.ScreenShakeEffector screenShakeEffector;
        private CinemaCamera.CameraEffector zoomBounceCameraEffector;

        // Time Management
        bool isTimeStopped = false;
        float currentTimeScale = 1;
        int currentTimeSetAuthorityLevel;


        private new void Awake()
        {
            base.Awake();
            currentTimeScale = Time.timeScale;
        }

        private void Start()
        {
            screenShakeEffector = gameObject.AddComponent<CinemaCamera.ScreenShakeEffector>();
            zoomBounceCameraEffector = gameObject.AddComponent<CinemaCamera.CameraEffector>();
            InitializeVignette();

            effectUpdate = new UnityEvent();

            timeSlowEffect.SetCallbacks(effectUpdate, 
                (float curveValue, float effectIntensity) =>
                {
                    SetTimeScale(effectIntensity);
                },
                () => // called on curve pulse completed
                {
                    SetTimeScale(1);
                }
            );

            zoomBounceEffect.SetCallbacks(effectUpdate,
                (float curveValue, float effectIntensity) =>
                {
                    zoomBounceCameraEffector.frustumSize = effectIntensity;
                },
                () => // called on curve pulse completed
                {
                    zoomBounceCameraEffector.frustumSize = 0;
                }
            );

            vignettePulseEffect.SetCallbacks(effectUpdate,
                (float curveValue, float effectIntensity) =>
                {
                    print("curveValue: " + curveValue);
                    vignette.color.value = Color.Lerp(defaultVignette.color.value, Color.red, curveValue * 2);
                    vignette.intensity.value = (effectIntensity * (1 - defaultVignette.intensity.value) * curveValue) + defaultVignette.intensity.value;

                },
                () => // called on curve pulse completed
                {
                    vignette.intensity.value = defaultVignette.intensity.value;
                    vignette.color.value = defaultVignette.color.value;
                }
            );

        }

        private void Update()
        {
            UpdateTimeScale();

            effectUpdate.Invoke();
        }


        #region ScreenShake

        public void PlayScreenShake(EffectIntensity intensity = EffectIntensity.mid)
        {
            screenShakeEffector.Shake(screenShakeIntensity.GetValue(intensity));
        }

        #endregion

        #region Bloom



        #endregion

        #region TimeStop

        void InitializeTimeStop()
        {
            
        }

        public void PlayTimeSlow(EffectIntensity intensity = EffectIntensity.mid)
        {
            timeSlowEffect.PlayEffect(intensity);
        }

        #endregion

        #region ZoomBounce

        void InitializeZoomBounce()
        {

        }

        public void PlayZoomBounce(EffectIntensity intensity = EffectIntensity.mid)
        {
            zoomBounceEffect.PlayEffect(intensity);
        }

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

        public void PlayVignettePulse(EffectIntensity intensity = EffectIntensity.mid, Color color = default) // TODO find a way to incorperate Color
        {
            vignettePulseEffect.PlayEffect(intensity);
        }

        #endregion

        #region Chromatic Aberration



        #endregion

        #region Time Management

        private void UpdateTimeScale()
        {
            if (Time.timeScale == currentTimeScale && isTimeStopped) return;

            Time.timeScale = currentTimeScale;
        }

        public void StopTime()
        {
            isTimeStopped = true;
            Time.timeScale = 0;
        }

        public void StartTime()
        {
            isTimeStopped = false;
            Time.timeScale = currentTimeScale;
        }

        public void SetTimeScale(float scale, int authorityLevel = 0)
        {
            if (authorityLevel < currentTimeSetAuthorityLevel) return;

            currentTimeScale = scale;
            currentTimeSetAuthorityLevel = authorityLevel;
        }



        #endregion

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

            if (GUILayout.Button("Play ZoomBounce"))
            {
                effectManager.PlayZoomBounce(EffectIntensity.mid);
            }

            if (GUILayout.Button("Play TimeStop"))
            {
                effectManager.PlayTimeSlow(EffectIntensity.mid);
            }

            if (GUILayout.Button("Play Hit"))
            {
                effectManager.PlayTimeSlow(EffectIntensity.mid);
                effectManager.PlayZoomBounce(EffectIntensity.mid);
                effectManager.PlayVignettePulse(EffectIntensity.mid, Color.red);
            }

        }
    }

    #endif
}