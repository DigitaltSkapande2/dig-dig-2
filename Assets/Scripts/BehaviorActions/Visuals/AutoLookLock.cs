using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Set Auto Look Lock",
        description: "Toggle the EntityCharacterController movement look following, where the entity looks in the direction it is moving.",
        category: "WotT/Visuals",
        story: "[Agent] sets auto look lock to [State]",
        id: "WotT_Auto_Look_Lock"
    )]
    public partial class WotTAutoLookLock : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<bool> State = new(false);

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();
            m_AgentCharacterBehaviorInputController.SetAutomaticLookRotationLock(State.Value);

            return Status.Success;
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
