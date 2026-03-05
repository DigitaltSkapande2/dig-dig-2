using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DigDig2.Effects
{
    [Serializable]
    public class VignettePulseEffectInstanceData : CumulativeEffectInstanceData
    {
        public Color color;
    }

    [RequireComponent(typeof(Volume))]
    public class VignettePulseEffect : CumulativeEffectBase<VignettePulseEffectInstanceData>
    {
        private Volume volume;
        private Vignette vignette;
        private Vignette defaultVignette;

        Color targetColor;

        private void Start() {
            volume = GetComponent<Volume>();

            // Ensure the volume has a Vignette effect
            if (volume.profile.TryGet(out vignette) == false)
            {
                vignette = volume.profile.Add<Vignette>();
                vignette.active = true;
            }

            // Store the default vignette settings
            defaultVignette = ScriptableObject.CreateInstance<Vignette>();
            defaultVignette.intensity.value = vignette.intensity.value;
            defaultVignette.color.value = vignette.color.value;

            // Set override states
            vignette.color.overrideState = true;
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;
        }

        internal override void OnEffectStart(VignettePulseEffectInstanceData effect)
        {
            UpdateTargetColors();
        }

        internal override void UpdateEffect(float curveValue)
        {
            vignette.color.value = Color.Lerp(defaultVignette.color.value, targetColor, curveValue * 2);
            vignette.intensity.value = (curveValue * (1 - defaultVignette.intensity.value) * curveValue) + defaultVignette.intensity.value;
        }

        internal override void OnEffectEnd(VignettePulseEffectInstanceData effect)
        {
            if (effectInstances.Count == 0)
            {
                vignette.intensity.value = defaultVignette.intensity.value;
                vignette.color.value = defaultVignette.color.value;
            }
            else
            {
                UpdateTargetColors();
            }
        }


        private void UpdateTargetColors()
        {
            float totalR = 0f, totalG = 0f, totalB = 0f;
            foreach (VignettePulseEffectInstanceData effect in effectInstances)
            {
                totalR += effect.color.r;
                totalG += effect.color.g;
                totalB += effect.color.b;
            }
            int count = effectInstances.Count;
            targetColor = new Color(totalR / count, totalG / count, totalB / count);
        }
    }
}
