using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.CinemaCamera {
    public class GameCamera : Singleton<GameCamera>
    {
        [SerializeField] public List<Transform> targets;
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 5f;

        Camera camera;
        private void Start()
        {
            camera = GetComponentInChildren<Camera>();
        }

        void Update()
        {
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            float targetFrustumSize = 0;

            // Loop through all Camera effectors that are currently effective
            foreach (CameraEffector effector in CameraEffector.GetEffectiveCameraEffectors())
            {
                // Position
                targetPos += effector.position;

                // Rotation
                if (effector.rotation.eulerAngles.magnitude > float.Epsilon) targetRotation = effector.rotation;

                // Frustum Size
                targetFrustumSize += effector.frustumSize;
            }

            // Apply Position
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
            // 
        }
    }
}