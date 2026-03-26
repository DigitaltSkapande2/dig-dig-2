using System;
using System.Collections.Generic;

using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.Player.Interaction;

using UnityEngine;
using UnityEngine.Events;
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
            if (characterObject) Destroy(characterObject); // TODO: Dissolving
            characterObject = newCharacter;
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
        
        public void SingleplayerSwitchCharacter()
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
            GameObject newCharacterObj = Instantiate(
                newPrefab, 
                characterObject.transform.position, 
                Quaternion.identity, 
                transform
            );
            characterType = newCharacterType;
            
            TransferCharacterData(characterObject, newCharacterObj);
            SetCharacterObject(newCharacterObj);
            
            gameManager.characterSwitched.Invoke(characterType, newCharacterObj);
        }
        
        public void TransferCharacterData(GameObject oldObj, GameObject newObj) 
        {
            if (newObj.TryGetComponent(out EntityCharacterController newEntityController) &&
                newObj.TryGetComponent(out PlayerCharacterInput newPlayerinput) && 
                newObj.TryGetComponent(out Health newHealth) &&
                oldObj.TryGetComponent(out EntityCharacterController oldEntityController) &&
                oldObj.TryGetComponent(out PlayerCharacterInput oldPlayerinput) && 
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
