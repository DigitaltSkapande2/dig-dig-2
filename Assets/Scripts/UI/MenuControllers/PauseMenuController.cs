using DigDig2.UINavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DigDig2
{
    public class PauseMenuController : MonoBehaviour
    {
        public bool Paused
        {
            get
            {
                return navigator.NavigationUri != "/";
            }
        }

        private bool lastPausedState = false;
        public UnityEvent<bool> stateChanged;

        private VisualElement pauseMenuContainer;
        
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

        public void Open()
        {
            navigator.NavigateTo("/pauseMenu");
        }

        public void Close()
        {
            navigator.NavigateTo("/");
        }
        
        private void OnCancel()
        {
            navigator.NavigateBack();
        }
    }
}
