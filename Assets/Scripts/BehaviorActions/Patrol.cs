using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;
using System.Collections.Generic;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Patrol",
        description: "goon",
        category: "Action/WotT",
        story: "[Agent] patrols along [Waypoints]",
        id: "5b4m09854m06m776m7"
    )]
    public partial class WotTPatrol : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
        [SerializeReference] public BlackboardVariable<float> Speed = new (3f);
        [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new (1.0f);
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new (0.2f);
        [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new ("SpeedMagnitude");
        [Tooltip("Should patrol restart from the latest point?")]
        [SerializeReference] public BlackboardVariable<bool> PreserveLatestPatrolPoint = new (false);

        private EntityCharacterBehaviorInputController m_AgentCharacterBehaviorInputController;
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
                float distance = GetDistanceToWaypoint();
                bool destinationReached = distance <= DistanceThreshold;

                // Check if we've reached the waypoint (ensuring NavMeshAgent has completed path calculation if available)
                if (destinationReached && (m_AgentCharacterBehaviorInputController == null || !m_AgentCharacterBehaviorInputController.pathPending))
                {
                    m_WaypointWaitTimer = WaypointWaitTime.Value;
                    m_Waiting = true;

                    return Status.Running;
                }
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (m_AgentCharacterBehaviorInputController != null)
            {
                // Why did unity do this and what does it mean? Does he snow?
                // if (m_AgentCharacterBehaviorInputController.isOnNavMesh)
                // {
                //     m_AgentCharacterBehaviorInputController.ResetPath();
                // }
            }
        }

        protected override void OnDeserialize()
        {
            int patrolPoint = m_CurrentPatrolPoint - 1;
            Initialize();
            // During deserialization, bypass PreserveLatestPatrolPoint.
            m_CurrentPatrolPoint = patrolPoint;
        }

        private void Initialize()
        {
            m_AgentCharacterBehaviorInputController = Agent.Value.GetComponentInChildren<EntityCharacterBehaviorInputController>();
            if (m_AgentCharacterBehaviorInputController != null)
            {
                // if (m_AgentCharacterBehaviorInputController.isOnNavMesh)
                // {
                //     m_AgentCharacterBehaviorInputController.ResetPath();
                // }

                // BH Tree should be able to control the entity's speed, like what is done here
                // m_OriginalSpeed = m_AgentCharacterBehaviorInputController.speed;
                // m_AgentCharacterBehaviorInputController.speed = Speed.Value;
                // m_OriginalStoppingDistance = m_AgentCharacterBehaviorInputController.stoppingDistance;
                // m_AgentCharacterBehaviorInputController.stoppingDistance = DistanceThreshold;
            }

            m_CurrentPatrolPoint = PreserveLatestPatrolPoint.Value ? m_CurrentPatrolPoint - 1 : -1;
        }

        private float GetDistanceToWaypoint()
        {
            if (m_AgentCharacterBehaviorInputController != null)
            {
                return m_AgentCharacterBehaviorInputController.remainingDistance;
            }

            Vector3 targetPosition = m_CurrentTarget;
            Vector3 agentPosition = Agent.Value.transform.position;
            agentPosition.y = targetPosition.y; // Ignore y for distance check.
            return Vector3.Distance(agentPosition, targetPosition);
        }

        private void MoveToNextWaypoint()
        {
            m_CurrentPatrolPoint = (m_CurrentPatrolPoint + 1) % Waypoints.Value.Count;            

            m_CurrentTarget = Waypoints.Value[m_CurrentPatrolPoint].transform.position;
            if (m_AgentCharacterBehaviorInputController != null)
            {
                m_AgentCharacterBehaviorInputController.SetDestination(m_CurrentTarget);
            }
        }
    }
}
