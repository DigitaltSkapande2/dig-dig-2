using DigDig2.Effects;
using UnityEngine;

namespace DigDig2
{
    public class OceanFollow : MonoBehaviour
    {

        [Tooltip("the object to follow")]
        [SerializeField] private Transform target;
        [Tooltip("the interval at witch to snap to. The sice of one grid piece")]
        [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
        [SerializeField] private EffectPlayer onWaterLowerEffect;

        [SerializeField] float verticalSpeed;

        private float targetY;

        public void LowerWater(float amount)
        {
            targetY -= amount;
            onWaterLowerEffect.Play();
        }

        void Start()
        {
            targetY = transform.position.y;
        }

        void Update()
        {
            if (target == null) return;

                Vector3 newPosition = new Vector3(
                    Mathf.Round(target.position.x / gridSize.x) * gridSize.x,
                    Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * verticalSpeed),
                    Mathf.Round(target.position.z / gridSize.y) * gridSize.y
                );
                transform.position = newPosition;
        }
    }
}
