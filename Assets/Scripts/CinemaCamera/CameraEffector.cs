using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2.Debugging;
using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class CameraEffector : MonoBehaviour
    {
        #region Static

        private static readonly List<CameraEffector> allEffectors = new();
        private static CameraEffector currentlyEffectiveEffector;

        private static void Register(CameraEffector effector)
        {
            allEffectors.Add(effector);
            BetterDebug.Log($"Registered: {effector.name}");
            RecalculateCurrentEffector();
        }

        private static void Unregister(CameraEffector effector)
        {
            allEffectors.Remove(effector);
            BetterDebug.Log($"Unregistered: {effector.name}");
            RecalculateCurrentEffector();
        }

        private static void RecalculateCurrentEffector()
        {
            CameraEffector next = allEffectors
                .Where(e => e.isActive)
                .OrderByDescending(e => e.priorityLevel)
                .ThenByDescending(e => e.activationOrder)
                .FirstOrDefault();

            if (next == currentlyEffectiveEffector) return;

            currentlyEffectiveEffector = next;

            foreach (var e in allEffectors)
            {
                bool isNowTarget = (e == currentlyEffectiveEffector);

                if (isNowTarget != e.targetIsCurrentEffector)
                {
                    e.fadeElapsed = 0f;
                    
                    if (isNowTarget) e.Weight = 1f;
                }

                e.targetIsCurrentEffector = isNowTarget;
            }

            BetterDebug.Log($"Current effector: {currentlyEffectiveEffector?.name ?? "none"}");
        }

        public static CameraEffector GetCurrentEffector() => currentlyEffectiveEffector;
        public static IEnumerable<CameraEffector> GetAll() => allEffectors;

        private static int activationCounter = 0;

        private static Quaternion SafeQuaternion(Quaternion q) =>
            q == default ? Quaternion.identity : Quaternion.Normalize(q);

        #endregion

        #region Fields

        [SerializeField] private bool startActive = true;
        [SerializeField] private int priorityLevel;
        [SerializeField] public bool overridePartials = false;

        public int PriorityLevel
        {
            get => priorityLevel;
            set { priorityLevel = value; RecalculateCurrentEffector(); }
        }
        
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float fadeOutDuration = 0.5f;

        [SerializeField] private float positionLerpSpeed = 5f;
        [SerializeField] private float rotationLerpSpeed = 5f;
        [SerializeField] private float frustumSizeLerpSpeed = 5f;

        [Header("Target Values")]
        public Vector3 targetPosition = Vector3.zero;
        public Quaternion targetRotation = Quaternion.identity;
        public float targetFrustumSize = 5f;

        public Vector3 lerpedPosition { get; private set; }
        public Quaternion lerpedRotation { get; private set; }
        public float lerpedFrustumSize { get; private set; }

        public float Weight;

        public bool isActive;
        public bool targetIsCurrentEffector;
        public int  activationOrder;

        private float fadeElapsed;

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive) return;
                isActive = value;
                if (isActive) activationOrder = ++activationCounter;
                RecalculateCurrentEffector();
            }
        }

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            isActive = startActive;

            if (startActive)
            {
                activationOrder = ++activationCounter;
                lerpedPosition = targetPosition;
                lerpedRotation = SafeQuaternion(targetRotation);
                lerpedFrustumSize = targetFrustumSize;
                Weight = 1f;
            }
            else
            {
                Weight = 0f;
            }

            Register(this);
        }

        protected virtual void OnDisable()
        {
            Unregister(this);
        }

        private void Reset()
        {
            targetRotation = Quaternion.identity;
        }

        #endregion

        #region Update

        public void UpdateEffector(float deltaTime)
        {
            TickWeight(deltaTime);
            UpdateLerping(deltaTime);
        }

        private void TickWeight(float deltaTime)
        {
            if (targetIsCurrentEffector) return;

            fadeElapsed += deltaTime;
            float t = fadeOutDuration > 0f
                ? Mathf.Clamp01(fadeElapsed / fadeOutDuration)
                : 1f;

            Weight = fadeOutCurve.Evaluate(t);
        }

        private void UpdateLerping(float deltaTime)
        {
            float pt = 1f - Mathf.Exp(-positionLerpSpeed * deltaTime);
            float rt = 1f - Mathf.Exp(-rotationLerpSpeed * deltaTime);
            float ft = 1f - Mathf.Exp(-frustumSizeLerpSpeed * deltaTime);

            lerpedPosition = Vector3.Lerp(lerpedPosition, targetPosition, pt);
            lerpedRotation = Quaternion.Slerp(SafeQuaternion(lerpedRotation), SafeQuaternion(targetRotation), rt);
            lerpedFrustumSize = Mathf.Lerp(lerpedFrustumSize, targetFrustumSize, ft);
        }

        #endregion

        public void SetActive(bool active) => IsActive = active;
    }
}