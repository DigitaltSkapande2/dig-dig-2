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
        struct CrystalLine
        {
            public GameObject enemy;

            [NonSerialized]
            public GameObject line;
        }
        [SerializeField] CrystalLine[] enemies;

        void Update()
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].line == null && enemies[i].enemy == null) continue;
                if (enemies[i].line == null)
                {
                    enemies[i].line = Instantiate(linePrefab, transform.position, quaternion.identity, transform);
                }

                if (enemies[i].enemy == null)
                {
                    Destroy(enemies[i].line);
                    continue;
                }

                enemies[i].line.GetComponent<LineRenderer>().SetPosition(0, transform.position);
                enemies[i].line.GetComponent<LineRenderer>().SetPosition(1, enemies[i].enemy.transform.position);
            }
        }
    }
}
