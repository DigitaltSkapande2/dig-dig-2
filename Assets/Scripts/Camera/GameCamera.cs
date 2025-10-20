using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.CinemaCamera {
    public class GameCamera : Singleton<GameCamera>
    {
        [SerializeField] public List<Transform> targets;
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 5f;

        private Vector3 targetPos;
        private Quaternion targetRotation;
        private float targetFrustumSize;


        void Update()
        {
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;

            foreach (var effector in CameraEffector.GetEffectiveCameraEffectors())
            {
                targetPos += effector.position;
                if (effector.rotation.magnitude > float.Epsilon)
                targetRotation = Quaternion.Euler(effector.rotation);
            }




            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

            //transform.rotation = targetRotation;
        }
    }
}