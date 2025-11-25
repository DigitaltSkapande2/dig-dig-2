using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Request Attack End",
        description: "Request an attacks end.",
        category: "WotT/Attacking",
        story: "[Agent] requests to end their attack",
        id: "WotT_Request_Attack_end"
    )]
    public partial class WotTCharge : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;

        private Attacker m_AgentAttacker;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();

            m_AgentAttacker.RequestAttackEnd();

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
