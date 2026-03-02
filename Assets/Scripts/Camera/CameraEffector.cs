using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace DigDig2.CinemaCamera
{
    public class CameraEffector : MonoBehaviour
    {
        #region Field Declaration
        // -- Static Fields -- //
        private static List<CameraEffector> allCameraEffectors = new();
        private static List<CameraEffector> effectivePivotCameraEffectors = new();
        private static List<CameraEffector> effectiveAbsoluteCameraEffectors = new();

        // -- Instance Fields -- //
        [SerializeField] private int priorityLevel;
        public int PriorityLevel
        {
            get
            {
                return priorityLevel;
            }
            set
            {
                priorityLevel = value;

                ReCompileEffectiveEffectors();
            }
        }
        [SerializeField] public bool overridesLowerPriority = false;
        
        [Header("Debug Visibility")]
        [SerializeField] private bool isActive = true;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (value == isActive) return;

                isActive = value;
                ReCompileEffectiveEffectors();
            }
        }

        protected readonly bool targetCameraPivot = true; 



        public Vector3 position;
        //public Vector2 cameraLocalPlanarOffset;
        public Quaternion rotation;
        public float frustumSize;

        #endregion

        #region  Static Methods

        public static void AddCameraEffector(CameraEffector effector)
        {
            allCameraEffectors.Add(effector);
            ReCompileEffectiveEffectors();
        }

        public static void RemoveCameraEffector(CameraEffector effector)
        {
            allCameraEffectors.Remove(effector);
            ReCompileEffectiveEffectors();
        }

        public static void ReCompileEffectiveEffectors()
        {
            // Find the highest priority level among effectors with any override
            int highestPriority = allCameraEffectors
            .Where(e => e.IsActive && e.overridesLowerPriority)
            .Select(e => e.PriorityLevel)
            .DefaultIfEmpty(0)
            .Max();

            Debug.Log($"Highest Priority: {highestPriority}");

            // Filter effectors with the highest priority and any override
            effectivePivotCameraEffectors = allCameraEffectors
            .Where(e => e.targetCameraPivot && e.IsActive && e.PriorityLevel >= highestPriority)
            .OrderByDescending(e => e.PriorityLevel)
            .ToList();

            effectiveAbsoluteCameraEffectors = allCameraEffectors
            .Where(e => !e.targetCameraPivot && e.IsActive && e.PriorityLevel >= highestPriority)
            .OrderByDescending(e => e.PriorityLevel)
            .ToList();

            Debug.Log($"Effective Pivot Camera Effectors Count: {effectivePivotCameraEffectors.Count}st");
            Debug.Log($"Effective Absolute Camera Effectors Count: {effectiveAbsoluteCameraEffectors.Count}st");
        }

        public static List<CameraEffector> GetEffectivePivotCameraEffectors()
        {
            return effectivePivotCameraEffectors;
        }

        public static List<CameraEffector> GetEffectiveAbsoluteCameraEffectors()
        {
            return effectiveAbsoluteCameraEffectors;
        }

        #endregion

        protected void OnEnable()
        {
            AddCameraEffector(this);
        }

        protected void OnDisable()
        {
            RemoveCameraEffector(this);
        }
    }
}
