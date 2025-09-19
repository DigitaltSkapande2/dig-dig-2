using UnityEngine;

namespace DigDig2.CinemaCamera 
{
    public class SetTargetCameraEffector : CameraEffector
    {
        Transform targetTransform;

        void Update()
        {
            position = targetTransform.position;
        } 
    }
}

