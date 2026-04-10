using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2.Debugging;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigDig2.CinemaCamera
{
    public class PartialCameraEffector : MonoBehaviour
    {
        #region Static Methods

        private static readonly List<PartialCameraEffector> allPartialEffectors = new();

        private static void Register(PartialCameraEffector e)
        {
            allPartialEffectors.Add(e);
            BetterDebug.Log($"Registered: {e.name}");
        }

        private static void Unregister(PartialCameraEffector e)
        {
            allPartialEffectors.Remove(e);
            BetterDebug.Log($"Unregistered: {e.name}");
        }

        public static IReadOnlyList<PartialCameraEffector> GetAll() => allPartialEffectors;
        
        private static Quaternion SafeQuaternion(Quaternion q) => q == default ? Quaternion.identity : q;

        #endregion

        #region Variables

        [SerializeField] private bool startActive = true;
        
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;

        [SerializeField] private float positionLerpSpeed = 8f;
        [SerializeField] private float rotationLerpSpeed = 8f;
        [SerializeField] private float frustumSizeLerpSpeed = 8f;

        [Header("Offsets")]
        public Vector3 targetPosition = Vector3.zero;
        public Quaternion targetRotation = Quaternion.identity; 
        public float targetFrustumSize = 0f;

        public Vector3 lerpedPosition { get; protected set; }
        public Quaternion lerpedRotation { get; protected set; }
        public float lerpedFrustumSize { get; protected set; }
        
        public float Weight { get; private set; }

        private bool isActive;
        private bool targetIsActive; // makes fade direction

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive) return;
                isActive = value;
                targetIsActive = value;
            }
        }

        #endregion

        #region UnityCallbacks

        private void OnEnable()
        {
            isActive = startActive;
            targetIsActive = startActive;
            Weight = startActive ? 1f : 0f;

            if (startActive)
            {
                lerpedPosition = targetPosition;
                lerpedRotation = targetRotation;
                lerpedFrustumSize = targetFrustumSize;
            }

            Register(this);
        }

        private void OnDisable()
        {
            Unregister(this);
        }

        #endregion

        #region Update

        // Gamecamera should call this
        public void UpdateEffector(float deltaTime)
        {
            TickWeight(deltaTime);
            UpdateLerping(deltaTime);
        }

        private void TickWeight(float deltaTime)
        {
            if (targetIsActive)
            {
                Weight = fadeInDuration > 0f
                    ? Mathf.MoveTowards(Weight, 1f, deltaTime / fadeInDuration)
                    : 1f;
            }
            else
            {
                Weight = fadeOutDuration > 0f 
                    ? Mathf.MoveTowards(Weight, 0f, deltaTime / fadeOutDuration)
                    : 0f;
            }
        }

        private void UpdateLerping(float deltaTime)
        {
            lerpedPosition = Vector3.Lerp(lerpedPosition, targetPosition, 1f - Mathf.Exp(-positionLerpSpeed * deltaTime));
            lerpedRotation = Quaternion.Slerp(SafeQuaternion(lerpedRotation), SafeQuaternion(targetRotation), 1f - Mathf.Exp(-rotationLerpSpeed * deltaTime));
            lerpedFrustumSize = Mathf.Lerp(lerpedFrustumSize, targetFrustumSize, 1f - Mathf.Exp(-frustumSizeLerpSpeed * deltaTime));
        }

        #endregion

        
        public void SetActive(bool active) => IsActive = active;
        public void SetPositionLerpSpeed(float newSpeed) => positionLerpSpeed = newSpeed;
        public void SetRotationLerpSpeed(float newSpeed) => rotationLerpSpeed = newSpeed;
        public void SetFrustumSizeLerpSpeed(float newSpeed) => frustumSizeLerpSpeed = newSpeed;
    }
}