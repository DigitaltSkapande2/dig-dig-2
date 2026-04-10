using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2.Debugging;
using DigDig2.Util;
using Unity.Mathematics;
using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class GameCamera : Singleton<GameCamera>
    {
        private static Quaternion SafeQuaternion(Quaternion q) => 
            q == default ? Quaternion.identity : Quaternion.Normalize(q);
        
        public Camera mainCamera;

        [SerializeField] private PartialCameraEffector selfCameraRotation;

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

            BetterDebug.Log($"Update: pos: {targetPos},\nrot: {targetRotation},\nsize: {frustumSize}");

            transform.SetPositionAndRotation(targetPos, targetRotation);
            mainCamera.orthographicSize = frustumSize;
        }

        private (Vector3 position, Quaternion rotation, float frustumSize) GetEffectorEffects()
        {
            
            float deltaTime = Time.deltaTime;
            
            Vector3 basePosition = Vector3.zero;
            Quaternion baseRotation = Quaternion.identity;
            float baseFrustumSize = defaultFrustumHeight;

            float overridePartialsWeight = 0f;
            
            foreach (var effector in CameraEffector.GetAll())
            {
                effector.UpdateEffector(deltaTime);
                //BetterDebug.Log($"[{effector}, {effector.Weight}]");
         
                float w = effector.Weight;
                if (w <= float.Epsilon) continue;
                
                if (effector.overridePartials) overridePartialsWeight += effector.Weight;
                
                basePosition = Vector3.Lerp(basePosition,effector.lerpedPosition, w);
                baseFrustumSize += effector.lerpedFrustumSize * w; // this bad, But we already made it bad. Don't Drink bad milk
                baseRotation = SafeQuaternion(Quaternion.Slerp(baseRotation, SafeQuaternion(effector.lerpedRotation), w));
            }
            //BetterDebug.Log($"base Rotation: {baseRotation}");
            
            // Parial effectors
            Vector3 partialPositionOffset = Vector3.zero;
            Quaternion partialRotationOffset = Quaternion.identity;
            float partialFrustumSizeOffset = 0f;
            

            foreach (var partial in PartialCameraEffector.GetAll())
            {
                partial.UpdateEffector(deltaTime);
                //BetterDebug.Log($"Partial: [{partial}, {partial.Weight}]");
     
                float w = partial.Weight;
                if (w <= float.Epsilon) continue;

                float balancedW = Mathf.Lerp(w, 0, Mathf.Clamp01(overridePartialsWeight));
            
                partialPositionOffset += partial.lerpedPosition * balancedW;
            
                Quaternion weightedRot = SafeQuaternion(Quaternion.Slerp(Quaternion.identity, SafeQuaternion(partial.lerpedRotation), balancedW));
                partialRotationOffset = SafeQuaternion(partialRotationOffset * weightedRot);
     
                partialFrustumSizeOffset += partial.lerpedFrustumSize * balancedW;
            }
            //BetterDebug.Log($"Partial Rotation: {partialRotationOffset}");
                        
            Vector3 finalPosition = basePosition + partialPositionOffset;
            Quaternion finalRotation = Quaternion.Normalize(SafeQuaternion(baseRotation) * SafeQuaternion(partialRotationOffset));
            float finalFrustumSize = baseFrustumSize + partialFrustumSizeOffset;
            
            return (finalPosition, finalRotation, finalFrustumSize);
        }

        public void SetRotationSpeed(float speed)
        {
            selfCameraRotation.SetRotationLerpSpeed(speed);
        }
        

        public void SetTargetRotation(float angle)
        {
            selfCameraRotation.targetRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}