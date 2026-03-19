using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using DigDig2.Combat;
using DigDig2.Game;
using DigDig2.Player;
using DigDig2.Player.Combat;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2.UI.Controllers
{
	[RequireComponent( typeof( UIDocument ) )]
	public class GameHudController : MonoBehaviour
	{
		[SerializeField] private float focusTargetIndicatorRotationSpeed = 10f;

		[SerializeField] private List<Texture2D> healthBarTextures = new( );

        [SerializeField] private Color playerDeadBannerTint;

        private VisualElement singlePlayerContainer;
        private VisualElement multiPlayerContainer;
        
        private UIDocument uiDocument;
        
        // SinglePlayer
        private VisualElement singlePlayerMaxCharacterIndicator;
		private VisualElement singlePlayerMiniCharacterIndicator;

        private VisualElement singlePlayerFocusIndicator;
        private VisualElement singlePlayerFocusTargetIndicatorImage;
        
        private VisualElement singlePlayerHealthBarImage;
        
        // MultiPlayer

        private VisualElement maxContainer;
        public VisualElement maxHealthBar;
        public VisualElement maxHealthBarImage;
        public VisualElement maxFocusIndicator;
        public VisualElement maxFocusIndicatorImage;

        private VisualElement minisContainer;
        public VisualElement minisHealthBar;
        public VisualElement minisHealthBarImage;
        public VisualElement minisFocusIndicator;
        public VisualElement minisFocusIndicatorImage;


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
  
            
            GameManager.Instance.gameStarted.AddListener(OnGameStarted);
		}
        
        private void OnGameStarted()
        {
            print("YAHOOOOO");
            if (GameManager.Instance.IsMultiplayer)
            {
                print("NEt an Yaho");
                OnMultiplayerGameStarted();
                updateFunction = MultiplayerUpdate;
            }
            else
            {
                print("YAHOO IN THE HOUSE");
                OnSingleplayerGameStarted();
                updateFunction = SinglePlayerUpdate;
            }
        }

		private void Update( )
		{
            updateFunction?.Invoke();
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

            singlePlayerFocusIndicator = uiDocument.rootVisualElement.Query<VisualElement>( "focusTargetIndicator" );
            singlePlayerFocusTargetIndicatorImage = singlePlayerFocusIndicator.Query<VisualElement>( "image" );
        }

		private async void SetupCharacterBindings( )
		{
			GameManager.Instance.characterSwitched.AddListener( OnCharacterSwitch );

			await UniTask.WaitUntil( ( ) => GameManager.Instance.PlayerOne.characterObject );
			OnCharacterSwitch( GameManager.Instance.PlayerOne.characterType, GameManager.Instance.PlayerOne.characterObject );
		}

        private void SinglePlayerUpdate()
        {
            RotateFocusIndicatorImage(singlePlayerFocusIndicator, singlePlayerFocusTargetIndicatorImage);
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
            
            // Focus Indicator
            if (characterObject.TryGetComponent(out PlayerFocuser playerFocuser))
            {
                playerFocuser.isFocusingStateChanged.AddListener((active) => { SetFocusIndicatorActive(singlePlayerFocusIndicator, active); });
                playerFocuser.focusPositionUpdated.AddListener((pos) => { UpdateFocusIndicatorPosition(singlePlayerFocusIndicator, pos);});
            }
		}
        
        #endregion

        #region Multiplayer
        
        private void OnMultiplayerGameStarted()
        {
            FetchMultiPlayerUIDocumentReferences();
            multiPlayerContainer.style.opacity = new StyleFloat(100);

            GameObject maxCharacterObj = GameManager.Instance.players[GameManager.Instance.maxPlayerID].characterObject;
            GameObject minisCharacterObj = GameManager.Instance.players[GameManager.Instance.minisPlayerID].characterObject;
            
            
            // Setup Health callbacks
            Health maxHealthComponent = maxCharacterObj.GetComponent<Health>( );
            maxHealthComponent.healthChanged.AddListener( (health) => UpdateHealthBar( maxHealthBarImage, health) );
            UpdateHealthBar( maxHealthBarImage, maxHealthComponent.HealthPoints );
            
            Health minisHealthComponent = minisCharacterObj.GetComponent<Health>( );
            minisHealthComponent.healthChanged.AddListener( (health) => UpdateHealthBar( minisHealthBarImage, health) );
            UpdateHealthBar( maxHealthBarImage, minisHealthComponent.HealthPoints );
            
            
            GameManager.Instance.playerDeath.AddListener(OnPlayerDeath);
            
            
            // Hook up focusing
            if (maxCharacterObj.TryGetComponent(out PlayerFocuser maxPlayerFocuser))
            {
                maxPlayerFocuser.isFocusingStateChanged.AddListener((active) => { SetFocusIndicatorActive(maxFocusIndicator, active); });
                maxPlayerFocuser.focusPositionUpdated.AddListener((pos) => { UpdateFocusIndicatorPosition(maxFocusIndicator, pos); });
            }
            if (minisCharacterObj.TryGetComponent(out PlayerFocuser minisPlayerFocuser))
            {
                minisPlayerFocuser.isFocusingStateChanged.AddListener((active) => { SetFocusIndicatorActive(minisFocusIndicator, active); });
                minisPlayerFocuser.focusPositionUpdated.AddListener((pos) => { UpdateFocusIndicatorPosition(minisFocusIndicator, pos); });
            }
        }

        private void OnPlayerDeath(PlayerRef player)
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
            element.style.unityBackgroundImageTintColor = new StyleColor(playerDeadBannerTint);
        }

        private void FetchMultiPlayerUIDocumentReferences()
        {
            maxContainer = multiPlayerContainer.Query("maxContainer");
            maxHealthBar = maxContainer.Query("maxHealthBar");
            maxHealthBarImage = maxHealthBar.Query("image");
            maxFocusIndicator = maxContainer.Query("maxFocusTargetIndicator");
            maxFocusIndicatorImage = maxFocusIndicator.Query("image");
            
            minisContainer = multiPlayerContainer.Query("minisContainer");
            minisHealthBar = minisContainer.Query("minisHealthBar");
            minisHealthBarImage = minisHealthBar.Query("image");
            minisFocusIndicator = minisContainer.Query("minisFocusTargetIndicator");
            minisFocusIndicatorImage = minisFocusIndicator.Query("image");
        }

        private void MultiplayerUpdate()
        {
            RotateFocusIndicatorImage(maxFocusIndicator, maxFocusIndicatorImage);
            RotateFocusIndicatorImage(minisFocusIndicator, minisFocusIndicatorImage);
        }


        #endregion

        private void RotateFocusIndicatorImage(VisualElement indicator, VisualElement indicatorImage)
        {
            indicatorImage.style.rotate = new(
                new Rotate(
                    indicatorImage.resolvedStyle.rotate.angle.value +
                    Time.deltaTime * focusTargetIndicatorRotationSpeed / indicator.resolvedStyle.scale.value.x
                )
            );
        }
        
        
        public void UpdateFocusIndicatorPosition( VisualElement indicator, Vector3 worldPosition )
        {
            Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel( uiDocument.rootVisualElement.panel, worldPosition, Camera.main );
            indicator.style.translate = new( new Translate( screenPosition.x, screenPosition.y ) );
        }

        private void SetFocusIndicatorActive(VisualElement indicator, bool active)
        {
            indicator.style.display = new( active ? DisplayStyle.Flex : DisplayStyle.None );
            indicator.style.opacity = new( active ? 1f : 0f );
            indicator.style.scale = new( new Scale( active ? new( 1f, 1f ) : new Vector2( 2f, 2f ) ) );

        }
        
        private void UpdateHealthBar(VisualElement healthBarImage, int health )
        {
            Debug.Log( healthBarTextures[ health ] );
            healthBarImage.style.backgroundImage = new( healthBarTextures[ health ] );
        }
	}
}
