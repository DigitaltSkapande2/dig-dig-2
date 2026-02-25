using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.Effects
{
    //  T is the InstanceData Class

    public class EffectBase<T> : MonoBehaviour where T : ICloneable
    {
        [SerializeField] internal bool useUnscaledTime = true;
        internal List<T> effectInstances = new List<T>();
        public virtual void PlayEffectInstance(T effectInstance)
        {
            // Store a copy of the provided instance so callers can reuse the same template
            // without mutating the serialized asset/state.
            T copy = (T)effectInstance.Clone();
            effectInstances.Add(copy);
            OnEffectStart(copy);
        }

        public void Update()
        {
            for (int i = effectInstances.Count - 1; i > -1; i--)
            {
                UpdateEffect(effectInstances[i]);
            }
        }

        internal virtual void OnEffectStart(T effect)
        {

        }

        internal virtual void UpdateEffect(T effect)
        {

        }

        internal virtual void OnEffectEnd(T effect)
        {

        }
    }
}
