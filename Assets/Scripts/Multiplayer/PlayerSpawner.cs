// 
// using UnityEngine;

// namespace DigDig2
// {
//     public class PlayerSpawner : Singleton<PlayerSpawner>
//     {
//         [SerializeField] private GameObject maxPrefab;
//         [SerializeField] private GameObject miniPrefab;

//         
//         public void SpawnPlayers(Vector3 maxStartPos, Quaternion maxStartRotation, Vector3 minisStartPos, Quaternion minisStartRotation)
//         {
//             GameObject maxInstance = Instantiate(maxPrefab, maxStartPos, maxStartRotation, transform);
//             NetworkServer.AddPlayerForConnection(NetworkManager.singleton.MaxPlayerConnection, maxInstance);

//             GameObject minisInstance = Instantiate(maxPrefab, maxStartPos, maxStartRotation, transform);
//             NetworkServer.AddPlayerForConnection(NetworkManager.singleton.MiniPlayerConnection, minisInstance);
//         }
//     }
// }


