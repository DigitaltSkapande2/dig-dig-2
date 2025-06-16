using DigDig2;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, GameInputSystem.IAttackActions
{
    [Tooltip("The prafab used to show in what direction the player is aiming")]
    [SerializeField] GameObject aimArrow;

    GameObject arrowInstance;

    private GameInputSystem.AttackActions attackActions;

    bool aiming;

    bool frozen;

    Vector2 mousePos;
    Vector3 attackDirection;

    Plane playerPlane;

    void Start()
    {
        EnableInput();
    }

    void OnDisable()
    {
        DisableInput();
    }
    /// <summary>
    /// Used by the PlayerCharacterController to mirror the "frozen" state of the player
    /// </summary>
    /// <param name="value"> The current forzen state of the player </param>
    public void SetFrozen(bool value)
    {
        frozen = value;

        if (!value)
        {
            aiming = false;

            // Removes the aiming indicator when frozen
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
        if (context.started)
        {
            Attack();
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (frozen) { return; }

        if (context.started)
        {
            aiming = true;
        }

        if (context.canceled)
        {
            aiming = false;

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
        if (aiming)
        {
            Indicator();
        }
    }

    void HandleAiming()
    {   // Defines plane at the same y-coordinate as the player
        playerPlane = new Plane(Vector3.up, transform.position);
        
        Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        float hitDistance;

        // Check raycast and return distance along the ray at which it intersects the plane
        if (playerPlane.Raycast(mouseRay, out hitDistance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDistance);
            attackDirection = (hitPoint - transform.position).normalized * 2;
        }
    }

    void Indicator()
    {
        HandleAiming();

        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(aimArrow, attackDirection, Quaternion.identity);
        }

        arrowInstance.transform.position = transform.position + attackDirection;
        arrowInstance.transform.LookAt(transform.position + attackDirection * 2);
    }

    void Attack()
    {
        HandleAiming();

        Collider[] others = Physics.OverlapSphere(transform.position + attackDirection, 1);

        Debug.Log(others.Length);

        if (others.Length > 0)
        {
            foreach (Collider other in others)
            {
                UnityEngine.Debug.Log(other.name);

                if (other.GetComponent<Damageable>() == null) { return; }
                
                other.gameObject.GetComponent<Damageable>().Damage(1);
            }
        }
    }
}
