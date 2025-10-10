using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Attack",
        description: "Trigger an attack action.",
        category: "WotT/Attacking",
        story: "[Agent] attacks with [Attack] using [Hitbox]",
        id: "WotT_Set_Sprint"
    )]
    public partial class WotTAttack : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<AttackData> Attack;
        [SerializeReference] public BlackboardVariable<Hitbox> Hitbox;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Attack.Value == null)
            {
                LogFailure("No attack assigned.");
                return Status.Failure;
            }

            if (Hitbox.Value == null)
            {
                LogFailure("No hitbox assigned.");
                return Status.Failure;
            }

            Initialize();

            Hitbox.Value.SetAttackData(Attack.Value);

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
