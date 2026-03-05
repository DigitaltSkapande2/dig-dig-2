using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DigDig2.Effects
{
    [RequireComponent(typeof(Volume))]
    public class GreyscalePulseEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
    {
        private Volume volume;
        private ColorAdjustments colorAdjustments;
        private ColorAdjustments defaultcolorAdjustments;

        private void Start() {
            volume = GetComponent<Volume>();

            // Ensure the volume has a Vignette effect
            if (volume.profile.TryGet(out colorAdjustments) == false)
            {
                colorAdjustments = volume.profile.Add<ColorAdjustments>();
                colorAdjustments.active = true;
            }

            // Store the default vignette settings
            defaultcolorAdjustments = ScriptableObject.CreateInstance<ColorAdjustments>();
            defaultcolorAdjustments.saturation = colorAdjustments.saturation;
        }

        internal override void UpdateEffect(float curveValue)
        {
            colorAdjustments.saturation.value = 1 - curveValue;
        }

        internal override void OnEffectEnd(CumulativeEffectInstanceData effect)
        {
            if (effectInstances.Count == 0)
            {
                colorAdjustments.saturation.value = defaultcolorAdjustments.saturation.value;
            }
        }
    }
}
