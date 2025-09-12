using DigDig2;
using UnityEngine;
using UnityEngine.Events;

public class Attackable : MonoBehaviour
{
    public UnityEvent<AttackData> hit = new();

    public void Hit(AttackData attackData)
    {
        hit.Invoke(attackData);
    }
}