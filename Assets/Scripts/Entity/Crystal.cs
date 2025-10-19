using System;
using System.Collections.Generic;
using Mirror.BouncyCastle.Asn1.Cmp;
using Unity.Mathematics;
using UnityEngine;

namespace DigDig2
{
    public class Crystal : MonoBehaviour
    {
        [SerializeField] GameObject linePrefab;

        [Serializable]
        struct Line
        {
            public GameObject enemy;

            [NonSerialized]
            public GameObject line;
        }
        [SerializeField] List<Line> enemies = new();

        bool hasShield;

        void Awake()
        {
            GetComponent<Attackable>().hit.AddListener(OnHit);
        }

        void OnHit(AttackData data)
        {
            if (hasShield)
            {
                Debug.Log("BONK");
            }
        }

        void Update()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].line == null && enemies[i].enemy == null) continue;
                if (enemies[i].line == null)
                {
                    Line theLineStruct = enemies[i];
                    theLineStruct.line = Instantiate(linePrefab, transform.position, quaternion.identity, transform);
                    enemies[i] = theLineStruct;
                }

                if (enemies[i].enemy == null)
                {
                    Destroy(enemies[i].line);
                    enemies.RemoveAt(i);
                    continue;
                }

                enemies[i].line.GetComponent<CrystalLine>().SetPositions(transform.position, enemies[i].enemy.transform.position);
            }

            if (enemies.Count <= 0 && hasShield)
            {
                GetComponent<Health>().enabled = true;
                hasShield = false;
            }
        }
    }
}
