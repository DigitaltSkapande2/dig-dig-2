using System;
using Cysharp.Threading.Tasks;
using DigDig2.Debugging;
using UnityEngine;

namespace DigDig2.Util
{
    public class MaterialFloatToggler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private string floatVarName;
        [SerializeField] private float activatedValue;
        [SerializeField] private float deactivatedValue;
        [SerializeField] private AnimationCurve fadeActive;
        [SerializeField] private AnimationCurve fadeUnactive;
        [SerializeField] private float fadeDuration;

        [Header("Debug")]
        [SerializeField] private bool isActive;

        private bool isTransitioning = false;

        private void Start()
        {
            if (isActive) SetFloatVal(targetRenderers, floatVarName, activatedValue);
            else SetFloatVal(targetRenderers, floatVarName, deactivatedValue);
        }

        public void Toggle()
        {
            if (isActive) DeActivate();
            else Activate();
        }
        
        public async void ActivateForDuration(float duration)
        {
            Activate();
            await UniTask.WaitForSeconds(duration);
            DeActivate();
        }

        public void Activate()
        {
            if (isTransitioning || isActive) return;
            isActive = true;
            FadeFromTo(targetRenderers, floatVarName, fadeDuration, deactivatedValue, activatedValue, fadeActive).Forget();
        }
        
        public void DeActivate()
        {
            if (isTransitioning || !isActive) return;
            isActive = false;
            FadeFromTo(targetRenderers, floatVarName, fadeDuration, deactivatedValue, activatedValue, fadeUnactive).Forget();
        }

        private async UniTask FadeFromTo(Renderer[] targetRenderers, string varName, float duration, float fromValue, float toValue, AnimationCurve curve)
        {
            isTransitioning = true;

            if (duration > 0f)
            {
                float startTime = Time.time;
                float t = 0f;
                while (t < 1f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    t = (Time.time - startTime) / duration;

                    float curveValue = curve.Evaluate(t);
                    float finalVal = Mathf.Lerp(fromValue, toValue, curveValue);

                    SetFloatVal(targetRenderers, varName, finalVal);
                }
            }

            // Finalize
            float c = curve.Evaluate(1);
            float f = Mathf.Lerp(fromValue, toValue, c);
            SetFloatVal(targetRenderers, varName, f);
            isTransitioning = false;
        }

        private static void SetFloatVal(Renderer[] targetRenderers, string varName, float value)
        {
            foreach (var renderer in targetRenderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasFloat(varName))
                    {
                        mat.SetFloat(varName, value);
                    }
                }
            }
        }
        
    }
}