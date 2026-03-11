using System;
using DigDig2.CinemaCamera;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace DigDig2.Effects
{
    [RequireComponent(typeof(Volume))]
    public class DepthOfFieldEffect : MonoBehaviour
    {
        private Volume volume;
        private DepthOfField depthOfField;
        Camera mainCamera;
        
        private void Start()
        {
            volume = GetComponent<Volume>();
            mainCamera = GameCamera.Instance.mainCamera;

            // Ensure the volume has a Vignette effect
            if (volume.profile.TryGet(out depthOfField) == false)
            {
                depthOfField = volume.profile.Add<DepthOfField>();
                depthOfField.active = true;
            }
        }
        
        private void Update()
        {
            float targetdepth = Mathf.Abs(mainCamera.transform.localPosition.z + mainCamera.nearClipPlane);
            depthOfField.focusDistance.value = targetdepth;
        }
    }
}