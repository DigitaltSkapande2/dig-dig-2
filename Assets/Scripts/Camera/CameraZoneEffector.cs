using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public abstract class CameraZoneEffector : CameraEffector
    {
        Transform target;
        Collider collider;

        private void Start()
        {
            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError("CinematicZoneEffector requires a Collider component.");
            }

            target = GameCamera.Instance.transform;
        }

        private void Update()
        {
            isActive = collider.bounds.Contains(target.position);
            if (isActive)
            {
                WhileActiveUpdate();
            }
        }

        protected abstract void WhileActiveUpdate();
    }
}