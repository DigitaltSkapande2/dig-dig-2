using System;
using UnityEngine;
using UnityEngine.AI;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController))]
    public class EntityCharacterBehaviorInputController : MonoBehaviour
    {
        [SerializeField] float distanceTolerance = 1f;

        public bool pathPending = false;
        public float remainingDistance = 0f;

        private Vector3 currentDesitation = Vector3.zero;
        private NavMeshPath navMeshPath;
        private Vector3[] currentWaypoints = {};
        private int currentWaypointIndex = 0;

        private EntityCharacterController entityCharacterController;

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
            if (currentWaypoints.Length > 0 && pathPending)
            {
                Vector3 currentWaypoint = currentWaypoints[currentWaypointIndex];
                Vector3 positionDifference = currentWaypoint - transform.position;
                float distanceToWaypoint = positionDifference.magnitude;

                positionDifference.y = 0f;
                entityCharacterController.inputMoveVector = positionDifference.normalized;

                if (distanceToWaypoint <= distanceTolerance)
                {
                    if (currentWaypoints.Length > currentWaypointIndex + 1)
                    {
                        currentWaypointIndex++;
                    }
                    else
                    {
                        pathPending = false;
                        currentDesitation = Vector3.zero;
                        currentWaypointIndex = 0;
                        Array.Clear(currentWaypoints, 0, currentWaypoints.Length);

                        entityCharacterController.inputMoveVector = Vector3.zero;
                    }
                }
            }
            else
            {
                entityCharacterController.inputMoveVector = Vector3.zero;
            }
        }

        public void SetDestination(Vector3 destination)
        {
            currentDesitation = destination;

            if (navMeshPath != null)
            {
                NavMesh.CalculatePath(transform.position, destination, 1, navMeshPath);
                currentWaypoints = navMeshPath.corners;
                currentWaypointIndex = 0;

                pathPending = true;
            }
        }
    }
}
