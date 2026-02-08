using Mirror;
using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class SingleplayerCharacterSwitching : MonoBehaviour
    {
        [SerializeField] GameObject otherPrefab;

        public void SwitchCharacter()
        {
            Vector3 lookVector = GetComponent<EntityCharacterController>().GetForwardVector();
            GameObject playerCharacter = Instantiate(otherPrefab, transform.position, transform.rotation);
            playerCharacter.transform.LookAt(playerCharacter.transform.position + lookVector);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            Destroy(gameObject);
        }
    }
}
