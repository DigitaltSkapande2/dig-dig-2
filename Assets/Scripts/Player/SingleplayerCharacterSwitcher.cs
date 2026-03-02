using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class SingleplayerCharacterSwitcher : MonoBehaviour
    {
        [SerializeField] GameObject otherPrefab;
        [SerializeField] private float cooldown;

        float lastTimeSwitched;

        public void SwitchCharacter()
        {
            if (Time.time - lastTimeSwitched > cooldown)
            {
                GameManager.Instance.SingleplayerSwitchCharacter();
            }
        }
    }
}
