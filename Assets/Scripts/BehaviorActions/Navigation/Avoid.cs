using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Avoid",
        description: "Runs away from the target for a certain amount of seconds.",
        category: "WotT/Navigation",
        story: "[Agent] avoids [Target] for [Time] seconds",
        id: "WotT_Avoid"
    )]
    public partial class WotTAvoid : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Time;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;
        [CreateProperty] private float avoidTimer;

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
                m_AgentCharacterBehaviorInputController.Stop();
                return Status.Success;
            }
            else
            {
                UpdateAvoidDirection();
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
