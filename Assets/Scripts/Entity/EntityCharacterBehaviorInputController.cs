using System;
using UnityEngine;
using UnityEngine.AI;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController))]
    public class EntityCharacterBehaviorAgent : MonoBehaviour
    {
        [Header("Following Path State")]
        [Tooltip("The distance that the entity marks a waypoint as passed.")]
        [SerializeField] private float pathWaypointDistanceTolerance = 1f;

        #region Movement State Variables

        [SerializeField] public MovementState movementState = MovementState.Idle;

        // Following Path
        [NonSerialized] public Vector3 currentDesitation = Vector3.zero;
        [NonSerialized] public Vector3[] currentPathWaypoints = { };
        [NonSerialized] public int currentPathWaypointIndex = 0;
        private NavMeshPath navMeshPath;

        // Following Direction
        [NonSerialized] public Vector3 currentDirection = Vector3.zero;

        #endregion

        #region Visuals State Variables

        private Transform focusedTransform;

        #endregion

        private EntityCharacterController entityCharacterController;

        public enum MovementState
        {
            Idle,
            FollowingPath,
            FollowingDirection,
        }

        private void Awake()
        {
            entityCharacterController = GetComponent<EntityCharacterController>();
        }

        private void Start()
        {
            navMeshPath = new();
        }

        private void Update()
        {
            if (focusedTransform != null)
            {
                Debug.Log("Looker");
                LookTowards(focusedTransform.position);
            }
            
            switch (movementState)
            {
                case MovementState.FollowingPath:
                    if (currentPathWaypoints.Length > 0)
                    {
                        Vector3 currentPathWaypoint = currentPathWaypoints[currentPathWaypointIndex];
                        Vector3 positionDifference = currentPathWaypoint - transform.position;

                        positionDifference.y = 0f;
                        float distanceToWaypoint = positionDifference.magnitude;
                        if (currentPathWaypointIndex >= currentPathWaypoints.Length - 1)
                        {
                            entityCharacterController.inputMoveVector = positionDifference.normalized * Mathf.Min(distanceToWaypoint / (pathWaypointDistanceTolerance + 1f), 1f);
                        }
                        else
                        {
                            entityCharacterController.inputMoveVector = positionDifference.normalized;
                        }

                        if (distanceToWaypoint <= pathWaypointDistanceTolerance)
                        {
                            if (currentPathWaypoints.Length > currentPathWaypointIndex + 1)
                            {
                                // More waypoints to follow, go to next one
                                currentPathWaypointIndex++;
                            }
                            else
                            {
                                // No more waypoints to follow, entity has finished, reset path
                                Stop();
                            }
                        }
                    }

                    break;
                case MovementState.FollowingDirection:
                    entityCharacterController.inputMoveVector = currentDirection;
                    break;
                default:
                    entityCharacterController.inputMoveVector = Vector3.zero;
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!entityCharacterController) return;
            switch (movementState)
            {
                case MovementState.FollowingPath:
                    for (int pathWaypointIndex = currentPathWaypointIndex + 1; pathWaypointIndex < currentPathWaypoints.Length; pathWaypointIndex++)
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(currentPathWaypoints[pathWaypointIndex], 0.25f);
                    }
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(currentPathWaypoints[currentPathWaypointIndex], 0.5f);
                    break;
            }

            Gizmos.DrawLine(transform.position, transform.position + entityCharacterController.inputMoveVector);
        }

        #region State Management

        public void Stop()
        {
            movementState = MovementState.Idle;

            // Reset Following Path State
            currentDesitation = Vector3.zero;
            Array.Clear(currentPathWaypoints, 0, currentPathWaypoints.Length);
            currentPathWaypointIndex = 0;
            navMeshPath.ClearCorners();

            // Reset Follow Direction State
            currentDirection = Vector3.zero;
        }

        public bool SetDestination(Vector3 destination)
        {
            Stop();

            if (navMeshPath == null) return false;
            bool validPathFound = NavMesh.CalculatePath(transform.position, destination, 1, navMeshPath);
            if (!validPathFound) Debug.LogWarning("Path could not be calculated.");
            currentDesitation = destination;
            currentPathWaypoints = navMeshPath.corners;

            movementState = MovementState.FollowingPath;

            return true;
        }

        public bool SetDirection(Vector3 direction)
        {
            Stop();

            currentDirection = direction;

            movementState = MovementState.FollowingDirection;

            return true;
        }

        public bool LookTowards(Vector3 target)
        {
            entityCharacterController.LookTowards(target);

            return true;
        }

        public bool SetFocusedLookTransform(Transform newFocusedTransform)
        {
            focusedTransform = newFocusedTransform;

            return true;
        }

        public bool SetAutomaticLookRotationLock(bool isLocked)
        {
            entityCharacterController.SetAutomaticLookRotationLock(isLocked);

            return true;
        }

        public bool SetSprintMode(bool isSprinting)
        {
            entityCharacterController.isSprinting = isSprinting;

            return true;
        }
        
        #endregion
    }
}
