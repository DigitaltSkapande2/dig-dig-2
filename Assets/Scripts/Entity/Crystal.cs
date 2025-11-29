using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DigDig2
{
	[RequireComponent(typeof(Attackable), typeof(Health))]
	public class Crystal : MonoBehaviour
    {
        [Serializable]
        private struct EnemyConnections
        {
            public GameObject enemy;
            [NonSerialized] public GameObject lineDrawer;
            [NonSerialized] public CrystalLine lineComponent;
        }
        [Tooltip("Enemies connected to the crystal.")]
        [SerializeField] private List<EnemyConnections> enemyConnections = new();

        [Tooltip("The crystal visual GameObject.")]
        [SerializeField] private GameObject crystal;

        [Tooltip("The shield visual GameObject.")]
        [SerializeField] private GameObject shield;

        [Tooltip("The line prefab.")]
        [SerializeField] private GameObject linePrefab;

        private bool hasShield = true;

        private Attackable attackable;
        private Health health;
        private MeshRenderer crystalMeshRenderer;



        private void Awake()
        {
            attackable = GetComponent<Attackable>();
            health = GetComponent<Health>();

            attackable.hit.AddListener(OnHit);
        }

		private void Start()
        {
            if (crystal == null) { Debug.LogError("Crystal has not been assigned."); return; }
            crystalMeshRenderer = crystal.GetComponent<MeshRenderer>();
		}

		private void OnHit()
        {
            if (hasShield) return;

            Material material = crystalMeshRenderer.material;

            if (health.HealthPoints == 1) material.SetFloat("_CrackStage", 0.001f);
            else if (health.HealthPoints == 2) material.SetFloat("_CrackStage", 0.015f);
        }

        private void Update()
        {
            shield.SetActive(hasShield);
            health.enabled = !hasShield;

            for (int index = enemyConnections.Count - 1; index >= 0; index--)
            {
                EnemyConnections enemyConnection = enemyConnections[index];
                if (enemyConnection.lineDrawer == null && enemyConnection.enemy == null) continue;
                if (enemyConnection.lineDrawer == null)
                {
                    enemyConnection.lineDrawer = Instantiate(linePrefab, transform.position, quaternion.identity, transform);
                    enemyConnection.lineComponent = enemyConnection.lineDrawer.GetComponent<CrystalLine>();
                    enemyConnections[index] = enemyConnection;
                }

                if (enemyConnection.enemy == null)
                {
                    Destroy(enemyConnection.lineDrawer);
                    enemyConnections.RemoveAt(index);

                    if (enemyConnections.Count <= 0) hasShield = false;

                    continue;
                }

                enemyConnection.lineComponent.SetPositions(transform.position, enemyConnection.enemy.transform.position);
            }
        }

		private void OnDrawGizmosSelected()
		{
			foreach (EnemyConnections enemyConnection in enemyConnections)
            {
                Gizmos.color = Color.pink;
                Gizmos.DrawLine(transform.position, enemyConnection.enemy.transform.position);
			}
		}
	}
}
