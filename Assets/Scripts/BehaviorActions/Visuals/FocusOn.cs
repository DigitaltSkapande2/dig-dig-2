using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Focus On",
        description: "Tell the agent to continously look towards a transform.",
        category: "WotT/Visuals",
        story: "[Agent] focuses on [Transform]",
        id: "WotT_Focus_On"
    )]
    public partial class WotTFocusOn : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Transform;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Transform.Value == null)
            {
                LogFailure("No transform assigned.");
                return Status.Failure;
            }

            Initialize();
            m_AgentCharacterBehaviorInputController.SetFocusedLookTransform(Transform.Value);

            return Status.Success;
        }

        protected override Status OnUpdate()
        {
            m_AgentCharacterBehaviorInputController.LookTowards(Transform.Value.position);
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
    }
}
