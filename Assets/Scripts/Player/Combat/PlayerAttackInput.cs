
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class PlayerAttackInput : MonoBehaviour
    {
        private bool hasStarted = false;

        private Attacker attacker;

        public bool InputEnabled => inputEnabled;
        private bool inputEnabled = false;



        private void Awake()
        {
            attacker = GetComponent<Attacker>();
        }
		private void Start()
        {
            hasStarted = true;
        }
        
        

        #region Input Action Callbacks

        public void OnAttack1(InputValue context)
        {
            if (context.isPressed) attacker.RequestAttackStart(0);
            else attacker.RequestAttackEnd();
        }
        public void OnAttack2(InputValue context)
        {
            if (context.isPressed) attacker.RequestAttackStart(1);
            else attacker.RequestAttackEnd();
        }

        public void OnFocus(InputValue context)
        {
            if (context.isPressed) attacker.StartFocus();
            else attacker.EndFocus();
        }

        public void OnFocusTarget(InputValue context)
        {
            
        }

        #endregion
    }
}
