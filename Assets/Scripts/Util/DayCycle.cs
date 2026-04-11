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
    public class DayCycle : MonoBehaviour, ISaveable
    {
        Volume volume;
        ShadowsMidtonesHighlights smh;
        [SerializeField] private string saveKey = "day_cycle_elapsed";
        [SerializeField] float dayDurationInMinutes;
        [FormerlySerializedAs("nightValue")] [SerializeField] Vector4 nightMidtones;
        [SerializeField] private Light sceneDirectionalLight;
        [SerializeField] private float nightDirectionalLightIntensity;
        private Vector4 startMidtones;
        private float startDirectionalLightIntensity;
        private float timeElapsed;

        void Awake( )
        {
            volume = GetComponent<Volume>( );

            if ( !volume.profile.TryGet( out smh ) )
            {
                smh = volume.profile.Add<ShadowsMidtonesHighlights>();
            }

            startMidtones = smh.midtones.value;
            startDirectionalLightIntensity = sceneDirectionalLight.intensity;
        }

        private void Start()
        {
            SaveManager.Instance.RegisterSavable(saveKey, this);
        }

        void Update()
        {
            timeElapsed =  (timeElapsed + Time.deltaTime) % (dayDurationInMinutes * 60);
            float lerpValue = (Mathf.Cos(Mathf.PI * timeElapsed / (dayDurationInMinutes * 30)) + 1) / 2;
            
            smh.midtones.value = Vector4.Lerp(startMidtones, nightMidtones, lerpValue);

            sceneDirectionalLight.intensity =
                Mathf.Lerp(startDirectionalLightIntensity, nightDirectionalLightIntensity, lerpValue);
        }

        public object CollectData()
        {
            return timeElapsed;
        }

        public void RestoreState(object dataObject)
        {
            if (dataObject != null)
            {
                timeElapsed = JsonConvert.DeserializeObject<float>(dataObject.ToString());
            }
        }
    }
}
