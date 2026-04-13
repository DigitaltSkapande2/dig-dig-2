using System;
using DigDig2.SaveSystem;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace DigDig2
{
    [RequireComponent(typeof(Volume))]
    public class DayCycle : MonoBehaviour
    {
        Volume volume;
        ShadowsMidtonesHighlights smh;
        [SerializeField] private string saveKey = "day_cycle_elapsed";
        [SerializeField] float dayDurationInMinutes;
        [SerializeField] private float cycleStartOffset = 0.5f;
        [FormerlySerializedAs("nightValue")] [SerializeField] Vector4 nightMidtones;
        [SerializeField] private Light sceneDirectionalLight;
        [SerializeField] private float nightDirectionalLightIntensity;
        private Vector4 startMidtones;
        private float startDirectionalLightIntensity;
        private float timeElapsed;
        private float bakedCycleOffset;

        private SaveManager saveManager;

        void Awake( )
        {
            volume = GetComponent<Volume>( );

            if ( !volume.profile.TryGet( out smh ) )
            {
                smh = volume.profile.Add<ShadowsMidtonesHighlights>();
            }

            startMidtones = smh.midtones.value;
            startDirectionalLightIntensity = sceneDirectionalLight.intensity;

            saveManager = SaveManager.Instance;
        }

        private void Start()
        {
            bakedCycleOffset = (dayDurationInMinutes * 60) * cycleStartOffset;
        }

        void Update()
        {
            float lerpValue = (Mathf.Cos(Mathf.PI * (saveManager.GetPlayTime() + bakedCycleOffset) / (dayDurationInMinutes * 30)) + 1) / 2;
            
            smh.midtones.value = Vector4.Lerp(startMidtones, nightMidtones, lerpValue);

            sceneDirectionalLight.intensity =
                Mathf.Lerp(startDirectionalLightIntensity, nightDirectionalLightIntensity, lerpValue);
        }
    }
}
