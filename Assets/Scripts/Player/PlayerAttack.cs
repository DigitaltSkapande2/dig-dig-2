using Mirror.BouncyCastle.Cms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, GameInputSystem.IAttackActions
{
    [SerializeField] GameObject attackArrow;

    GameObject arrowInstance;

    private GameInputSystem.AttackActions attackActions;

    bool attacking;

    bool frozen;

    Vector2 mousePos;

    Plane playerPlane;

    GameObject attackIndicator;

    void Start()
    {
        EnableInput();
    }

    void OnDisable()
    {
        DisableInput();
    }

    public void SetFrozen(bool value)
    {
        frozen = value;

        if (!value)
        {
            attacking = false;

            if (arrowInstance != null)
            {
                Destroy(arrowInstance);
            }
            
        }
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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (frozen) { return; }

        if (context.started)
        {
            attacking = true;
        }

        if (context.canceled)
        {
            attacking = false;

            if (arrowInstance != null)
            {
                Destroy(arrowInstance);
            }
        }
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        if (frozen) { return; }

        mousePos = context.ReadValue<Vector2>();
    }

    #endregion

    private void Update()
    {
        if (!attacking) { return; }

        playerPlane = new Plane(Vector3.up, transform.position);

        Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        float hitDistance = 0f;

        if (playerPlane.Raycast(mouseRay, out hitDistance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDistance);
            Vector3 attackDirection = (hitPoint - transform.position).normalized * 2;

            if (arrowInstance == null)
            {
                arrowInstance = Instantiate(attackArrow, hitPoint, Quaternion.Euler(0, 0, 0));
            }

            arrowInstance.transform.position = transform.position + attackDirection;
            arrowInstance.transform.LookAt(transform.position + 2 * attackDirection);
        }
    }
}
