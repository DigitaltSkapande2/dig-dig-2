using System.Collections;
using UnityEngine;
using DigDig2.Debugging;
using DigDig2.Combat;
using DigDig2.EffectSystem;

namespace DigDig2.Entity
{
    public class RollDash : Dash
    {
        [SerializeField] float dashPower = 5f;
        [SerializeField] float dashDuration = 0.5f;
        [SerializeField] private float dashCooldown = 0.7f;
        [SerializeField] AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] GameObject dashVFX;
        [SerializeField] private EffectPlayer onDashEffect;

        private Vector3 dashVelocity;
        private float lastTimeDashed = 0;

        

        public override IEnumerator PerformDash(Vector3 direction, EntityCharacterController entitycontroller)
        {
            BetterDebug.Log("START Dashing");
            BetterDebug.Log(" Dashing dir " + direction);
            float elapsed = 0f;

            onDashEffect?.Play();
            Instantiate(dashVFX, transform.position - Vector3.up, Quaternion.identity, transform).transform.forward = -direction;
            if (TryGetComponent(out Attackable attackable))
            {
                attackable.SetInvincible(dashDuration);
            }

            while (elapsed < dashDuration)  
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dashDuration);

                float curveValue = dashCurve.Evaluate(t);

                dashVelocity = (entitycontroller.GetMoveSpeed() * direction) + ( curveValue * dashPower * direction) ;

                yield return null;
            }

            BetterDebug.Log("END dash");

            lastTimeDashed = Time.time;
            
            dashVelocity = Vector3.zero;
        }
        

        public override Vector3 GetVelocity()
        {
            return dashVelocity;
        }

        public override bool CanDash()
        {
            return Time.time - lastTimeDashed > dashCooldown;
        }
    }
}