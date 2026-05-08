// using System;
// using Cysharp.Threading.Tasks;
// using TMPro;
// using UnityEngine;

// ###--- CURRENTLY NOT IN USE ---### //

// namespace DigDig2.UI.Controllers
// {
//     public class LoadingMenu : MonoBehaviour
//     {
//         [SerializeField] GameObject loadingScreenPanel;
//         [SerializeField] TMP_Text descriptionText;
//         string description;

//         public async UniTask WaitFor(UniTask waitTask, string description = "")
//         {
//             ShowUIPanel(true);

//             await waitTask;

//             ShowUIPanel(false);
//             Destroy(gameObject);
//         }

//         private void ShowUIPanel(bool show)
//         {
//             loadingScreenPanel.SetActive(show);
//         }
//     }
// }


