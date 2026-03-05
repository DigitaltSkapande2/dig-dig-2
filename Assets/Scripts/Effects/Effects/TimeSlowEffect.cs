using UnityEngine;

namespace DigDig2.Effects
{
    public class TimeSlowEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
    {
        internal override void UpdateEffect(float curveValue)
        {
            SetTimeScale(curveValue);
        }

        internal override void OnEffectEnd(CumulativeEffectInstanceData effect)
        {
            print($"SKIBIDI TOILET {effectInstances.Count}");
            if (effectInstances.Count <= 1)
            {
                print("WIZZZ");
                SetTimeScale(1);
            }
        }


        private void SetTimeScale(float timeScale)
        {
            Debug.Log("TIMESCALE: "+ timeScale);
            Time.timeScale = timeScale;
        }
    }
}
