using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT End Charge",
        description: "Ends the current attack charge action.",
        category: "WotT/Attacking",
        story: "[Agent] ends attack charge",
        id: "WotT_Charge_Attack"
    )]
    public partial class WotTEndCharge : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;
        private Attacker m_AgentAttacker;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();

            m_AgentAttacker.EndAttackCharge();

            return Status.Success;
        }

        protected override void OnDeserialize()
        {
            Initialize();
        }

        private void Initialize()
        {
            m_AgentCharacterBehaviorInputController = Agent.Value.GetComponentInChildren<EntityCharacterBehaviorAgent>();
            m_AgentAttacker = Agent.Value.GetComponentInChildren<Attacker>();
        }
    }
}
