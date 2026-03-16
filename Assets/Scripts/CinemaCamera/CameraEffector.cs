using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2.Debugging;
using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class CameraEffector : MonoBehaviour
    {
        #region Field Declaration
        /// <summary>All active CameraEffector COMPONENTS</summary>> 
        private static readonly List<CameraEffector> allCameraEffectors = new(); 
        /// <summary>All CameraEffectors with active influence right now</summary>> 
        private static List<CameraEffector> effectivePivotCameraEffectors = new();

        [SerializeField] private bool startActive = true;
        [SerializeField] private int priorityLevel;
        public int PriorityLevel
        {
            get => priorityLevel;
            set
            {
                priorityLevel = value; 
                ReCompileEffectiveEffectors();
            }
        }

        [SerializeField] public bool overridesLowerPriority;

        [Tooltip("How fast this effector fades in/out (units per second, 1/x = seconds to full weight)")]
        [SerializeField] private float transitionFadeDuration = 3f;

        [SerializeField] private float positionLerpSpeed = 1f;
        [SerializeField] private float rotationLerpSpeed = 1f;
        [SerializeField] private float frustumSizeLerpSpeed = 1f;
        
        [Header("StartPos")]
        public Vector3 targetPosition;
        public Vector3 lerpedPosition { get; protected set; }
        
        public Quaternion targetRotation = Quaternion.identity;
        public Quaternion lerpedRotation { get; protected set; }
        
        public float targetFrustumSize;
        public float lerpedFrustumSize { get; protected set; }
        
        
        private bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive) return;
                activeChangeTime = Time.time;
                isActive = value;
                ReCompileEffectiveEffectors();
            }
        }

        private float activeChangeTime;

        // 0 = full effect, 1= no effect
        [NonSerialized] public float Weight = 0f;

        [SerializeField] private AnimationCurve easeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        #endregion

        #region Static Methods

        private static void AddCameraEffector(CameraEffector effector)
        {
            BetterDebug.Log($"Adding Effector: {effector.name}");
            allCameraEffectors.Add(effector);
            ReCompileEffectiveEffectors();
        }
        
        private static void RemoveCameraEffector(CameraEffector effector)
        {
            BetterDebug.Log($"Removing Effector: {effector.name}");
            allCameraEffectors.Remove(effector);
            ReCompileEffectiveEffectors();
        }

        private static void ReCompileEffectiveEffectors()
        {
            int cutOffPriority = allCameraEffectors
                .Where(e => e.isActive && e.overridesLowerPriority)
                .Select(e => e.PriorityLevel)
                .DefaultIfEmpty(0)
                .Max();
            
            effectivePivotCameraEffectors = allCameraEffectors
                .Where(effector => (effector.isActive && effector.PriorityLevel >= cutOffPriority) || effector.Weight > float.Epsilon)
                .OrderByDescending(effector => effector.PriorityLevel)
                .ToList();
            
            BetterDebug.Log($"Recompiled effective effectors, cutofPriority: {cutOffPriority}, effectors {String.Join(", \n", effectivePivotCameraEffectors.Select(e => e.name))}");
            
        }

        public static List<CameraEffector> GetEffectiveEffectors() => effectivePivotCameraEffectors;

        #endregion

        // Called by the GameCamera so all effectors get updated before the camera updates.
        public void UpdateEffector(float deltaTime)
        {
            TickWeight(deltaTime);
            UpdateLerping(deltaTime);
        }
        
        public void TickWeight(float deltaTime)
        {
            var targetCurve = isActive ? easeInCurve : easeOutCurve;
            
            Weight = targetCurve.Evaluate((Time.time - activeChangeTime) / transitionFadeDuration);

            // remove if fully faded out
            if (!isActive && Weight <= float.Epsilon && allCameraEffectors.Contains(this))
            {
                ReCompileEffectiveEffectors();
            }
        }

        protected virtual void UpdateLerping(float deltaTime)
        {
            lerpedPosition = Vector3.Lerp(lerpedPosition, targetPosition,1f - Mathf.Exp(-positionLerpSpeed * deltaTime));
            lerpedRotation = Quaternion.Slerp(lerpedRotation, targetRotation,1f - Mathf.Exp(-rotationLerpSpeed * deltaTime));
            lerpedFrustumSize = Mathf.Lerp(lerpedFrustumSize, targetFrustumSize,1f - Mathf.Exp(-frustumSizeLerpSpeed * deltaTime));
        }

        protected void OnEnable()
        {
            isActive = startActive;
            if (startActive)
            {
                Weight = 1f;
                lerpedPosition = targetPosition;
                lerpedRotation = targetRotation;
                lerpedFrustumSize = targetFrustumSize;
            }
            else
            {
                lerpedRotation = Quaternion.identity;
                targetRotation = Quaternion.identity;
            }
            
            AddCameraEffector(this); 
        }

        public void SetRotationSpeed(float speed)
        {
            rotationLerpSpeed = speed;
        }

        protected virtual void OnDisable()
        {
            RemoveCameraEffector(this);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
        }
    }
}