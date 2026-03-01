using Unity.Mathematics;
using UnityEngine;

namespace DigDig2.CinemaCamera {
    public class GameCamera : Singleton<GameCamera>
    {
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 5f;
        [SerializeField] public float rotationSpeed = 1;
        [Header("BAD CODE Animation Settings")]
        [SerializeField] private string zoomOutAnimationTriggerName = "ZoomOut";

        private Vector3 targetPos;
        private Quaternion targetRotation;
        private Quaternion baseTargetRotation;
        private float targetFrustumSize;

        Camera mainCamera;
        Animator animator;
        private float defaultFrustumHeight;

        void Start()
        {
            mainCamera = GetComponentInChildren<Camera>();
            defaultFrustumHeight = mainCamera.orthographicSize;
            animator = GetComponent<Animator>();
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
            baseTargetRotation = quaternion.Euler(0, Mathf.Deg2Rad * angle, 0);
        }


        #region BAD CODE 

        public void ZoomOutAnimation()
        {
            animator.SetTrigger(zoomOutAnimationTriggerName);
        }

        #endregion
    }
}
