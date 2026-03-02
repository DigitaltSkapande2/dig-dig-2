using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigDig2.Debugging
{
    public class DebugMenuElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private RectTransform elementContainer;
        [Header("prefabs")]
        [SerializeField] private GameObject boolPrefab;
        [SerializeField] private GameObject intPrefab;
        [SerializeField] private GameObject floatPrefab;
        [SerializeField] private GameObject stringPrefab;
        [SerializeField] private GameObject defaultPrefab;

        private List<GameObject> debugFieldElements = new List<GameObject>();

        private VerticalLayoutGroup verticalLayoutGroup;

        private void Awake()
        {
            verticalLayoutGroup = elementContainer.GetComponent<VerticalLayoutGroup>();
        }

        public void Initialize(MonoBehaviour behaviour, FieldInfo[] debugFields, DebugMenuToggleable toggleable)
        {
            labelText.text = behaviour.GetType().Name; // Use the class name as the label

            foreach (FieldInfo field in debugFields)
            {
                GameObject fieldUI = null;

                if (field.FieldType == typeof(bool))
                {
                    fieldUI = DisplayFieldBool(field, behaviour);
                }
                else if (field.FieldType == typeof(int))
                {
                    fieldUI = DisplayFieldInt(field, behaviour);
                }
                else if (field.FieldType == typeof(float))
                {
                    fieldUI = DisplayFieldFloat(field, behaviour);
                }
                else if (field.FieldType == typeof(string))
                {
                    fieldUI = DisplayFieldString(field, behaviour);
                }
                else
                {
                    fieldUI = DisplayFieldDefault(field, behaviour);
                }

                if (fieldUI != null)
                {
                    debugFieldElements.Add(fieldUI);
                }
            }

            Toggle UIToggle = GetComponentInChildren<Toggle>();
            if (toggleable == DebugMenuToggleable.toggleable)
            {
                // Attach toggle to script.GameObject.SetActive
                UIToggle.onValueChanged.AddListener((bool value) =>
                {
                    behaviour.gameObject.SetActive(value);
                    SetFieldsActive(value);
                });
                UIToggle.SetIsOnWithoutNotify(behaviour.gameObject.activeSelf);
            }
            else
            {
                UIToggle.interactable = false;
                UIToggle.isOn = false;
                UIToggle.enabled = false;
            }

            SetFieldsActive(behaviour.gameObject.activeSelf);
        }

        private void SetFieldsActive(bool state)
        {
            foreach (GameObject _element in debugFieldElements)
            {
                _element.SetActive(state);
                if (verticalLayoutGroup != null)
                {
                    verticalLayoutGroup.spacing = state ? 0 : -30;
                }
                LayoutRebuilder.MarkLayoutForRebuild(elementContainer);
            }
        }

        private GameObject DisplayFieldBool(FieldInfo field, MonoBehaviour target)
        {
            GameObject fieldUI = Instantiate(boolPrefab, elementContainer);
            fieldUI.GetComponentInChildren<Text>().text = field.Name;

            Toggle toggle = fieldUI.GetComponentInChildren<Toggle>();
            toggle.isOn = (bool)field.GetValue(target);
            toggle.onValueChanged.AddListener((bool newValue) => field.SetValue(target, newValue));

            return fieldUI;
        }

        private GameObject DisplayFieldInt(FieldInfo field, MonoBehaviour target)
        {
            GameObject fieldUI = Instantiate(intPrefab, elementContainer);
            fieldUI.GetComponentInChildren<Text>().text = field.Name;

            TMP_InputField inputField = fieldUI.GetComponentInChildren<TMP_InputField>();
            inputField.text = field.GetValue(target).ToString();
            inputField.onEndEdit.AddListener((string newValue) =>
            {
                if (int.TryParse(newValue, out int intValue))
                {
                    field.SetValue(target, intValue);
                }
            });

            return fieldUI;
        }

        private GameObject DisplayFieldFloat(FieldInfo field, MonoBehaviour target)
        {
            GameObject fieldUI = Instantiate(floatPrefab, elementContainer);
            fieldUI.GetComponentInChildren<Text>().text = field.Name;

            TMP_InputField inputField = fieldUI.GetComponentInChildren<TMP_InputField>();
            inputField.text = field.GetValue(target).ToString();
            inputField.onEndEdit.AddListener((string newValue) =>
            {
                if (float.TryParse(newValue, out float floatValue))
                {
                    field.SetValue(target, floatValue);
                }
            });

            return fieldUI;
        }

        private GameObject DisplayFieldString(FieldInfo field, MonoBehaviour target)
        {
            GameObject fieldUI = Instantiate(stringPrefab, elementContainer);
            TMP_InputField inputField = fieldUI.GetComponentInChildren<TMP_InputField>();

            inputField.text = field.GetValue(target).ToString();
            inputField.onEndEdit.AddListener((string newValue) =>
            {
                field.SetValue(target, newValue);
            });

            return fieldUI;
        }

        private GameObject DisplayFieldDefault(FieldInfo field, MonoBehaviour target)
        {
            GameObject fieldUI = Instantiate(defaultPrefab, elementContainer);
            Text fieldText = fieldUI.GetComponentInChildren<Text>();

            fieldText.text = $"{field.Name}: {field.GetValue(target)}";
            return fieldUI;
        }
    }
}
