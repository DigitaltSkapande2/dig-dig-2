using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DigDig2.Util
{
    public class MaterialColorToggler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private MeshRenderer[] targetRenderers;
        [SerializeField] private string colorVarName;
        [SerializeField, ColorUsage(true, true)] private Color activatedValue;
        [SerializeField, ColorUsage(true, true)] private Color deactivatedValue;
        [SerializeField] private AnimationCurve fadeActive;
        [SerializeField] private AnimationCurve fadeUnactive;
        [SerializeField] private float fadeDuration;

        [Header("Debug")]
        [SerializeField] private bool isActive;

        private bool isTransitioning = false;

        private void Start()
        {
            if (isActive) SetColorVal(targetRenderers, colorVarName, activatedValue);
            else SetColorVal(targetRenderers, colorVarName, deactivatedValue);
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
            if (isTransitioning) return;
            isActive = true;
            FadeFromTo(targetRenderers, colorVarName, fadeDuration, deactivatedValue, activatedValue, fadeActive).Forget();
        }
        
        public void DeActivate()
        {
            if (isTransitioning) return;
            isActive = false;
            FadeFromTo(targetRenderers, colorVarName, fadeDuration, activatedValue, deactivatedValue, fadeUnactive).Forget();
        }

        private async UniTask FadeFromTo(MeshRenderer[] targetRenderers, string varName, float duration, Color fromValue, Color toValue, AnimationCurve curve)
        {
            isTransitioning = true;
            float startTime = Time.time;
            float t = 0f;
            while (t < 1f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                t = (Time.time - startTime) / duration;
                
                float curveValue = curve.Evaluate(t);
                Color finalVal = Color.Lerp(fromValue, toValue, curveValue);

                SetColorVal(targetRenderers, varName, finalVal);
            }
            
            // Finalize
            SetColorVal(targetRenderers, varName, toValue);
            isTransitioning = false;
        }

        private static void SetColorVal(MeshRenderer[] targetRenderers, string varName, Color value)
        {
            foreach (var renderer in targetRenderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasColor(varName))
                    {
                        mat.SetColor(varName, value);
                    }
                }
            }
        }
        
    }
}