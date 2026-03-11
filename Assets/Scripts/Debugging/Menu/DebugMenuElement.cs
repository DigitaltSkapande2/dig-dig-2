using System.Collections.Generic;
using System.Reflection;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace DigDig2.Debugging.Menu
{
	public class DebugMenuElement : MonoBehaviour
	{
		[SerializeField] private TMP_Text labelText;
		[SerializeField] private RectTransform elementContainer;

		[Header( "Prefabs" )]
		[SerializeField] private GameObject boolPrefab;

		[SerializeField] private GameObject intPrefab;
		[SerializeField] private GameObject floatPrefab;
		[SerializeField] private GameObject stringPrefab;
		[SerializeField] private GameObject defaultPrefab;

		private readonly List<GameObject> debugFieldElements = new( );

		private VerticalLayoutGroup verticalLayoutGroup;

		private void Awake( ) { verticalLayoutGroup = elementContainer.GetComponent<VerticalLayoutGroup>( ); }

		public void Initialize( MonoBehaviour behaviour, FieldInfo[ ] debugFields, DebugMenuToggleable toggleable )
		{
			labelText.text = behaviour.GetType( ).Name; // Use the class name as the label

			foreach ( FieldInfo field in debugFields )
			{
				GameObject fieldUi;

				if ( field.FieldType == typeof( bool ) )
					fieldUi = DisplayFieldBool( field, behaviour );
				else if ( field.FieldType == typeof( int ) )
					fieldUi = DisplayFieldInt( field, behaviour );
				else if ( field.FieldType == typeof( float ) )
					fieldUi = DisplayFieldFloat( field, behaviour );
				else if ( field.FieldType == typeof( string ) )
					fieldUi = DisplayFieldString( field, behaviour );
				else
					fieldUi = DisplayFieldDefault( field, behaviour );

				if ( fieldUi ) debugFieldElements.Add( fieldUi );
			}

			Toggle uiToggle = GetComponentInChildren<Toggle>( );
			if ( toggleable == DebugMenuToggleable.Toggleable )
			{
				// Attach toggle to script.GameObject.SetActive
				uiToggle.onValueChanged.AddListener( value =>
					{
						behaviour.gameObject.SetActive( value );
						SetFieldsActive( value );
					}
				);

				uiToggle.SetIsOnWithoutNotify( behaviour.gameObject.activeSelf );
			} else
			{
				uiToggle.interactable = false;
				uiToggle.isOn = false;
				uiToggle.enabled = false;
			}

			SetFieldsActive( behaviour.gameObject.activeSelf );
		}

		private void SetFieldsActive( bool state )
		{
			foreach ( GameObject element in debugFieldElements )
			{
				element.SetActive( state );
				if ( verticalLayoutGroup ) verticalLayoutGroup.spacing = state ? 0 : -30;
				LayoutRebuilder.MarkLayoutForRebuild( elementContainer );
			}
		}

		private GameObject DisplayFieldBool( FieldInfo field, MonoBehaviour target )
		{
			GameObject fieldUi = Instantiate( boolPrefab, elementContainer );
			fieldUi.GetComponentInChildren<Text>( ).text = field.Name;

			Toggle toggle = fieldUi.GetComponentInChildren<Toggle>( );
			toggle.isOn = (bool)field.GetValue( target );
			toggle.onValueChanged.AddListener( newValue => field.SetValue( target, newValue ) );

			return fieldUi;
		}

		private GameObject DisplayFieldInt( FieldInfo field, MonoBehaviour target )
		{
			GameObject fieldUi = Instantiate( intPrefab, elementContainer );
			fieldUi.GetComponentInChildren<Text>( ).text = field.Name;

			TMP_InputField inputField = fieldUi.GetComponentInChildren<TMP_InputField>( );
			inputField.text = field.GetValue( target ).ToString( );
			inputField.onEndEdit.AddListener( newValue =>
				{
					if ( int.TryParse( newValue, out int intValue ) ) field.SetValue( target, intValue );
				}
			);

			return fieldUi;
		}

		private GameObject DisplayFieldFloat( FieldInfo field, MonoBehaviour target )
		{
			GameObject fieldUi = Instantiate( floatPrefab, elementContainer );
			fieldUi.GetComponentInChildren<Text>( ).text = field.Name;

			TMP_InputField inputField = fieldUi.GetComponentInChildren<TMP_InputField>( );
			inputField.text = field.GetValue( target ).ToString( );
			inputField.onEndEdit.AddListener( newValue =>
				{
					if ( float.TryParse( newValue, out float floatValue ) ) field.SetValue( target, floatValue );
				}
			);

			return fieldUi;
		}

		private GameObject DisplayFieldString( FieldInfo field, MonoBehaviour target )
		{
			GameObject fieldUi = Instantiate( stringPrefab, elementContainer );
			TMP_InputField inputField = fieldUi.GetComponentInChildren<TMP_InputField>( );

			inputField.text = field.GetValue( target ).ToString( );
			inputField.onEndEdit.AddListener( newValue => { field.SetValue( target, newValue ); } );

			return fieldUi;
		}

		private GameObject DisplayFieldDefault( FieldInfo field, MonoBehaviour target )
		{
			GameObject fieldUi = Instantiate( defaultPrefab, elementContainer );
			Text fieldText = fieldUi.GetComponentInChildren<Text>( );

			fieldText.text = $"{field.Name}: {field.GetValue( target )}";
			return fieldUi;
		}
	}
}
