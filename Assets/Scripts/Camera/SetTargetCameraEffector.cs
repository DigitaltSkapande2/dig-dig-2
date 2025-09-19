using UnityEngine;

namespace DigDig2.CinemaCamera 
{
    public class SetTargetCameraEffector : CameraEffector
    {
        [SerializeField] Transform targetTransform;

        void Update()
        {
            position = targetTransform.position;
        } 
    }
}

