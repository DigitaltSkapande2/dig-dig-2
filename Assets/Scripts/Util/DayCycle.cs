using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DigDig2
{
    [RequireComponent(typeof(Volume))]
    public class DayCycle : MonoBehaviour
    {
        Volume volume;
        ShadowsMidtonesHighlights smh;
        [SerializeField] float dayDurationInMinutes;
        [SerializeField] Vector4 nightValue;
        private Vector4 startValue; 

        void Awake( )
        {
            volume = GetComponent<Volume>( );

            if ( !volume.profile.TryGet( out smh ) )
            {
                smh = volume.profile.Add<ShadowsMidtonesHighlights>();
            }

            startValue = smh.midtones.value;
        }

        void Update()
        {
            // float lerpValue = (Mathf.Cos(Mathf.PI * Time.time / (dayDurationInMinutes * 30)) + 1) / 2;
            //
            // smh.midtones.value = Vector4.Lerp(startValue, nightValue, lerpValue);
            smh.midtones.value = nightValue;
        }
    }
}
