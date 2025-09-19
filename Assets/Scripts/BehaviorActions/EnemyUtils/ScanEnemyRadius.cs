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
        [SerializeReference] public BlackboardVariable Variable;
        [SerializeReference] public BlackboardVariable<LayerMask> EnemyLayer;
        [SerializeReference] public BlackboardVariable<EnemyPriorityMode> EnemySelectionPriorityMode;

        public enum EnemyPriorityMode
        {
            Closest,
            Strongest,
            Random,
        }

        private EntityCharacterBehaviorAgent m_AgentCharacterBehaviorInputController;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            Initialize();

            Collider[] foundEnemyColliders = Physics.OverlapSphere(m_AgentCharacterBehaviorInputController.transform.position, Radius, EnemyLayer.Value);
            GameObject selectedEnemy = null;
            switch (EnemySelectionPriorityMode.Value)
            {
                case EnemyPriorityMode.Closest:
                    float closestEnemyDistance = -1f;
                    foreach (Collider enemyCollider in foundEnemyColliders)
                    {
                        if (selectedEnemy == null || Vector3.Distance(enemyCollider.transform.position, m_AgentCharacterBehaviorInputController.transform.position) < closestEnemyDistance)
                        {
                            selectedEnemy = enemyCollider.gameObject;
                            closestEnemyDistance = Vector3.Distance(selectedEnemy.transform.position, m_AgentCharacterBehaviorInputController.transform.position);
                        }
                    }

                    break;
                case EnemyPriorityMode.Strongest:
                    LogFailure("Strongest priority mode has not been implemented yet.");
                    return Status.Failure;
                case EnemyPriorityMode.Random:
                    selectedEnemy = foundEnemyColliders[UnityEngine.Random.Range(0, foundEnemyColliders.Length - 1)].gameObject;
                    break;
            }

            Variable.ObjectValue = new BlackboardVariable<GameObject>(selectedEnemy);

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
