using UnityEngine;
using Unity.Behavior;
using Unity.Properties;
using System.Collections.Generic;

namespace DigDig2
{
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Waypoint Patrol",
        description: "Patrols the agent along a set of waypoints.",
        category: "WotT/Navigation",
        story: "[Agent] patrols along [Waypoints]",
        id: "WotT_Waypoint_Patrol"
    )]
    public partial class WotTWaypointPatrol : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [Tooltip("The waypoints to patrol around.")]
        [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
        [Tooltip("How long the agent will wait after reaching a waypoint to find a new waypoint.")]
        [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new(1.0f);
        [Tooltip("If the agent should select a waypoint in the list at random, or in order. This will not select the same waypoint that the agent just patrolled to.")]
        [SerializeReference] public BlackboardVariable<bool> RandomizeWaypointSelection = new(false);

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;
        [CreateProperty] private Vector3 m_CurrentTarget;
        [CreateProperty] private float m_WaypointWaitTimer;
        [CreateProperty] private int m_CurrentPatrolPoint = 0;
        [CreateProperty] private bool m_Waiting;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Waypoints.Value == null || Waypoints.Value.Count == 0)
            {
                LogFailure("No waypoints to patrol assigned.");
                return Status.Failure;
            }

            Initialize();

            m_Waiting = false;
            m_WaypointWaitTimer = 0.0f;

            MoveToNextWaypoint();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Waypoints.Value == null)
            {
                return Status.Failure;
            }

            if (m_Waiting)
            {
                if (m_WaypointWaitTimer > 0.0f)
                {
                    m_WaypointWaitTimer -= Time.deltaTime;
                }
                else
                {
                    m_WaypointWaitTimer = 0f;
                    m_Waiting = false;
                    MoveToNextWaypoint();
                }
            }
            else
            {
                // Check if we've reached the waypoint (ensuring NavMeshAgent has completed path calculation if available)
                if (m_AgentCharacterBehaviorInputController == null || m_AgentCharacterBehaviorInputController.movementState == EntityCharacterBehaviorAgent.MovementState.Idle)
                {
                    m_WaypointWaitTimer = WaypointWaitTime.Value;
                    m_Waiting = true;

                    return Status.Running;
                }
            }

            return Status.Running;
        }

        protected override void OnDeserialize()
        {
            Initialize();
        }

        private void Initialize()
        {
            m_AgentCharacterBehaviorInputController = Agent.Value.GetComponentInChildren<EntityCharacterBehaviorAgent>();
        }

        private void MoveToNextWaypoint()
        {
            if (!RandomizeWaypointSelection.Value)
            {
                m_CurrentPatrolPoint = (m_CurrentPatrolPoint + 1) % Waypoints.Value.Count;
            }
            else
            {
                int lastPatrolPoint = m_CurrentPatrolPoint;
                while (lastPatrolPoint == m_CurrentPatrolPoint)
                {
                    m_CurrentPatrolPoint = Random.Range(0, Waypoints.Value.Count - 1);
                }
            }

            m_CurrentTarget = Waypoints.Value[m_CurrentPatrolPoint].transform.position;
            if (m_AgentCharacterBehaviorInputController != null)
            {
                m_AgentCharacterBehaviorInputController.SetDestination(m_CurrentTarget);
            }
        }
    }
}
