using DigDig2.SaveSystem;
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

        void Update()
        {
            timeElapsed += Time.deltaTime;
            float lerpValue = (Mathf.Cos(Mathf.PI * timeElapsed / (dayDurationInMinutes * 30)) + 1) / 2;
            
            smh.midtones.value = Vector4.Lerp(startMidtones, nightMidtones, lerpValue);

            sceneDirectionalLight.intensity =
                Mathf.Lerp(startDirectionalLightIntensity, nightDirectionalLightIntensity, lerpValue);

            // smh.midtones.value = nightValue;
        }
    }
}
