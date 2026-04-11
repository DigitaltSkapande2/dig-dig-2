using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Game;
using DigDig2.Player;
using DigDig2.Player.Combat;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI.Collections;

namespace DigDig2.UI.Controllers
{
	[RequireComponent( typeof( UIDocument ) )]
	public class GameHudController : MonoBehaviour
	{
		[SerializeField] private List<Texture2D> healthBarTextures = new( );

        [SerializeField] private Color playerDeadBannerTint;

        private VisualElement singlePlayerContainer;
        private VisualElement multiPlayerContainer;
        
        private UIDocument uiDocument;
        
        // SinglePlayer
        private VisualElement singlePlayerMaxCharacterIndicator;
		private VisualElement singlePlayerMiniCharacterIndicator;
        
        private VisualElement singlePlayerHealthBarImage;
        
        // MultiPlayer

        private VisualElement maxContainer;
        public VisualElement maxHealthBar;
        public VisualElement maxHealthBarImage;

        private VisualElement minisContainer;
        public VisualElement minisHealthBar;
        public VisualElement minisHealthBarImage;

        private GameManager gameManager;


        private Action updateFunction;


        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>( );
        }

		private void Start( )
        {
            singlePlayerContainer = uiDocument.rootVisualElement.Query("singlePlayerContainer");
            singlePlayerContainer.style.opacity = new StyleFloat(0f);
            multiPlayerContainer = uiDocument.rootVisualElement.Query("multiPlayerContainer");
            multiPlayerContainer.style.opacity = new StyleFloat(0f);

            gameManager = GameManager.Instance;
            gameManager.gameStarted.AddListener(OnGameStarted);
		}
        
        private void OnGameStarted()
        {
            if (gameManager.IsMultiplayer)
            {
                OnMultiplayerGameStarted();
            }
            else
            {
                OnSingleplayerGameStarted();
            }
        }
        
        #region SinglePlayer

        private void OnSingleplayerGameStarted()
        {
            FetchSinglePlayerUIDocumentReferences();
            SetupCharacterBindings();
            
            singlePlayerContainer.style.opacity = new StyleFloat(100);
        }

        private void FetchSinglePlayerUIDocumentReferences()
        {
            VisualElement characterIndicatorContainer = uiDocument.rootVisualElement.Query<VisualElement>( "characterIndicatorContainer" );
            singlePlayerMaxCharacterIndicator = characterIndicatorContainer.Query<VisualElement>( "max" );
            singlePlayerMiniCharacterIndicator = characterIndicatorContainer.Query<VisualElement>( "mini" );

            VisualElement healthBar = uiDocument.rootVisualElement.Query<VisualElement>( "healthBar" );
            singlePlayerHealthBarImage = healthBar.Query<VisualElement>( "image" );
        }

		private async void SetupCharacterBindings( )
		{
            gameManager.characterSwitched.AddListener( OnCharacterSwitch );

			await UniTask.WaitUntil( ( ) => gameManager.PlayerOne.gameObject );
			OnCharacterSwitch( gameManager.PlayerOne.characterType, gameManager.PlayerOne.characterObject );
		}

		private void OnCharacterSwitch( CharacterType characterType, GameObject characterObject )
		{
            // Character Flair/Banner Indicators
			if ( characterType == CharacterType.Max )
			{
				singlePlayerMiniCharacterIndicator.RemoveFromClassList( "selected" );
				singlePlayerMaxCharacterIndicator.AddToClassList( "selected" );
			}
			else
			{
				singlePlayerMaxCharacterIndicator.RemoveFromClassList( "selected" );
				singlePlayerMiniCharacterIndicator.AddToClassList( "selected" );
			}
            
            // Health
			Health healthComponent = characterObject.GetComponent<Health>( );
			healthComponent.healthChanged.AddListener( (health) => UpdateHealthBar(singlePlayerHealthBarImage, health));
			UpdateHealthBar( singlePlayerHealthBarImage, healthComponent.HealthPoints );
		}
        
        #endregion

