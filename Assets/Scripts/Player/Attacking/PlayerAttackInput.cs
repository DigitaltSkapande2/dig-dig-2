using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace DigDig2
{
    public class PlayerAttackInput : MonoBehaviour, GameInputSystem.IAttackActions
    {
        private GameInputSystem.AttackActions attackActions;
        private Animator animator;
        private EntityCharacterController entityCharacterController;

        private Hitbox hitbox;

        private struct AttackInfo
        {
            public int chainIndex;
            public float lastAttackTime;
        }

        enum AttackType
        {
            Melee,
            Ranged
        }

        [SerializeField] AttackType attackType;
        [SerializeField] float chainMargin;

        [SerializeField] AttackData[] lightMeleeAttacks;
        [SerializeField] AttackData[] heavyMeleeAttacks;
        [SerializeField] AttackData[] lightRangedAttacks;
        [SerializeField] AttackData[] heavyRangedAttacks;

        Vector2 mousePos;
        Vector2 joystickVector;

        float attackCooldown;

        AttackInfo lightMeleeInfo;
        AttackInfo heavyMeleeInfo;
        AttackInfo lightRangedInfo;
        AttackInfo heavyRangedInfo;

        bool aiming;
        bool rangedAttackCharging;
        float chargeStartedTime;

        void Awake()
        {
            animator = GetComponent<Animator>();
            entityCharacterController = transform.parent.GetComponent<EntityCharacterController>();
        }

        void Start()
        {
            hitbox = GetComponentInChildren<Hitbox>();
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
            else if (attackType == AttackType.Ranged && context.canceled)
            {
                aiming = false;
            }
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

        void Update()
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
        }

        void LightMeleeAttack()
        {
            if (lightMeleeAttacks.Length == 0)
            {
                Debug.LogError("There are no assigned light melee attacks");
                return;
            }
            if (Time.time - lightMeleeInfo.lastAttackTime > attackCooldown + chainMargin) lightMeleeInfo.chainIndex = 0;
            
            Debug.Log(lightMeleeInfo.chainIndex);
            lightMeleeInfo.lastAttackTime = Time.time;
            attackCooldown = lightMeleeAttacks[lightMeleeInfo.chainIndex].cooldown;

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
            if (Time.time - heavyMeleeInfo.lastAttackTime > attackCooldown + chainMargin) heavyMeleeInfo.chainIndex = 0;
            
            Debug.Log(heavyMeleeInfo.chainIndex);
            heavyMeleeInfo.lastAttackTime = Time.time;
            attackCooldown = lightMeleeAttacks[heavyMeleeInfo.chainIndex].cooldown;

            if (heavyMeleeInfo.chainIndex >= lightMeleeAttacks.Length - 1)
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

        }

        void HeavyRangedAttack(float chargeValue)
        {

        }
    }
}
