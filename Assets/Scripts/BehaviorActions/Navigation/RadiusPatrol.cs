using UnityEngine;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Radius Patrol",
        description: "Randomly patrols the agent around a point with a certain radius.",
        category: "WotT/Navigation",
        story: "[Agent] patrols around [Center] with a max radius of [MaxRadius]",
        id: "WotT_Radius_Patrol"
    )]
    public partial class WotTRadiusPatrol : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [Tooltip("The middle point of the patrol circle/radius.")]
        [SerializeReference] public BlackboardVariable<Vector3> Center;
        [Tooltip("The max distance from the middle point that the agent will patrol to.")]
        [SerializeReference] public BlackboardVariable<float> MaxRadius = new(5f);
        [Tooltip("The min distance from the middle point that the agent will patrol to.")]
        [SerializeReference] public BlackboardVariable<float> MinRadius = new(0.1f);
        [Tooltip("How long the agent will wait after reaching a waypoint to find a new waypoint.")]
        [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new(1.0f);
        [Tooltip("The minimum distance that the new waypoint has to be from the last waypoint to be considered a valid patrol waypoint.")]
        [SerializeReference] public BlackboardVariable<float> AllowedDistanceBetweenLastAndNewPoint = new(1.5f);
        [Tooltip("The maximum amount of tries the agent will attempt to select a new waypoint.")]
        [SerializeReference] public BlackboardVariable<int> MaxPointSelectionAttempts = new(5);

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;
        [CreateProperty] private float m_WaypointWaitTimer;
        [CreateProperty] private bool m_Waiting;
        [CreateProperty] private Vector3 m_LastPoint;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Center.Value == null)
            {
                LogFailure("No center position assigned.");
                return Status.Failure;
            }

            Initialize();

            m_Waiting = false;
            m_WaypointWaitTimer = 0.0f;

            MoveToNextRandomPoint();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null)
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
                    MoveToNextRandomPoint();
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

        private void MoveToNextRandomPoint()
        {
            int targetPointSelectionAttempts = 0;
            Vector3 targetPoint;
            while (true)
            {
                float randomRotation = Random.Range(0, 360);
                float randomRadius = Random.Range(MinRadius.Value, MaxRadius.Value);
                Vector3 directionNormal = new(Mathf.Cos(randomRotation * Mathf.Deg2Rad), 0f, Mathf.Sin(randomRotation * Mathf.Deg2Rad));

                targetPoint = Center.Value + directionNormal * randomRadius;

                if (Vector3.Distance(targetPoint, m_LastPoint) >= AllowedDistanceBetweenLastAndNewPoint.Value) break;

                targetPointSelectionAttempts++;
                if (targetPointSelectionAttempts >= MaxPointSelectionAttempts.Value)
                {
                    Debug.LogWarning("Agent radius patrol reached maximum point selection attempts, please consider lowering the Allowed Distance Between Last And New Point or increase the Max Point Selection Attempts.");
                    break;
                }
            }

            m_AgentCharacterBehaviorInputController.SetDestination(targetPoint);

            m_LastPoint = targetPoint;
        }
    }
}
