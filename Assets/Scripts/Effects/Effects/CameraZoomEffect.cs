using UnityEngine;
using DigDig2.CinemaCamera;

namespace DigDig2.Effects
{
    public class CameraZoomEffect : CumulativeEffectBase<EffectInstanceData>
    {
        CameraEffector cameraZoomEffector;

        private void Awake()
        {
            GameObject cameraZoomEffectorObject = new GameObject();
            cameraZoomEffectorObject.transform.SetParent(transform);
            cameraZoomEffector = cameraZoomEffectorObject.AddComponent<CameraEffector>();
        }

        internal override void UpdateEffect(float curveValue)
        {
            cameraZoomEffector.frustumSize = curveValue;
        }

        internal override void OnEffectEnd(EffectInstanceData effect)
        {
            cameraZoomEffector.frustumSize = 0;
        }
    }
}
