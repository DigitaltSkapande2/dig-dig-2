using System;
using System.Collections.Generic;
using DigDig2.Debugging;
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

        [SerializeField] private CameraEffector selfCameraRotation;

        private float defaultFrustumHeight;

        private void Start()
        {
            mainCamera = GetComponentInChildren<Camera>();
            defaultFrustumHeight = mainCamera.orthographicSize;
            
            var (targetPos, targetRotation, frustumSize) = GetEffectorEffects();
            transform.position = targetPos;
            transform.rotation = targetRotation;
            mainCamera.orthographicSize = frustumSize;
        }

        private void Update()
        {
            var (targetPos, targetRotation, frustumSize) = GetEffectorEffects();

            //BetterDebug.Log($"Update: pos: {targetPos},\nrot: {targetRotation.eulerAngles},\nsize: {frustumSize}");

            transform.SetPositionAndRotation(targetPos, targetRotation);
            mainCamera.orthographicSize = frustumSize;
        }

        private (Vector3, Quaternion, float) GetEffectorEffects()
        {
            effectors = CameraEffector.GetEffectiveEffectors();

            Vector3 targetPos = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            float frustumSize = defaultFrustumHeight;
            
            foreach (CameraEffector effector in effectors)
            {
                effector.UpdateEffector(Time.deltaTime);

                float w = effector.Weight;
                if (w <= 0f) continue;

                targetPos += effector.lerpedPosition * w;
                frustumSize += effector.lerpedFrustumSize * w;
                
                // rotations are evil
                if (effector.lerpedRotation == default) continue;
                Quaternion weightedRot = Quaternion.Slerp(Quaternion.identity, effector.lerpedRotation, w);
                targetRotation *= weightedRot;
            }

            return (targetPos, targetRotation, frustumSize);
        }

        public void SetRotationSpeed(float speed)
        {
            selfCameraRotation.SetRotationSpeed(speed);
        }

        public void SetTargetRotation(float angle, bool instant = false)
        {
            selfCameraRotation.targetRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}