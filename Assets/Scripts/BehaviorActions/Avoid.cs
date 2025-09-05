using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Avoid",
        description: "goon",
        category: "Action/WotT",
        story: "[Agent] avoids [Target] for [Time] seconds",
        id: "94872g53jf2425j892450n8924j598"
    )]
    public partial class WotTAvoid : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Time;
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);
        [SerializeReference] public BlackboardVariable<float> AllowedTargetDistanceError = new(8f);

        private float avoidTimer;

        private EntityCharacterBehaviorInputController m_AgentCharacterBehaviorInputController;
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
                LogFailure("No target object assigned.");
                return Status.Failure;
            }

            avoidTimer = Time.Value;

            Initialize();
            UpdateAvoidDirection();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Target.Value == null)
            {
                return Status.Failure;
            }

            avoidTimer -= UnityEngine.Time.deltaTime;
            if (avoidTimer <= 0)
            {
                avoidTimer = 0;
                return Status.Success;
            }
            else
            {
                UpdateAvoidDirection();
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

        private void UpdateAvoidDirection()
        {
            m_AgentCharacterBehaviorInputController.SetDirection(GetAvoidDirection());
        }

        private Vector3 GetAvoidDirection()
        {
            return -(Target.Value.transform.position - Agent.Value.transform.position).normalized;
        }
    }
}
