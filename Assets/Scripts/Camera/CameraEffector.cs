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
        public bool isActive;

        [NonSerialized] public Vector3 position;
        [NonSerialized] public Vector3 rotation;
        [NonSerialized] public float frustumSize;

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
            Debug.Log(effectiveCameraEffectors[0]);
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
            .Select(e => e.PriorityLevel)
            .DefaultIfEmpty(0)
            .Max();

            Debug.Log(highestPriority);

            // Filter effectors with the highest priority and any override
            effectiveCameraEffectors = allCameraEffectors
            .Where(e => e.PriorityLevel == highestPriority)
            .OrderByDescending(e => e.PriorityLevel)
            .ToList();
        }

        public static List<CameraEffector> GetEffectiveCameraEffectors()
        {
            return effectiveCameraEffectors;
        }

        #endregion

        private void Start()
        {
            AddCameraEffector(this);
        }
    }
}