using Unity.Mathematics;
using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class GameCamera : Singleton<GameCamera>, ISaveable
    {
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 5f;
        [SerializeField] public float rotationSpeed = 1;

        private Vector3 targetPos;
        private Quaternion targetRotation;
        private Quaternion baseTargetRotation;
        private float targetFrustumSize;

        public Camera mainCamera;
        private float defaultFrustumHeight;

        void Start()
        {
            mainCamera = GetComponentInChildren<Camera>();
            defaultFrustumHeight = mainCamera.orthographicSize;
        }

        void Update()
        {
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            float frustumSize = defaultFrustumHeight;


            foreach (var effector in CameraEffector.GetEffectivePivotCameraEffectors())
            {
                targetPos += effector.position;
                if (effector.rotation.eulerAngles.magnitude > float.Epsilon)
                    targetRotation.eulerAngles += effector.rotation.eulerAngles;

                frustumSize += effector.frustumSize;
            }


            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            targetRotation.eulerAngles += this.targetRotation.eulerAngles;

            this.targetRotation = Quaternion.Slerp(this.targetRotation, baseTargetRotation, Time.deltaTime * rotationSpeed);

            transform.rotation = targetRotation;

            mainCamera.orthographicSize = frustumSize;
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
        }

        public void SetTargetRotation(float angle)
        {
            SetTargetRotation(angle, false);
        }

        public void SetTargetRotation(float angle, bool setInstant)
        {
            baseTargetRotation = quaternion.Euler(0, Mathf.Deg2Rad * angle, 0);
            Debug.Log($"Set Camera Rotation to {angle} {setInstant}");
            if (setInstant) transform.rotation = baseTargetRotation;
        }

        #region Saving

        public object CollectData()
        {
            return baseTargetRotation;
        }
        public void RestoreState(object dataObject)
        {
            if (dataObject == null) return;
            baseTargetRotation = (Quaternion)dataObject;
            transform.rotation = baseTargetRotation;
        }

        #endregion
    }
}
