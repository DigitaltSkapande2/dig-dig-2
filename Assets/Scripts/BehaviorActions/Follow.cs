using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Follow",
        description: "goon",
        category: "Action/WotT",
        story: "[Agent] follows [Followed]",
        id: "456798546g9786452978n"
    )]
    public partial class WotTFollow : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Followed;
        [SerializeReference] public BlackboardVariable<float> TargetDistanceToFollowed = new(2f);
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);
        [SerializeReference] public BlackboardVariable<float> AllowedTargetDistanceError = new(8f);

        private EntityCharacterBehaviorInputController m_AgentCharacterBehaviorInputController;
        [CreateProperty] private Vector3 m_CurrentTarget;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Followed.Value == null)
            {
                LogFailure("No followed object assigned.");
                return Status.Failure;
            }

            Initialize();
            UpdateFollowDestination();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Followed.Value == null)
            {
                return Status.Failure;
            }

            float distance = GetDistanceToFollowed();
            bool destinationReached = distance <= DistanceThreshold;
            if (destinationReached)
            {
                return Status.Success;
            }
            else
            {
                float targetDistanceError = GetCurrentTargetDistanceToFollowed();
                
                if (!m_AgentCharacterBehaviorInputController.pathPending || targetDistanceError >= AllowedTargetDistanceError.Value) UpdateFollowDestination();   
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {

        }

        protected override void OnDeserialize()
        {
            
        }

        private void Initialize()
        {
            m_AgentCharacterBehaviorInputController = Agent.Value.GetComponentInChildren<EntityCharacterBehaviorInputController>();
        }

        private void UpdateFollowDestination()
        {
            m_AgentCharacterBehaviorInputController.SetDestination(GetFollowTarget());
        }

        private float GetDistanceToFollowed()
        {
            return (Agent.Value.transform.position - Followed.Value.transform.position).magnitude;
        }

        private float GetCurrentTargetDistanceToFollowed()
        {
            return (m_CurrentTarget - Followed.Value.transform.position).magnitude;
        }

        private Vector3 GetFollowTarget()
        {
            m_CurrentTarget = (Agent.Value.transform.position - Followed.Value.transform.position).normalized * TargetDistanceToFollowed.Value + Followed.Value.transform.position;
            return m_CurrentTarget;
        }
    }
}
