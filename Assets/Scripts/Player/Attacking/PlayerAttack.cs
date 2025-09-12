using DigDig2;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, GameInputSystem.IAttackActions
{
    [SerializeField] AttackData attackData;

    private GameInputSystem.AttackActions attackActions;
    Vector2 mousePos;
    bool frozen;

    Transform box;

    Vector3 dir;
    float t;
    bool active;

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
//sigma was here
    #endregion

    #region Inputs

    public void OnAttack1(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Attack();
        }
    }

    public void OnAttack2(InputAction.CallbackContext context)
    {

    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        if (frozen) { return; }

        mousePos = context.ReadValue<Vector2>();
    }

    #endregion

    private void Update()
    {
        if (active) HandleSwing();
    }

    void Attack()
    {
        if (active) return;

        t = 0;
        active = true;

        float xCurveValue = attackData.X.Evaluate(0);
        float yCurveValue = attackData.Y.Evaluate(0);
        float zCurveValue = attackData.Z.Evaluate(0);

        float stepX = attackData.X.Evaluate(attackData.step);
        float stepY = attackData.Y.Evaluate(attackData.step);
        float stepZ = attackData.Z.Evaluate(attackData.step);

        Vector3 pos = transform.right * xCurveValue + transform.up * yCurveValue + transform.forward * zCurveValue;
        Vector3 stepPos = transform.right * stepX + transform.up * stepY + transform.forward * stepZ;

        dir = (stepPos - pos).normalized;

        box = Instantiate(attackData.hitbox, transform.position + pos, Quaternion.identity, transform).transform;
        box.forward = dir;
        box.GetComponent<Hitbox>().SetAttackData(attackData);
    }

    void HandleSwing()
    {
        t += 1 / attackData.attackTime * Time.deltaTime;
        float position = attackData.speed.Evaluate(t);

        float xCurveValue = attackData.X.Evaluate(position);
        float yCurveValue = attackData.Y.Evaluate(position);
        float zCurveValue = attackData.Z.Evaluate(position);

        float stepX = attackData.X.Evaluate(position + attackData.step);
        float stepY = attackData.Y.Evaluate(position + attackData.step);
        float stepZ = attackData.Z.Evaluate(position + attackData.step);

        Vector3 pos = transform.right * xCurveValue + transform.up * yCurveValue + transform.forward * zCurveValue;
        Vector3 stepPos = transform.right * stepX + transform.up * stepY + transform.forward * stepZ;

        if (position + attackData.step < 1)
        {
            dir = (stepPos - pos).normalized;
        }

        box.position = transform.position + pos;
        box.forward = dir;

        if (t > 1)
        {
            active = false;
            Destroy(box.gameObject);
        }
    }

}
