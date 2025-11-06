using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;
using System.Collections.Generic;

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
        private const int MAX_RANDOM_SCAN_TRIES = 10;

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> Radius = new(5f);
        [SerializeReference] public BlackboardVariable Variable;
        [SerializeReference] public BlackboardVariable<List<string>> AttackableFilter = new();
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

            if (AttackableFilter.Value.Count <= 0)
			{
                LogFailure("Attackable filter is empty.");
                return Status.Failure;
			}

            Initialize();

            Collider[] colliders = Physics.OverlapSphere(m_AgentCharacterBehaviorInputController.transform.position, Radius);
            Attackable selectedEnemy = null;
            switch (EnemySelectionPriorityMode.Value)
            {
                case EnemyPriorityMode.Closest:
                    float closestEnemyDistance = -1f;
                    foreach (Collider enemyCollider in colliders)
                    {
                        Attackable enemyAttackable = enemyCollider.GetComponent<Attackable>();
                        if (enemyAttackable == null) continue;
                        if (!AttackableFilter.Value.Contains(enemyAttackable.Group)) continue;
                        if (selectedEnemy == null || Vector3.Distance(enemyCollider.transform.position, m_AgentCharacterBehaviorInputController.transform.position) < closestEnemyDistance)
                        {
                            selectedEnemy = enemyAttackable;
                            closestEnemyDistance = Vector3.Distance(selectedEnemy.transform.position, m_AgentCharacterBehaviorInputController.transform.position);
                        }
                    }

                    break;
                case EnemyPriorityMode.Strongest:
                    LogFailure("Strongest priority mode has not been implemented yet.");
                    return Status.Failure;
                case EnemyPriorityMode.Random:
                    int random_scan_tries = 0;
                    while (true) {
                        random_scan_tries += 1;
                        if (random_scan_tries >= MAX_RANDOM_SCAN_TRIES) break;

                        Attackable candidateAttackable = colliders[UnityEngine.Random.Range(0, colliders.Length - 1)].GetComponent<Attackable>();
                        if (candidateAttackable == null) continue;
                        if (!AttackableFilter.Value.Contains(candidateAttackable.Group)) continue;
                        selectedEnemy = candidateAttackable;
                        break;
					}
                    break;
            }

            Variable.ObjectValue = selectedEnemy;

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
