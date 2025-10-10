using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    public class PlayerAttackInput : MonoBehaviour, ProjectWideInputActions.IAttackActions
    {
        private Attacker attacker;

        private ProjectWideInputActions.AttackActions attackActions;

        private Vector2 mousePos;
        private Vector2 joystickVector;



        private void Awake()
        {
            attacker = GetComponent<Attacker>();
        }

		void Start()
        {
            EnableInput();
        }

        #region Input Setup

        private void EnableInput()
        {
            attackActions = InputManager.Instance.inputActions.Attack;

            attackActions.SetCallbacks(this);
        }

        private void DisableInput()
        {
            attackActions.RemoveCallbacks(this);
        }

        #endregion

        #region Inputs

        public void OnAttack1(InputAction.CallbackContext context)
        {
            attacker.Attack(0);
        }
        public void OnAttack2(InputAction.CallbackContext context)
        {
            attacker.Attack(0);
        }

        public void OnMouseAim(InputAction.CallbackContext context)
        {
            mousePos = context.ReadValue<Vector2>();
        }

        public void OnJoystickAim(InputAction.CallbackContext context)
        {
            joystickVector = context.ReadValue<Vector2>();
        }

        #endregion

        /* void Update()
        {
            attackCooldown -= Time.deltaTime;
            HandleRotation();
        }

        void HandleRotation()
        {
            RaycastHit hit;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint((Vector3)mousePos);

            if (aiming && Physics.Raycast(mouseWorldPos, Camera.main.transform.forward, out hit, 100, LayerMask.GetMask("Ground")))
            {
                entityCharacterController.LookTowards(hit.point);
            }
        } */

        #region Attacks

        /* void LightMeleeAttack()
        {
            if (lightMeleeAttacks.Length == 0)
            {
                Debug.LogError("There are no assigned light melee attacks");
                return;
            }

            if (Time.time - lightMeleeInfo.lastAttackTime > lastAttackCooldown + chainMargin) lightMeleeInfo.chainIndex = 0;

            animator.CrossFade(lightMeleeAttacks[lightMeleeInfo.chainIndex].animation.name, crossFadeTransitionTime, 0, 0, 0);
            lightMeleeAttacks[lightMeleeInfo.chainIndex].attackOrigin = transform.position;
            hitbox.SetAttackData(lightMeleeAttacks[lightMeleeInfo.chainIndex]);

            lightMeleeInfo.lastAttackTime = Time.time;
            attackCooldown = lightMeleeAttacks[lightMeleeInfo.chainIndex].cooldown;
            lastAttackCooldown = lightMeleeAttacks[lightMeleeInfo.chainIndex].cooldown;

            if (lightMeleeInfo.chainIndex >= lightMeleeAttacks.Length - 1)
            {
                lightMeleeInfo.chainIndex = 0;
            }
            else
            {
                lightMeleeInfo.chainIndex++;
            }
        }

        void HeavyMeleeAttack()
        {
            if (heavyMeleeAttacks.Length == 0)
            {
                Debug.LogError("There are no assigned heavy melee attacks");
                return;
            }

            if (Time.time - heavyMeleeInfo.lastAttackTime > lastAttackCooldown + chainMargin) heavyMeleeInfo.chainIndex = 0;

            animator.CrossFade(heavyMeleeAttacks[heavyMeleeInfo.chainIndex].animation.name, crossFadeTransitionTime, 0, 0, 0);
            heavyMeleeAttacks[heavyMeleeInfo.chainIndex].attackOrigin = transform.position;
            hitbox.SetAttackData(heavyMeleeAttacks[heavyMeleeInfo.chainIndex]);

            heavyMeleeInfo.lastAttackTime = Time.time;
            attackCooldown = heavyMeleeAttacks[heavyMeleeInfo.chainIndex].cooldown;
            lastAttackCooldown = heavyMeleeAttacks[heavyMeleeInfo.chainIndex].cooldown;

            if (heavyMeleeInfo.chainIndex >= heavyMeleeAttacks.Length - 1)
            {
                heavyMeleeInfo.chainIndex = 0;
            }
            else
            {
                heavyMeleeInfo.chainIndex++;
            }
        }

        void LightRangedAttack()
        {
            if (lightRangedAttacks.Length == 0)
            {
                Debug.LogError("There are no assigned light ranged attacks");
                return;
            }

            if (Time.time - lightRangedInfo.lastAttackTime > lastAttackCooldown + chainMargin) lightRangedInfo.chainIndex = 0;

            animator.CrossFade(lightRangedAttacks[lightRangedInfo.chainIndex].animation.name, crossFadeTransitionTime, 0, 0, 0);
            //Instantiate Projectile

            lightRangedInfo.lastAttackTime = Time.time;
            attackCooldown = lightRangedAttacks[lightRangedInfo.chainIndex].cooldown;
            lastAttackCooldown = lightRangedAttacks[lightRangedInfo.chainIndex].cooldown;

            if (lightRangedInfo.chainIndex >= lightRangedAttacks.Length - 1)
            {
                lightRangedInfo.chainIndex = 0;
            }
            else
            {
                lightRangedInfo.chainIndex++;
            }
        }

        void HeavyRangedAttack(float chargeValue)
        {
            if (heavyRangedAttacks.Length == 0)
            {
                Debug.LogError("There are no assigned heavy ranged attacks");
                return;
            }

            if (Time.time - heavyRangedInfo.lastAttackTime > lastAttackCooldown + chainMargin) heavyRangedInfo.chainIndex = 0;

            animator.CrossFade(heavyRangedAttacks[heavyRangedInfo.chainIndex].animation.name, crossFadeTransitionTime, 0, 0, 0);
            //Instantiate Projectile

            heavyRangedInfo.lastAttackTime = Time.time;
            attackCooldown = heavyRangedAttacks[heavyRangedInfo.chainIndex].cooldown;
            lastAttackCooldown = heavyRangedAttacks[heavyRangedInfo.chainIndex].cooldown;

            if (heavyRangedInfo.chainIndex >= heavyRangedAttacks.Length - 1)
            {
                heavyRangedInfo.chainIndex = 0;
            }
            else
            {
                heavyRangedInfo.chainIndex++;
            }
        } */
        
        #endregion
    }
}
