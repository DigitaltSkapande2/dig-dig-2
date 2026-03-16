using System;

using UnityEditor;

using UnityEngine;

namespace DigDig2.EditorAddons
{
	[AttributeUsage( AttributeTargets.Field )]
	public class ReadOnlyAttribute : Attribute
	{ }

	[CustomPropertyDrawer( typeof( ReadOnlyAttribute ) )]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUI.GetPropertyHeight( property, label, true );

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			GUI.enabled = false;
			EditorGUI.PropertyField( position, property, label, true );
			GUI.enabled = true;
		}
	}
}
