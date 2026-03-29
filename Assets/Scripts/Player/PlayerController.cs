using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.Player.Interaction;
using DigDig2.Entity;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Player
{
	public class PlayerController : MonoBehaviour
	{ 
        public CharacterType characterType;
        
        public int inputPlayerIndex = -1;
        public List<InputDevice> inputDevice;
        public bool IsAlive => health?.IsAlive ?? false;
        
        // Character
        [NonSerialized] public PlayerCharacterController playerCharacterController;
        [NonSerialized] public EntityCharacterController entityController;
        [NonSerialized] public GameObject characterObject;
        [NonSerialized] public Health health;

        // Character Switching
        [Header("Character Switching")]
        [SerializeField] private float cooldown;
        
        private float lastTimeSwitched;
        
		// Interactors
		private Interactor interactor;
        
        // Singleton Refs
        private GameManager gameManager;
        
        
        #region UnityCallbacks
        

		private void Start( )
		{
            gameManager = GameManager.Instance;
        }
        
        
        #endregion
        
        #region setup methods

        public void SetInputPlayerIDRecursive(int newid)
        {            
            inputPlayerIndex = newid;
            inputDevice = InputManager.Instance.GetInputPlayersDevices(newid);
            SetInputPlayerIDRecursive(transform, newid);
        }

        private void SetInputPlayerIDRecursive(Transform trans, int newid)
        {
            SetInputPlayerID(trans, newid);
            
            for (int i = trans.childCount - 1; i >= 0; i--)
            {
                SetInputPlayerIDRecursive(trans.GetChild(i), newid);
            }
        }
        
        private void SetInputPlayerID(Transform trans, int newid)
        {
            foreach (var inputModule in trans.GetComponents<InputModule>())
            { 
                inputModule.AllowedInputPlayerIndex = newid;
            }
        }

        public void SetCharacterObject(GameObject newCharacter)
        {
            if (characterObject) playerCharacterController.Disappear(true).Forget();
            characterObject = newCharacter;
            playerCharacterController = newCharacter.GetComponent<PlayerCharacterController>();
            entityController = newCharacter.GetComponent<EntityCharacterController>();
            
            health = newCharacter.GetComponent<Health>();
        }
        
        #endregion

		#region Character Switching
        
        // -- INPUT METHOD -- //
        public void OnInputGameSwitchCharacter(InputInfo inputInfo)
        {
            if (inputInfo.context.started && Time.time - lastTimeSwitched > cooldown &&
                !gameManager.IsMultiplayer) Invoke(nameof(SingleplayerSwitchCharacter), 0.03f);
        }
        
        public async void SingleplayerSwitchCharacter()
        {
            if (gameManager.IsMultiplayer)
            {
                BetterDebug.Log( "Tried to switch character in multiplayer mode, this is not allowed.", LogSeverity.Error );
            }
            
            Vector3 oldPlayerPos = characterObject.transform.position;
            
            // get New Character Type
            CharacterType newCharacterType = characterType == CharacterType.Max ? CharacterType.Minis : CharacterType.Max;
            BetterDebug.Log($"NEW character: {newCharacterType}");
            // Spawn new player
            GameObject newPrefab = gameManager.GetCharacterPrefabFromCharacterType(newCharacterType);
            var newCharacterObj = await InstantiateAsync(
                newPrefab, 
                transform,
                characterObject.transform.position, 
                Quaternion.identity
            );
            characterType = newCharacterType;
            
            TransferCharacterData(characterObject, newCharacterObj[0]);
            SetCharacterObject(newCharacterObj[0]);
            playerCharacterController.shouldStartDissolved = true;
            
            gameManager.characterSwitched.Invoke(characterType, newCharacterObj[0]);
        }
        
        public void TransferCharacterData(GameObject oldObj, GameObject newObj) 
        {
            if (newObj.TryGetComponent(out EntityCharacterController newEntityController) &&
                newObj.TryGetComponent(out PlayerCharacterController newPlayerinput) && 
                newObj.TryGetComponent(out Health newHealth) &&
                oldObj.TryGetComponent(out EntityCharacterController oldEntityController) &&
                oldObj.TryGetComponent(out PlayerCharacterController oldPlayerinput) && 
                oldObj.TryGetComponent(out Health oldHealth))
            {
                newEntityController.LookTowards(newObj.transform.position + oldEntityController.GetForwardVector(), false);
                newEntityController.inputMoveVector = oldEntityController.inputMoveVector;
                
                newPlayerinput.inputMoveVector = oldPlayerinput.inputMoveVector;
            
                newHealth.HealthPoints = oldHealth.HealthPoints;
            }
        }
        
        #endregion
        

	}
}
