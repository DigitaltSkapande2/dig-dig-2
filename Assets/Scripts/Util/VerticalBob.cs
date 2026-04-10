using UnityEngine;

namespace DigDig2
{
    public class VerticalBob : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private float speed;

        float timeOffset;
        private Vector3 initialPosition;

        void Awake()
        {
            initialPosition = transform.position;
            timeOffset = Random.Range(-1, 1);
        }

        void Update()
        {
            float cycleValue = Mathf.Sin(Time.time * speed + timeOffset);
            transform.position = initialPosition + Vector3.up * cycleValue * range/2;
        }
    }
}
