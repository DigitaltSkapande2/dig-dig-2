using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Follow",
        description: "Follows target until it's reached.",
        category: "WotT/Navigation",
        story: "[Agent] follows [Target]",
        id: "WotT_Follow"
    )]
    public partial class WotTFollow : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> FollowDistance = new(2f);
        [Tooltip("The max allowed distance from the navigation target to the actual target. When distance is above this number the path gets recalculated.")]
        [SerializeReference] public BlackboardVariable<float> AllowedTargetErrorDistance = new(8f);
        [SerializeReference] public BlackboardVariable<float> DistanceTolerance = new(1f);

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;
        [CreateProperty] private Vector3 m_CurrentTarget;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Target.Value == null)
            {
                LogFailure("No target to follow.");
                return Status.Failure;
            }

            Initialize();
            UpdateFollowDestination();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Target.Value == null)
            {
                return Status.Failure;
            }

            GetFollowTarget();
            if (GetDistanceToCurrentTarget() <= DistanceTolerance.Value)
            {
                return Status.Success;
            }

            if (m_AgentCharacterBehaviorInputController.movementState == EntityCharacterBehaviorAgent.MovementState.Idle)
            {
                return Status.Success;
            }
            else
            {
                float targetDistanceError = GetCurrentTargetDistanceToFollowedTarget();
                if (targetDistanceError >= AllowedTargetErrorDistance.Value) UpdateFollowDestination();
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

        private void UpdateFollowDestination()
        {
            m_AgentCharacterBehaviorInputController.SetDestination(GetFollowTarget());
        }

        private float GetCurrentTargetDistanceToFollowedTarget()
        {
            return (m_CurrentTarget - Target.Value.transform.position).magnitude;
        }

        private float GetDistanceToCurrentTarget() {
            return (m_CurrentTarget - Agent.Value.transform.position).magnitude;
        }

        private Vector3 GetFollowTarget()
        {
            m_CurrentTarget = (Agent.Value.transform.position - Target.Value.transform.position).normalized * FollowDistance.Value + Target.Value.transform.position;
            return m_CurrentTarget;
        }
    }
}
