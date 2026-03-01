using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class LockTargetEffector : CameraEffector
    {
        [SerializeField] public Transform targetTransform;

        void Update()
        {
            if (targetTransform != null)
            {
                position = targetTransform.position;
                rotation = targetTransform.rotation;
            }
        }
    }
}