using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Request Attack Start",
        description: "Request an attack.",
        category: "WotT/Attacking",
        story: "[Agent] requests to attack with attack number [AttackIndex]",
        id: "WotT_Request_Attack_Start"
    )]
    public partial class WotTRequestAttackStart : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<int> AttackIndex = new(0);

        private Attacker m_AgentAttacker;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();

            m_AgentAttacker.RequestAttackStart(AttackIndex.Value);

            return Status.Success;
        }

        protected override void OnDeserialize()
        {
            Initialize();
        }

        private void Initialize()
        {
            m_AgentAttacker = Agent.Value.GetComponentInChildren<Attacker>();
        }
    }
}
