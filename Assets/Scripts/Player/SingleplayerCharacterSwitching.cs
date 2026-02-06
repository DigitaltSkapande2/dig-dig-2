using Mirror;
using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class SingleplayerCharacterSwitching : MonoBehaviour
    {
        [SerializeField] GameObject otherPrefab;

        Attacker attacker;

        void Awake()
        {
            attacker = GetComponent<Attacker>();
        }

        public void SwitchCharacter()
        {
            GameObject playerCharacter = Instantiate(otherPrefab, transform.position, transform.rotation);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            Destroy(gameObject, Time.deltaTime);
        }
    }
}
