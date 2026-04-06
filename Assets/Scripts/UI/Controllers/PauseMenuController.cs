using DigDig2.Game;
using DigDig2.UI.Navigation;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DigDig2.UI.Controllers
{
	public class PauseMenuController : MonoBehaviour
	{
		[SerializeField] private float openCooldown = 0.2f;

		[SerializeField] private GameObject buttonHoverEffectPrefab;
		[SerializeField] private GameObject buttonClickEffectPrefab;

		public UnityEvent<bool> stateChanged;
		private Button exitButton;

		private bool lastPausedState;
		private UserInterfaceNavigator navigator;
		private float openCooldownTimer;

		private VisualElement pauseMenuContainer;
		private Button resumeButton;
		private Button settingsButton;

		private UIDocument uiDocument;

		public bool Paused
		{
			get => navigator.NavigationUri != "/";
		}

		private void Awake( )
		{
			uiDocument = GetComponent<UIDocument>( );
			navigator = GetComponent<UserInterfaceNavigator>( );
		}

		private void Start( )
		{
			pauseMenuContainer = uiDocument.rootVisualElement.Query<VisualElement>( "animationContainer" );

			uiDocument.rootVisualElement.RegisterCallback<ClickEvent>( ButtonClick, TrickleDown.TrickleDown );
			uiDocument.rootVisualElement.RegisterCallback<NavigationSubmitEvent>( ButtonClick, TrickleDown.TrickleDown );
			uiDocument.rootVisualElement.RegisterCallback<FocusEvent>( ButtonHover, TrickleDown.TrickleDown );

			resumeButton = pauseMenuContainer.Query<Button>( "resume" );
			resumeButton.clicked += Close;
			settingsButton = pauseMenuContainer.Query<Button>( "settings" );
			exitButton = pauseMenuContainer.Query<Button>( "exit" );
			exitButton.clicked += SaveAndExit;

			navigator.Hierarchy = new(
				"game",
				null,
				0,
				null,
				null,
				true,
				new( )
				{
					new( "pauseMenu", pauseMenuContainer, 0.1f, resumeButton )
				}
			);

			navigator.navigatedTo.AddListener( uri =>
				{
					if ( uri == "/" && lastPausedState )
					{
						lastPausedState = false;
						stateChanged.Invoke( false );
					}
					else if ( !lastPausedState )
					{
						lastPausedState = true;
						stateChanged.Invoke( true );
					}
				}
			);
		}

		private void Update( )
		{
			if ( openCooldownTimer > 0 ) openCooldownTimer = Mathf.Max( openCooldownTimer - Time.deltaTime, 0 );
		}

		private void ButtonClick( EventBase _ ) { PlaySoundEffect( buttonClickEffectPrefab ); }

		private void ButtonHover( EventBase _ ) { PlaySoundEffect( buttonHoverEffectPrefab ); }

		private void PlaySoundEffect( GameObject effectPrefab ) { Destroy( Instantiate( effectPrefab, Vector3.zero, Quaternion.identity, transform ), 10f ); }
        
		public void Open( )
		{
			navigator.NavigateTo( "/pauseMenu" );
			openCooldownTimer = openCooldown;
		}

		public void Close( )
		{
			if ( openCooldownTimer <= 0 ) navigator.NavigateTo( "/" );
		}

        private void SaveAndExit()
        {
            GameManager.Instance.SaveAndLoadMainMenu( );
        }

		private void OnInputUICancel( )
		{
			if ( openCooldownTimer <= 0 ) navigator.NavigateBack( );
		}

        private void OnInputGamePause()
        {
            if (Paused) Close();
            else Open();
        }
    }
}
