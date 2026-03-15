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

        private static readonly List<CameraEffector> allCameraEffectors = new();
        // Now stores ALL active effectors; weight handles blending
        private static List<CameraEffector> effectivePivotCameraEffectors = new();
        private static List<CameraEffector> effectiveAbsoluteCameraEffectors = new();

        [SerializeField] private bool startActive;
        [SerializeField] private int priorityLevel;
        public int PriorityLevel
        {
            get => priorityLevel;
            set { priorityLevel = value; ReCompileEffectiveEffectors(); }
        }

        [SerializeField] public bool overridesLowerPriority;

        [Tooltip("How fast this effector fades in/out (units per second, 1/x = seconds to full weight)")]
        [SerializeField] private float transitionSpeed = 3f;

        [Header("Debug Visibility")]
        [SerializeField] private bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive) return;
                isActive = value;
                ReCompileEffectiveEffectors();
            }
        }

        // 0 = full effect, 1= no effect
        [NonSerialized] public float Weight = 0f;

        // Whether this effectors weight should tween towards 1 or 0
        private bool targetWeight = false;

        public Vector3 position;
        public Quaternion rotation;
        public float frustumSize;

        #endregion

        #region Static Methods

        public static void AddCameraEffector(CameraEffector effector)
        {
            allCameraEffectors.Add(effector);
            ReCompileEffectiveEffectors();
        }

        public static void RemoveCameraEffector(CameraEffector effector)
        {
            effector.targetWeight = false;
            ReCompileEffectiveEffectors();
        }

        public static void ReCompileEffectiveEffectors()
        {
            int highestPriority = allCameraEffectors
                .Where(e => e.isActive && e.overridesLowerPriority)
                .Select(e => e.PriorityLevel)
                .DefaultIfEmpty(0)
                .Max();
            
            foreach (var effector in allCameraEffectors)
            {
                bool shouldBeActive = effector.isActive && effector.PriorityLevel >= highestPriority;
                effector.targetWeight = shouldBeActive;
            }
            
            effectivePivotCameraEffectors = allCameraEffectors
                .Where(effector => effector.targetWeight || effector.Weight > float.Epsilon)
                .OrderByDescending(effector => effector.PriorityLevel)
                .ToList();
        }

        public static List<CameraEffector> GetEffectiveEffectors() => effectivePivotCameraEffectors;
        public static List<CameraEffector> GetEffectiveAbsoluteCameraEffectors() => effectiveAbsoluteCameraEffectors;

        #endregion
        
        public void TickWeight(float deltaTime)
        {
            float target = targetWeight ? 1f : 0f;
            Weight = Mathf.Lerp(Weight, target, 1f - Mathf.Exp(-transitionSpeed * deltaTime));

            // remove if fully faded out
            if (!targetWeight && Weight <= float.Epsilon && allCameraEffectors.Contains(this))
            {
                Debug.Log("BIDEN SUCKS DIH");
                BetterDebug.Log(Weight -target);
                ReCompileEffectiveEffectors();
            }
        }

        // private void Update()
        // {
        //     TickWeight(Time.deltaTime);
        // }

        protected void Awake() { Weight = startActive ? 1f : 0f; AddCameraEffector(this); }

        protected void OnDisable() { RemoveCameraEffector(this); }
    }
}