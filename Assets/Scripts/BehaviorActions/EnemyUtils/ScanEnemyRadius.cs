using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Scan For Enemy in Radius",
        description: "Scan for enemies around the agent.",
        category: "WotT/Enemy Utils",
        story: "[Agent] scans for an enemy in radius [Radius] and assigns it to [Variable]",
        id: "WotT_Scan_Enemy_Radius"
    )]
    public partial class WotTScanEnemyRadius: Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> Radius = new(5f);
        [SerializeReference] public BlackboardVariable<BlackboardVariable> Variable;

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();
            Debug.LogWarning("ScanEnemyRadius is unfinished.");

            return Status.Failure;
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
