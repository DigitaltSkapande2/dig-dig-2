using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace DigDig2
{
    public class CharacterSelectSequencer : NetworkIdentity
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject minisPrefab;
        [Header("Scene Refs")]
        [SerializeField] private Transform maxSpawnPoint;
        [SerializeField] private Transform minisSpawnPoint;
        [Header("Clickable Materials")]
        [SerializeField] private ClickableMesh maxClickableCollider;
        [SerializeField] private ClickableMesh minisClickableCollider;

        [Header("UI Refs")]
        [SerializeField] private Button switchCharacterButton;
        [SerializeField] private TMP_Text MaxNameplateText;
        [SerializeField] private TMP_Text MinisNameplateText;
        [Header("Materials")]
        [SerializeField] private float intensityToSet = 5f;
        [SerializeField] private string float_to_modify = "fresnell_intensity";


        private bool hostIsMax = true;

        private GameObject maxInstance;
        private GameObject minisInstance;

        private void Start()
        {
            maxClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Max, maxClickableCollider));
            minisClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Mini, minisClickableCollider));

            if (isServer)
            {
                maxInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
                minisInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);
                switchCharacterButton.onClick.AddListener(OnSwitchCharacterButtonClicked);
            }
        }

        private void OnSwitchCharacterButtonClicked()
        {
            hostIsMax = !hostIsMax;
            if (hostIsMax)
            {
                MaxNameplateText.text = "Max (Host)";
                MinisNameplateText.text = "Minis (Client)";
            }
            else
            {
                MaxNameplateText.text = "Max (Client)";
                MinisNameplateText.text = "Minis (Host)";
            }
        }

        private void OnStartButtonClicked()
        {
            //NetworkServer.ReplacePlayerForConnection() // TODOreplace characters,
        }

        private void OnCharacterClicked(CharacterType characterType, ClickableMesh clickedMesh)
        {
            Debug.Log("Clicked on character: " + characterType);
        }

        private void FixedUpdate()
        {
            
        }
    }
}
