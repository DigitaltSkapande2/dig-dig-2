using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

using DigDig2.Debugging;

using Object = UnityEngine.Object;

namespace DigDig2.Util {
	// Credit to Tibère B in https://stackoverflow.com/questions/63778384/unity-how-to-update-an-object-when-a-serialized-field-is-changed
	public class OnChangedCallAttribute : PropertyAttribute
	{
		public readonly string methodName;
		public OnChangedCallAttribute(string methodNameNoArguments)
		{
			methodName = methodNameNoArguments;
		}
	}

	#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(OnChangedCallAttribute))]
	public class OnChangedCallAttributePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(position, property, label);
			if (!EditorGUI.EndChangeCheck()) return;

			Object targetObject = property.serializedObject.targetObject;
        
			var callAttribute = attribute as OnChangedCallAttribute;
			string methodName = callAttribute?.methodName;

			Type classType = targetObject.GetType();
			MethodInfo methodInfo = classType.GetMethods().FirstOrDefault(info => info.Name == methodName);

			// Update the serialized field
			property.serializedObject.ApplyModifiedProperties();
        
			// If we found a public function with the given name that takes no parameters, invoke it
			if (methodInfo != null && !methodInfo.GetParameters().Any())
			{
				methodInfo.Invoke(targetObject, null);
			}
			else
			{
				// TODO: Create proper exception
				BetterDebug.Log($"OnChangedCall error : No public function taking no " +
					$"argument named {methodName} in class {classType.Name}", LogSeverity.Error);
			}
		}
	}
	#endif

}
