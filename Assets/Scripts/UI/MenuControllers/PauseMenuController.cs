using DigDig2.UINavigation;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DigDig2
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private float openCooldown = 0.2f;
        
        public bool Paused
        {
            get
            {
                return navigator.NavigationUri != "/";
            }
        }

        private bool lastPausedState = false;
        public UnityEvent<bool> stateChanged;

        private float openCooldownTimer = 0;

        private VisualElement pauseMenuContainer;

        private Button resumeButton;
        private Button settingsButton;
        private Button exitButton;
        
        private UIDocument uiDocument;
        private UserInterfaceNavigator navigator;

        
        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            navigator = GetComponent<UserInterfaceNavigator>();
        }

        private void Start()
        {
            pauseMenuContainer = uiDocument.rootVisualElement.Query<VisualElement>("animationContainer");

            resumeButton = pauseMenuContainer.Query<Button>("resume");
            resumeButton.clicked += Close;
            settingsButton = pauseMenuContainer.Query<Button>("settings");
            exitButton = pauseMenuContainer.Query<Button>("exit");
            exitButton.clicked += SaveAndExit;
            
            navigator.Hierarchy = new NavigationNode("game", null, 0, null, true, new()
            {
                new NavigationNode("pauseMenu", pauseMenuContainer, 0.1f),
            });
            
            navigator.navigatedTo.AddListener((string uri) =>
            {
                if (uri == "/" && lastPausedState != false)
                {
                    lastPausedState = false;
                    stateChanged.Invoke(false);
                }
                else if (lastPausedState != true)
                {
                    lastPausedState = true;
                    stateChanged.Invoke(true);
                }
            });
        }
        
        private void Update() 
        {
            if (openCooldownTimer > 0) openCooldownTimer = Mathf.Max(openCooldownTimer - Time.deltaTime, 0);
        }
        
        public void Open()
        {
            navigator.NavigateTo("/pauseMenu");
            openCooldownTimer = openCooldown;
        }
        public void Close()
        {
            if (openCooldownTimer <= 0) navigator.NavigateTo("/");
        }

        private void SaveAndExit()
        {
            GameManager.Instance.SaveAndExit();
        }
        
        private void OnCancel()
        {
            if (openCooldownTimer <= 0) navigator.NavigateBack();
        }
    }
}
