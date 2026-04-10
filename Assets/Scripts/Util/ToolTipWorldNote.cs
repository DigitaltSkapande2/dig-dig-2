using System;
using DigDig2.CinemaCamera;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DigDig2
{
    public class ToolTipWorldNote : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private float speed;
        [SerializeField] private float activationRange = 10f;
        [SerializeField] private float scaleFadeSpeed = 1.5f;

        float timeOffset;
        private Vector3 initialPosition;

        private GameCamera gameCamera;

        private float targetScale = 1;

        void Awake()
        {
            initialPosition = transform.position;
            timeOffset = Random.Range(-1, 1);
        }

        private void Start()
        {
            gameCamera = GameCamera.Instance;
        }

        void Update()
        {
            // Bob
            float cycleValue = Mathf.Sin(Time.time * speed + timeOffset);
            transform.position = initialPosition + Vector3.up * cycleValue * range/2;
            
            // target scale
            if (Vector3.Distance(transform.position, gameCamera.transform.position) <= activationRange) targetScale = 1;
            else targetScale = 0;
            
            // "lerp" scale
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale,
                1f - Mathf.Exp(-scaleFadeSpeed * Time.deltaTime));
        }
    }
}
