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
        story: "[Agent] patrols around [Transform] with a max radius of [MaxRadius]",
        id: "WotT_Radius_Patrol"
    )]
    public partial class WotTRadiusPatrol : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<float> MaxRadius = new(5f);
        [SerializeReference] public BlackboardVariable<float> MinRadius = new(0.1f);
        [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new(1.0f);
        [SerializeReference] public BlackboardVariable<float> AllowedDistanceBetweenLastAndNewPoint = new(1.5f);
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

            if (Transform.Value == null)
            {
                LogFailure("No center transform assigned.");
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
                    Debug.Log(m_WaypointWaitTimer); 
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

                targetPoint = Transform.Value.position + directionNormal * randomRadius;

                if (Vector3.Distance(targetPoint, m_LastPoint) >= AllowedDistanceBetweenLastAndNewPoint.Value) break;

                targetPointSelectionAttempts++;
                if (targetPointSelectionAttempts >= MaxPointSelectionAttempts.Value) break;
            }

            m_AgentCharacterBehaviorInputController.SetDestination(targetPoint);

            m_LastPoint = targetPoint;
        }
    }
}
