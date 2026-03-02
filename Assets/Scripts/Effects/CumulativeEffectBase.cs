using System;
using UnityEngine;

namespace DigDig2.Effects
{
    [Serializable]
    public class CumulativeEffectInstanceData : ICloneable
    {
        // Duration passed since the effect started, used to evaluate the curve
        [NonSerialized] internal float durationPassed = 0f;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public float duration = 0.4f;
        public float intensity = 1f;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }


    /// <summary>
    ///  Cumulative, meaning the effect stacks if called multiple times before finishing
    /// </summary>
    public class CumulativeEffectBase<T> : EffectBase<T> where T : CumulativeEffectInstanceData
    {

        internal override void OnEffectStart(T effect)
        {
            if (effect == null) return;
            effect.durationPassed = 0f;
        }

        internal new void Update()
        {
            float cumulativeCurveValue = 0f;
            // iterate backwards to allow removal while iterating
            for (int i = effectInstances.Count - 1; i >= 0; i--)
            {
                T effect = effectInstances[i];
                if (effect == null)
                {
                    effectInstances.RemoveAt(i);
                    continue;
                }

                // advance timer using chosen timescale
                effect.durationPassed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                // expired?
                if (effect.durationPassed > effect.duration)
                {
                    effectInstances.RemoveAt(i);
                }

                // evaluate curve (normalized time) and accumulate the weighted value
                float normalized = Mathf.Clamp01(effect.durationPassed / Mathf.Max(float.Epsilon, effect.duration));
                float curveValue = effect.intensityCurve.Evaluate(normalized);
                cumulativeCurveValue += curveValue * effect.intensity;

                // if T is a class, changes to 'effect' already apply; if T is a struct the assignment
                // would be necessary to write back the mutated copy. We keep code simple and avoid
                // the assignment for class-based EffectInstanceData.
            }

            if (effectInstances.Count > 0) UpdateEffect(cumulativeCurveValue);
            if (effectInstances.Count > 0)
            {
                UpdateEffect(cumulativeCurveValue);
            }
        }

        internal virtual void UpdateEffect(float cumulativeValue)
        {
            // default: do nothing. Derived classes should override.
        }
    }
}
