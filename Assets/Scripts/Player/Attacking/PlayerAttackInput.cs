using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    public class PlayerAttackInput : MonoBehaviour, GameInputSystem.IAttackActions
    {
        private GameInputSystem.AttackActions attackActions;
        private Animator animator;

        enum AttackType
        {
            Melee,
            Ranged
        }

        [SerializeField] AttackType attackType;

        Vector2 mousePos;

        float attackCooldown;

        bool aiming;
        bool rangedAttackCharging;
        float chargeStartedTime;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            EnableInput();
        }

        #region Input Setup

        private void EnableInput()
        {
            attackActions = GameInputManager.Instance.gameInputSystem.Attack;

            attackActions.SetCallbacks(this);
            attackActions.Enable();
        }

        private void DisableInput()
        {
            attackActions.Disable();
        }

        #endregion

        #region Inputs

        public void OnAttack1(InputAction.CallbackContext context)
        {
            if (attackType == AttackType.Melee && context.started && !(attackCooldown > 0))
            {
                Debug.Log("goon");
                LightMeleeAttack();
            }

            else if (attackType == AttackType.Ranged && context.started && !(attackCooldown > 0))
            {
                if (aiming)
                {
                    chargeStartedTime = Time.time;
                    rangedAttackCharging = true;
                }
                else
                {
                    LightRangedAttack();
                }
            }

            else if (attackType == AttackType.Ranged && context.canceled && aiming && rangedAttackCharging)
            {
                rangedAttackCharging = false;
                HeavyRangedAttack(Time.time - chargeStartedTime);
            }
        }
        public void OnAttack2(InputAction.CallbackContext context)
        {
            if (attackType == AttackType.Melee && context.started && !(attackCooldown > 0))
            {
                HeavyMeleeAttack();
                return;
            }

            else if (attackType == AttackType.Ranged && context.started)
            {
                aiming = true;
            }
            else
            {
                aiming = false;
            }
        }

        public void OnMouse(InputAction.CallbackContext context)
        {
            mousePos = context.ReadValue<Vector2>();
        }

        #endregion

        void LightMeleeAttack()
        {
            
        }

        void HeavyMeleeAttack()
        {

        }
        
        void LightRangedAttack()
        {

        }

        void HeavyRangedAttack(float chargeValue)
        {
            
        }
    }
}
