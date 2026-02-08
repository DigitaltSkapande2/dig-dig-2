using System;
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
        private static List<CameraEffector> effectiveCameraEffectors = new();

        // -- Instance Fields -- //
        [SerializeField] public bool overridesLowerPriority = false;
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

        public Vector3 position;
        //public Vector2 cameraLocalPlanarOffset;
        public Quaternion rotation;
        public float frustumSize;

        [SerializeField] private int priorityLevel;
        public int PriorityLevel
        {
            get
            {
                return priorityLevel;
            }
            set
            {
                RemoveCameraEffector(this);

                priorityLevel = value;

                AddCameraEffector(this);
            }
        }

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
            .Where(e => e.overridesLowerPriority && e.IsActive)
            .Select(e => e.PriorityLevel)
            .DefaultIfEmpty(0)
            .Max();

            Debug.Log(highestPriority);

            // Filter effectors with the highest priority and any override
            effectiveCameraEffectors = allCameraEffectors
            .Where(e => e.IsActive)
            .Where(e => e.PriorityLevel >= highestPriority)
            .OrderByDescending(e => e.PriorityLevel)
            .ToList();

            Debug.Log($"Effective Effectors Count: {effectiveCameraEffectors.Count}");
        }

        public static List<CameraEffector> GetEffectiveCameraEffectors()
        {
            return effectiveCameraEffectors;
        }

        #endregion

        protected void Start()
        {
            AddCameraEffector(this);
        }

        protected void OnDisable()
        {
            RemoveCameraEffector(this);
        }
    }
}