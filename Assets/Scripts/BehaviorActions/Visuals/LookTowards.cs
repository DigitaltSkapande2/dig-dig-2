using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Look Towards",
        description: "Tell the agent to look towards a point.",
        category: "WotT/Visuals",
        story: "[Agent] looks towards [Point]",
        id: "WotT_Look_Towards"
    )]
    public partial class WotTLookTowards : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> Point;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();
            m_AgentCharacterBehaviorInputController.LookTowards(Point.Value);

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
