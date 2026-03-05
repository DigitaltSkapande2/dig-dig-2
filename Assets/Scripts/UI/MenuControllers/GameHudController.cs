using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHudController : MonoBehaviour
    {
        [SerializeField] private float focusTargetIndicatorRotationSpeed = 10f;
        
        private VisualElement characterIndicatorContainer;
        private VisualElement maxCharacterIndicator;
        private VisualElement miniCharacterIndicator;

        private VisualElement healthBar;
        private VisualElement fullHealthBar;
        
        private VisualElement focusTargetIndicator;
        private VisualElement focusTargetIndicatorImage;

        private UIDocument uiDocument;

        private int maxHealth = 1;
        
        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            characterIndicatorContainer = uiDocument.rootVisualElement.Query<VisualElement>("characterIndicatorContainer");
            maxCharacterIndicator = characterIndicatorContainer.Query<VisualElement>("max");
            miniCharacterIndicator = characterIndicatorContainer.Query<VisualElement>("mini");

            healthBar = uiDocument.rootVisualElement.Query<VisualElement>("healthBar");
            fullHealthBar = healthBar.Query<VisualElement>("full");

            focusTargetIndicator = uiDocument.rootVisualElement.Query<VisualElement>("focusTargetIndicator");
            focusTargetIndicatorImage = focusTargetIndicator.Query<VisualElement>("image");

            SetupCharacterBindings();
        }

        private void Update()
        {
            focusTargetIndicatorImage.style.rotate = new StyleRotate(new Rotate(
                focusTargetIndicatorImage.resolvedStyle.rotate.angle.value +
                 Time.deltaTime * focusTargetIndicatorRotationSpeed / focusTargetIndicator.resolvedStyle.scale.value.x));
        }

        private async void SetupCharacterBindings()
        {
            GameManager.Instance.characterSwitched.AddListener(UpdateCharacter);
            
            await UniTask.WaitUntil(() => GameManager.Instance.LocalPlayerObj);
            UpdateCharacter(GameManager.Instance.currentCharacter, GameManager.Instance.LocalPlayerObj);
        }

        private void UpdateCharacter(CharacterType characterType, GameObject characterObject)
        {
            Debug.Log("GameHudController: Updating character");
            
            if (characterType == CharacterType.Max)
            {
                miniCharacterIndicator.RemoveFromClassList("selected");
                maxCharacterIndicator.AddToClassList("selected");
            }
            else
            {
                maxCharacterIndicator.RemoveFromClassList("selected");
                miniCharacterIndicator.AddToClassList("selected");
            }

            Health healthComponent = characterObject.GetComponent<Health>();
            maxHealth = healthComponent.MaxHealthPoints;
            healthComponent.healthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(healthComponent.HealthPoints);
        }

        private void UpdateHealthBar(int health)
        {
            Debug.Log("GameHudController: Updating health");
            
            fullHealthBar.style.width = new StyleLength(new Length(health / (float)maxHealth * 100f, LengthUnit.Percent));
        }

        public void UpdateFocusTarget(bool inUse, Vector3 worldPosition)
        {
            focusTargetIndicator.style.display = new StyleEnum<DisplayStyle>(inUse ? DisplayStyle.Flex : DisplayStyle.None);
            focusTargetIndicator.style.opacity = new StyleFloat(inUse ? 1f : 0f);
            focusTargetIndicator.style.scale = new StyleScale(new Scale(inUse ? new Vector2(1f, 1f) : new Vector2(2f, 2f)));
            if (!inUse) return;
            
            Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel(uiDocument.rootVisualElement.panel, worldPosition, Camera.main);
            focusTargetIndicator.style.translate =
                new StyleTranslate(new Translate(screenPosition.x, screenPosition.y));
        }
    }
}
