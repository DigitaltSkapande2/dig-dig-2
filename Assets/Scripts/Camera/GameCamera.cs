using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2;
using Edgegap.Editor.Api.Models.Results;
using UnityEngine;

namespace DigDig2.CinemaCamera {
    public class GameCamera : MonoBehaviour
    {
        private static List<CameraEffector> allCameraEffectors = new();
        private static List<CameraEffector> effectiveCameraEffectors = new();
        [SerializeField] public List<Transform> targets;
        [Tooltip("the time in seconds it will take for the camera to get to the target position")]
        [SerializeField] public float followSpeed = 0.1f;

        private Vector3 targetPos;
        private Quaternion targetRotation;
        private float targetFrustumSize;

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

        void Update()
        {
            Vector3 targetPos = Vector3.zero;

            foreach (var effector in effectiveCameraEffectors)
            {
                targetPos += effector.position;

            }

            


            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed);
        }
    }
}