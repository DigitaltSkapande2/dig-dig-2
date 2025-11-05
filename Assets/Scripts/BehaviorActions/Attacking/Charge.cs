using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Charge",
        description: "Trigger an attack charge action.",
        category: "WotT/Attacking",
        story: "[Agent] charges attack with attack #[AttackIndex]",
        id: "WotT_Charge_Attack"
    )]
    public partial class WotTCharge : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<int> AttackIndex = new(0);

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

            m_AgentAttacker.ChargeAttack(AttackIndex.Value);

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
