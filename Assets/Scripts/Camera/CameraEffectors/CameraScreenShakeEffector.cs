using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class ScreenShakeEffector : CameraEffector
    {
        [SerializeField] float ShakeIntensity = 0f;
        [SerializeField] private float shakeFrequency = 5;
        [SerializeField] private float shakeAmplitude = 5;

        private void Update()
        {
            if (ShakeIntensity == 0f) return;

            Vector2 shakeOffset = Vector2.zero;

            // -0.5 to make it center around 0, then *2 to make it -1 to 1
            shakeOffset.x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeAmplitude;
            shakeOffset.y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeAmplitude;

            shakeOffset *= ShakeIntensity;

            rotation = Quaternion.Euler(shakeOffset.y, shakeOffset.x, 0f);

            //position 
            Vector2 offset = UnityEngine.Random.insideUnitCircle * ShakeIntensity;
            position = (GameCamera.Instance.transform.up * offset.y) + (GameCamera.Instance.transform.right * offset.x);

            ShakeIntensity = ShakeIntensity < 0.001f ? 0f : Mathf.Lerp(ShakeIntensity, 0f, Time.deltaTime * 2f);
        }

        public void Shake(float intensity)
        {
            ShakeIntensity += intensity;
        }
    }
}