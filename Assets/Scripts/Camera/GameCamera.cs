using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DigDig2.CinemaCamera {
    public class GameCamera : Singleton<GameCamera>
    {
        [SerializeField] public List<Transform> targets;
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 5f;
        [SerializeField] public float roatationSpeed = 1;

        private Vector3 targetPos;
        private Quaternion targetRotation;
        private Quaternion baseTargetRotation;
        private float targetFrustumSize;


        void Update()
        {
            this.targetRotation = Quaternion.Slerp(this.targetRotation, baseTargetRotation, Time.deltaTime * roatationSpeed);

            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;

            foreach (var effector in CameraEffector.GetEffectiveCameraEffectors())
            {
                targetPos += effector.position;
                if (effector.rotation.eulerAngles.magnitude > float.Epsilon)
                    targetRotation.eulerAngles += effector.rotation.eulerAngles;
            }


            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            targetRotation.eulerAngles += this.targetRotation.eulerAngles;

            transform.rotation = targetRotation;
        }

        public void SetTargetRotation(float angle)
        {
            baseTargetRotation = quaternion.Euler(0, Mathf.Deg2Rad * angle, 0);
        }
    }
}