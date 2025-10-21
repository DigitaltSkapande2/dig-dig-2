using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public abstract class TriggerZoneCameraEffectorBase : CameraEffector
    {
        Transform target;
        Collider collider;

        protected new void Start()
        {
            Debug.Log("ZINKKKKK");
            base.Start();

            collider = GetComponent<Collider>();
            if (collider == null)
            {   
                Debug.LogError("CinematicZoneEffector requires a Collider component.");
            }

            target = GameCamera.Instance.GetComponent<Transform>();
        }

        private void Update()
        {
            if (target == null || collider == null) return;
            
            bool shouldActive = collider.bounds.Contains(target.position);

            if (shouldActive != IsActive)
            {
                IsActive = shouldActive;
                if (shouldActive)
                {
                    OnZoneEnter();
                }
                else
                {
                    OnZoneExit();
                }
            }

            if (IsActive)
            {
                WhileActiveUpdate();
            }
        }
        
        protected abstract void OnZoneEnter();
        protected abstract void OnZoneExit();
        protected abstract void WhileActiveUpdate();
    }
}