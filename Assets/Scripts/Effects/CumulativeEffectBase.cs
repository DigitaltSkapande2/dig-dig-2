using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.Effects
{
    [Serializable]
    public class EffectInstanceData
    {
        [NonSerialized] internal float durationPassed = 0f;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public float duration = 0.4f;
        public float intensity = 1f;

        // Create a shallow clone of this instance. Derived classes will inherit this and
        // MemberwiseClone will preserve derived fields as well.
        public virtual EffectInstanceData Clone()
        {
            return (EffectInstanceData)MemberwiseClone();
        }
    }


    /// <summary>
    ///  Cumulative, meaning the effect stacks if called multiple times before finishing
    /// </summary>
    public class CumulativeEffectBase<T> : MonoBehaviour where T : EffectInstanceData
    {
        [SerializeField] internal bool useUnscaledTime = true;
        internal List<T> effectInstances = new List<T>();
        public void PlayEffectInstance(T effectInstance)
        {
            // Store a copy of the provided instance so callers can reuse the same template
            // without mutating the serialized asset/state.
            T copy = (T)effectInstance.Clone();
            copy.durationPassed = 0f;
            effectInstances.Add(copy);
            OnEffectStart(copy);
        }

        public void Update()
        {
            float cumulativeCurveValue = 0f;
            for (int i = effectInstances.Count - 1; i > -1; i--)
            {
                T effect = effectInstances[i];
                effect.durationPassed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                if (effect.durationPassed > effect.duration)
                {
                    effectInstances.RemoveAt(i);
                    OnEffectEnd(effect);
                    continue;
                }

                float curveValue = effect.intensityCurve.Evaluate(effect.durationPassed / effect.duration);
                cumulativeCurveValue += curveValue * effect.intensity;

                // if T is a class, changes to 'effect' already apply; if T is a struct the assignment
                // would be necessary to write back the mutated copy. We keep code simple and avoid
                // the assignment for class-based EffectInstanceData.
            }

            if (effectInstances.Count > 0) UpdateEffect(cumulativeCurveValue);
        }

        internal virtual void OnEffectStart(T effect)
        {

        }

        internal virtual void UpdateEffect(float curveValue)
        {

        }

        internal virtual void OnEffectEnd(T effect)
        {

        }
    }
}
