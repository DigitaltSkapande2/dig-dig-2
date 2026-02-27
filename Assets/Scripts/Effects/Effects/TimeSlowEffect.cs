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
            SetTimeScale(1);
        }


        private void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }
    }
}
