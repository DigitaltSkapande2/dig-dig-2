using System.Collections;
using DigDig2.Debugging;
using UnityEngine;

namespace DigDig2.Entity
{
    public class BlinkDash : Dash
    {
        [SerializeField] private int numberOfFrames = 2;
        [SerializeField] private float dashLenght = 4f;
        [SerializeField] private float dashCooldown = 0.5f;
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private int groundCheckTries = 14;
        [SerializeField] private float blinkYTolerance = 3f;
 
        private float lastTimeDashed = 0f;
        
        public override IEnumerator PerformDash(Vector3 direction, EntityCharacterController entitycontroller)
        {
            entitycontroller.GetVisualsParent().SetActive(false);
            for (int i = 0; i < numberOfFrames; i++)
            {
                yield return null;
            }
            entitycontroller.GetVisualsParent().SetActive(true);

            Ray entityHeightRay = new Ray(entitycontroller.transform.position, Vector3.down);
            float entityheight = 0f;
            if (Physics.Raycast(entityHeightRay, out var hit, groundLayer))
            {
                entityheight = Mathf.Abs(hit.point.y - entitycontroller.transform.position.y);
                BetterDebug.Log($"ENTITY HEIGHT {entityheight}, {hit.point}, {entitycontroller.transform.position}");
            }
            
            Vector3 localTarget = direction * dashLenght;
            Vector3 initialTargetPosition = entitycontroller.transform.position + localTarget;

            Vector3 finalTeleportTarget = entitycontroller.transform.position;

            if (IsValidGround(initialTargetPosition, out var iniPos))
            {
                finalTeleportTarget = iniPos;
                
            }
            else 
            {
                for (int i = groundCheckTries; i > 0; i--)
                {
                    Vector3 target = entitycontroller.transform.position + (localTarget / groundCheckTries) * i;
                    if (IsValidGround(target, out var pos))
                    {
                        finalTeleportTarget = pos + -(localTarget / 6);
                        break;
                    }
                }
            }
            
            
            entitycontroller.Teleport(finalTeleportTarget + new Vector3(0, 10 * entityheight, 0));
            // _TeleportPosPos = finalTeleportTarget + new Vector3(0, 10 * entityheight, 0);
            
            lastTimeDashed = Time.time;
        }

        // private Vector3? _debugGroundPos;
        // private Vector3? _TeleportPosPos;

        private bool IsValidGround(Vector3 targetPosition, out Vector3 groundPos)
        {
            Ray initialGroundCheckRay = new Ray(targetPosition + new Vector3(0, blinkYTolerance, 0), Vector3.down);
    
            if (Physics.Raycast(initialGroundCheckRay, out var hitInfo) &&
                Mathf.Abs(targetPosition.y - hitInfo.point.y) < blinkYTolerance &&
                (groundLayer.value & (1 << hitInfo.collider.gameObject.layer)) != 0)
            {
                groundPos = hitInfo.point;
                // _debugGroundPos = groundPos;
                return true;
            }

            // _debugGroundPos = null;
            groundPos = Vector3.zero;
            return false;
        }

        // private void OnDrawGizmos()
        // {
        //     if (_debugGroundPos.HasValue)
        //     {
        //         Gizmos.color = Color.green;
        //         Gizmos.DrawSphere(_debugGroundPos.Value, 0.4f);
        //         Gizmos.color = Color.mediumVioletRed;
        //         Gizmos.DrawSphere(_TeleportPosPos.Value, 0.4f);
        //     }
        // }

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