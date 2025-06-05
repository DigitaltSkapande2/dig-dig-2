using Mirror.BouncyCastle.Cms;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, GameInputSystem.IAttackActions
{
    private GameInputSystem.AttackActions attackActions;
    bool attacking;
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
        if (context.started)
        {
            attacking = true;
        }

        if (context.canceled)
        {
            attacking = false;

            Destroy(attackIndicator);
        }
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    #endregion

    private void Update()
    {
        if (!attacking) { return; }

        if (attackIndicator == null)
        {
            attackIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(attackIndicator.GetComponent<SphereCollider>());
        }

        playerPlane = new Plane(Vector3.up, transform.position);

        Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        float hitDistance = 0f;

        if (playerPlane.Raycast(mouseRay, out hitDistance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDistance);

            Vector3 attackDirection = (hitPoint - transform.position).normalized;

            attackIndicator.transform.position = transform.position + attackDirection * 2;
        }
    }
}
