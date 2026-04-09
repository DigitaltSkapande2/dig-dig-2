using System;
using System.Collections;
using System.Linq;
using DigDig2.Debugging;
using DigDig2.EffectSystem;
using DigDig2.Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigDig2.Entity
{
    public class BlinkDash : Dash
    {
        [SerializeField] private float blinkDisappearTime = 0.1f;
        [SerializeField] private float dashLenght = 4f;
        [SerializeField] private float dashCooldown = 0.5f;
        [Tooltip("prevents dashing too far in case you try to blink to a lower platform")]
        [SerializeField] private float maxBlinkDistance = 5f;

        [SerializeField] private EffectPlayer blinkStartEffect;
        [SerializeField] private EffectPlayer blinkEndEffect;
        [SerializeField] private float endBlinkEffectTimeOffset;
        [SerializeField] private float effectYOffset;
        
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private int groundCheckTries = 14;
        [SerializeField] private float blinkYTolerance = 3f;
        [SerializeField] private float entityHeight = 0.7f;
        [SerializeField] private float entityRadius = 0.7f;
        [SerializeField] private LayerMask cannotBlinkThroughLayers;
        [SerializeField] private LayerMask triggerLayers;
 
        private float lastTimeDashed = 0f;
        
        public override IEnumerator PerformDash(Vector3 direction, EntityCharacterController entitycontroller)
        {
            entitycontroller.GetVisualsParent().SetActive(false);
            blinkStartEffect.Play(entitycontroller.transform.position + Vector3.up * effectYOffset);
            yield return new WaitForSeconds(blinkDisappearTime + endBlinkEffectTimeOffset);
            
            Vector3 localTarget = direction * dashLenght;
            Vector3 initialTargetPosition = entitycontroller.transform.position + localTarget;

            Vector3 finalTeleportTarget = entitycontroller.transform.position;

            if (IsValidGround(initialTargetPosition, out var iniPos) && 
                CanBlinkFromTo(entitycontroller.transform.position, iniPos))
            {
                finalTeleportTarget = iniPos;
            }
            else 
            {
                for (int i = groundCheckTries; i > 0; i--)
                {
                    Vector3 target = entitycontroller.transform.position + (localTarget / groundCheckTries) * i;
                    if (IsValidGround(target, out var pos)&& 
                        CanBlinkFromTo(entitycontroller.transform.position, pos))
                    {
                        finalTeleportTarget = pos;
                        break;
                    }
                }
            }

            Vector3 teleportVector = finalTeleportTarget - entitycontroller.transform.position;
            RaycastHit[] triggers = Physics.RaycastAll(entitycontroller.transform.position, teleportVector, teleportVector.magnitude, triggerLayers);

            foreach (var trigger in triggers)
            {
                GameplayTrigger gameplayTrigger;
                if (trigger.transform.TryGetComponent<GameplayTrigger>(out gameplayTrigger))
                {
                    gameplayTrigger.triggerEvent.Invoke();
                }
            }

            blinkEndEffect.Play(finalTeleportTarget + Vector3.up * effectYOffset);
            entitycontroller.Teleport(finalTeleportTarget);
            yield return new WaitForSeconds(Mathf.Abs(endBlinkEffectTimeOffset));
            entitycontroller.GetVisualsParent().SetActive(true);
            lastTimeDashed = Time.time;
        }
        

        private bool IsValidGround(Vector3 targetPosition, out Vector3 groundPos)
        {
            Ray initialGroundCheckRay = new Ray(targetPosition + new Vector3(0, 10f, 0), Vector3.down);
    
            if (Physics.Raycast(initialGroundCheckRay, out var hitInfo) &&
                (groundLayer.value & (1 << hitInfo.collider.gameObject.layer)) != 0)
            {
                Vector3 entityPos = hitInfo.point + new Vector3(0, entityHeight, 0);
                var colliders = new Collider[5];
                Physics.OverlapSphereNonAlloc(entityPos, entityRadius, colliders);
                if (colliders.Any())
                {
                    groundPos = entityPos;
                    return true;
                }
            }
            
            groundPos = Vector3.zero;
            return false;
        }



        private bool CanBlinkFromTo(Vector3 startPos, Vector3 endPos)
        {
            Ray ray = new Ray(startPos, endPos - startPos);
            return Mathf.Abs(startPos.y - endPos.y) <= blinkYTolerance && Vector3.Distance(startPos, endPos) <= maxBlinkDistance && !Physics.Raycast(ray, (endPos - startPos).magnitude, cannotBlinkThroughLayers);
        }

        public override Vector3 GetVelocity()
        {
            return Vector3.zero;
        }

        public override bool CanDash()
        {
            return Time.time - lastTimeDashed > dashCooldown;
        }
    }
}