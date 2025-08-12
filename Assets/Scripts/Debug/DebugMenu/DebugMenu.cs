using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DigDig2.Debugging
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("Object ref")]
        [SerializeField] private GameObject debugMenuGraphics;
        [SerializeField] private RectTransform debugMenuContent;
        [SerializeField] private GameObject debugMenuElementPrefab;

        private List<DebugMenuElement> debugMenuElements = new List<DebugMenuElement>();
        private GameInputSystem.DebugMenuActions inputMap;

        private void Start()
        {
            inputMap = GameInputManager.Instance.gameInputSystem.DebugMenu;
            inputMap.OpenDebugMenu.started += OpenMenu;
            inputMap.CloseDebugMenu.started += context => CloseMenu();
            inputMap.Enable();
        }

        private void OnDisable()
        {
            inputMap.Disable();
        }

        public void OpenMenu(InputAction.CallbackContext context)
        {
            SetMenuActive(true);
            UpdateMenuElements();
        }

        public void CloseMenu()
        {
            SetMenuActive(false);
        }

        private void SetMenuActive(bool _state)
        {
            debugMenuGraphics.SetActive(_state);
        }

        private void UpdateMenuElements()
        {
            ClearMenuElements();
            GenerateMenuElements();
            LayoutRebuilder.MarkLayoutForRebuild(debugMenuContent);
        }

        private void GenerateMenuElements()
        {
            UnityEngine.Debug.Log("Generating Debug Menu Elements...");
            // Find all objects in the scene
            MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            List<Type> nonDebugableBehaviourTypes = new List<Type>();

            foreach (MonoBehaviour behaviour in allBehaviours)
            {
                Type behaviourType = behaviour.GetType();
                if (nonDebugableBehaviourTypes.Contains(behaviourType)) continue;

                // Check the class for [Debug] attribute
                if (Attribute.IsDefined(behaviourType, typeof(DebugAttribute)))
                {
                    var debugAttr = (DebugAttribute)Attribute.GetCustomAttribute(behaviourType, typeof(DebugAttribute));

                    // Find fields marked with [DebugSerialized]
                    FieldInfo[] debugFields = behaviourType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => Attribute.IsDefined(f, typeof(DebugSerializedAttribute)))
                        .ToArray()
                    ;

                    // Spawn menu element for this behaviour
                    DebugMenuElement element = SpawnMenuElement(behaviour, debugFields, debugAttr.IsToggelable);
                    debugMenuElements.Add(element);

                }
                else
                {
                    nonDebugableBehaviourTypes.Add(behaviourType);
                }
            }
        }

        private DebugMenuElement SpawnMenuElement(MonoBehaviour behaviour, FieldInfo[] debugFields, DebugMenuToggleable toggleable)
        {
            DebugMenuElement _elementComponent = Instantiate(debugMenuElementPrefab, debugMenuContent).GetComponent<DebugMenuElement>();
            _elementComponent.Initialize(behaviour, debugFields, toggleable);
            return _elementComponent;
        }

        private void ClearMenuElements()
        {
            foreach (DebugMenuElement _element in debugMenuElements)
            {
                Destroy(_element.gameObject);
            }
            debugMenuElements.Clear();
        }
    }
}
