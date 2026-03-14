using System;
using System.Collections.Generic;
using DigDig2.Util;
using Unity.Mathematics;
using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class GameCamera : Singleton<GameCamera>
    {
        [Tooltip("Seconds to reach target position")]
        [SerializeField] public float followSpeed = 5f;
        [SerializeField] public float rotationSpeed = 1f;

        [SerializeField]
        private List<CameraEffector> effectors;

        public Camera mainCamera;
        private Quaternion baseTargetRotation = Quaternion.identity;
        private float defaultFrustumHeight;

        private void Start()
        {
            mainCamera = GetComponentInChildren<Camera>();
            defaultFrustumHeight = mainCamera.orthographicSize;
            if (baseTargetRotation == new Quaternion(0, 0, 0, 0)) baseTargetRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            var (targetPos, targetRotation, frustumSize) = GetEffectorEffects();
            transform.position = targetPos;
            transform.rotation = targetRotation;
            mainCamera.orthographicSize = frustumSize;
        }

        private void Update()
        {
            var (targetPos, targetRotation, frustumSize) = GetEffectorEffects();
            
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            transform.rotation = targetRotation; //Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, frustumSize, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        }

        private (Vector3, Quaternion, float) GetEffectorEffects()
        {
            effectors = CameraEffector.GetEffectiveEffectors();

            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            float frustumSize = defaultFrustumHeight;

            targetRotation *= baseTargetRotation;
            
            foreach (CameraEffector effector in effectors)
            {
                effector.TickWeight(Time.deltaTime);

                float w = effector.Weight;
                if (w <= 0f) continue;

                targetPos += effector.position * w;
                frustumSize += effector.frustumSize * w;
                
                Quaternion weightedRot = Quaternion.Slerp(Quaternion.identity, effector.rotation, w);
                targetRotation *= weightedRot;
            }

            return (targetPos, targetRotation, frustumSize);
        }

        public void SetRotationSpeed(float speed) => rotationSpeed = speed;

        public void SetTargetRotation(float angle) => SetTargetRotation(angle, false);

        public void SetTargetRotation(float angle, bool setInstant)
        {
            baseTargetRotation = Quaternion.Euler(0, angle, 0);
            if (setInstant) transform.rotation = baseTargetRotation;
        }
    }
}