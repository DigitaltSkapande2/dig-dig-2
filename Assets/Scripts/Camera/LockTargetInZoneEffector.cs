using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class LockTargetInZoneEffector : TriggerZoneCameraEffectorBase
    {
        [SerializeField] Transform targetTransform;
        [SerializeField] float targetFrustumSize = 10f;

        protected override void OnZoneEnter()
        {
            
        }

        protected override void OnZoneExit()
        {
            
        }

        protected override void WhileActiveUpdate()
        {
            if (targetTransform != null)
            {
                position = targetTransform.position;
                rotation = targetTransform.eulerAngles;
                frustumSize = targetFrustumSize;
            }
        }
    }
}