        #region Multiplayer
        
        private void OnMultiplayerGameStarted()
        {
            FetchMultiPlayerUIDocumentReferences();
            multiPlayerContainer.style.opacity = new StyleFloat(100);
            ForEachChild(maxContainer,
                element => element.style.unityBackgroundImageTintColor = new StyleColor(Color.white));
            ForEachChild(minisContainer,
                element => element.style.unityBackgroundImageTintColor = new StyleColor(Color.white));

            PlayerController maxCharacterObj = gameManager.playerControllers[gameManager.maxPlayerID];
            PlayerController minisCharacterObj = gameManager.playerControllers[gameManager.minisPlayerID];
            
            ClearGrayTint(maxContainer);
            ClearGrayTint(minisContainer);
            
            SetupPlayerCharacterConnectionsMax(maxCharacterObj);
            SetupPlayerCharacterConnectionsMini(minisCharacterObj);

            foreach (var player in gameManager.playerControllers)
            {
                player.characterObjectSet.AddListener(OnNewCharacterObject);
            }
        }
        
        private void OnNewCharacterObject(PlayerController playerController)
        {
            if (playerController.characterType == CharacterType.Max) SetupPlayerCharacterConnectionsMax(playerController);
            else SetupPlayerCharacterConnectionsMini(playerController);
            OnPlayerHealthChanged(playerController);
        }

        private void SetupPlayerCharacterConnectionsMax(PlayerController playerController)
        {
            Health maxHealthComponent = playerController.health;
            maxHealthComponent.healthChanged.AddListener( (health) => UpdateHealthBar( maxHealthBarImage, health) );
            UpdateHealthBar( maxHealthBarImage, maxHealthComponent.HealthPoints );
            
            playerController.health.healthChanged.AddListener( _ =>
            {
                OnPlayerHealthChanged(playerController);
            });
        }

        private void SetupPlayerCharacterConnectionsMini(PlayerController playerController)
        {
            Health minisHealthComponent = playerController.health;
            minisHealthComponent.healthChanged.AddListener( (health) => UpdateHealthBar( minisHealthBarImage, health) );
            UpdateHealthBar( maxHealthBarImage, minisHealthComponent.HealthPoints );
            
            playerController.health.healthChanged.AddListener( _ =>
            {
                OnPlayerHealthChanged(playerController);
            });
        }

        private void ForEachChild(VisualElement rootElement, Action<VisualElement> func)
        {
            func(rootElement);
            foreach (var child in rootElement.Children())
            {
                func(child);
            }
        }

        private void OnPlayerHealthChanged(PlayerController player)
        {
            if (player.health.HealthPoints <= 0) OnPlayerDeath(player);
            else
            {
                ClearGrayTint(player.characterType == CharacterType.Max ? maxContainer : minisContainer);
            }
        }

        private void OnPlayerDeath(PlayerController player)
        {
            if (player.characterType == CharacterType.Max)
            {
                SetGrayTint(maxContainer);
            }
            else
            {
                SetGrayTint(minisContainer);
            }
        }
        
        private void SetGrayTint(VisualElement element)
        {
            element.AddToClassList("grayscale");
        }

        private void ClearGrayTint(VisualElement element)
        {
            element.RemoveFromClassList("grayscale");
        }

        private void FetchMultiPlayerUIDocumentReferences()
        {
            maxContainer = multiPlayerContainer.Query("maxContainer");
            maxHealthBar = maxContainer.Query("maxHealthBar");
            maxHealthBarImage = maxHealthBar.Query("image");
            
            minisContainer = multiPlayerContainer.Query("minisContainer");
            minisHealthBar = minisContainer.Query("minisHealthBar");
            minisHealthBarImage = minisHealthBar.Query("image");
        }       
        
        #endregion
        
        private void UpdateHealthBar(VisualElement healthBarImage, int health )
        {
            Debug.Log( healthBarTextures[ health ] );
            healthBarImage.style.backgroundImage = new( healthBarTextures[ health ] );
        }
	}
}