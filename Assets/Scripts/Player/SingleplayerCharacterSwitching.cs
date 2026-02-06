using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class SingleplayerCharacterSwitching : MonoBehaviour, ProjectWideInputActions.IPlayerActions
    {
        [SerializeField] AttackType[] meleeAttacks;
        [SerializeField] AttackType[] rangedAttacks;

        bool currentAttackType;

        private ProjectWideInputActions.PlayerActions playerActions;

        Attacker attacker;

        void Awake()
        {
            attacker = GetComponent<Attacker>();
        }

        void Start()
        {
            if (!NetworkManager.singleton.IsMultiplayer) EnableInput();
        }

        #region Input Setup

        private void EnableInput()
        {
            playerActions = InputManager.Instance.inputActions.Player;

            playerActions.SetCallbacks(this);
        }

        private void DisableInput()
        {
            playerActions.RemoveCallbacks(this);
        }

        public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {

        }

        public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {

        }

        public void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {

        }

        public void OnSwitchCharacter(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {

        }

        #endregion

        void SwitchCharacter()
        {
            if (currentAttackType) 
            {
                attacker.SetAttackTypes(rangedAttacks);
            }
            else 
            {
                attacker.SetAttackTypes(meleeAttacks);
            }
        }
    }
}
