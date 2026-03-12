using System;

using DigDig2.EffectSystem.Effects;

using UnityEditor;

using UnityEngine;

namespace DigDig2.EffectSystem
{
	[Serializable]
	public struct EffectPlayer
	{
		public bool spawnPrefab;
		public SpawnPrefabEffectData spawnPrefabEffectData;
		public bool screenShake;
		public CumulativeEffectInstanceData screenShakeEffectData;
		public bool cameraZoom;
		public CumulativeEffectInstanceData cameraZoomEffectData;
		public bool timeSlow;
		public CumulativeEffectInstanceData timeSlowEffectData;
		public bool vignettePulse;
		public VignettePulseEffectInstanceData vignettePulseEffectData;
		public bool greyscale;
		public CumulativeEffectInstanceData greyscaleEffectData;

		public void Play( Vector3 position = default, Quaternion rotation = default, Vector3 scale = default, Transform parent = null )
		{
			if ( scale == Vector3.zero ) scale = Vector3.one;

			if ( spawnPrefab )
			{
				SpawnPrefabEffect spawnPrefabEffect = EffectCore.Instance.spawnPrefabEffect;
				if ( spawnPrefabEffect )
				{
					SpawnPrefabEffectData effectInstance = spawnPrefabEffectData;
					effectInstance.position = position;
					effectInstance.rotation = rotation;
					effectInstance.scale = scale;
					effectInstance.parent = parent;
					spawnPrefabEffect.PlayEffectInstance( effectInstance );
				}
			}

			if ( screenShake )
			{
				ScreenShakeEffect screenShakeEffect = EffectCore.Instance.screenShakeEffect;
				if ( screenShakeEffect ) screenShakeEffect.PlayEffectInstance( screenShakeEffectData );
			}

			if ( cameraZoom )
			{
				CameraZoomEffect cameraZoomEffect = EffectCore.Instance.cameraZoomEffect;
				if ( cameraZoomEffect ) cameraZoomEffect.PlayEffectInstance( cameraZoomEffectData );
			}

			if ( timeSlow )
			{
				TimeSlowEffect timeSlowEffect = EffectCore.Instance.timeSlowEffect;
				if ( timeSlowEffect ) timeSlowEffect.PlayEffectInstance( timeSlowEffectData );
			}

			if ( vignettePulse )
			{
				VignettePulseEffect vignettePulseEffect = EffectCore.Instance.vignettePulseEffect;
				if ( vignettePulseEffect ) vignettePulseEffect.PlayEffectInstance( vignettePulseEffectData );
			}

			if ( greyscale )
			{
				TimeSlowEffect greyscaleEffect = EffectCore.Instance.timeSlowEffect;
				if ( greyscaleEffect ) greyscaleEffect.PlayEffectInstance( greyscaleEffectData );
			}
		}
	}

	#region Editor

	#if UNITY_EDITOR
	[CustomPropertyDrawer( typeof( EffectPlayer ) )]
	public class EffectPlayerDrawer : PropertyDrawer
	{
		private readonly Color backPanelColor = new( 0.12f, 0.12f, 0.12f, 0.20f );
		private readonly Color subPanelColor = new( 0.12f, 0.12f, 0.12f, 0.35f );

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			// Draw a background box for the whole EffectPlayer
			var boxRect = new Rect( position.x, position.y, position.width, position.height );
			EditorGUI.DrawRect( boxRect, backPanelColor );

			// Foldout header
			var headerRect = new Rect( position.x + 4, position.y + 4, position.width - 8, EditorGUIUtility.singleLineHeight );
			property.isExpanded = EditorGUI.Foldout( headerRect, property.isExpanded, label, true );

			if ( !property.isExpanded )
			{
				EditorGUI.EndProperty( );
				return;
			}

			EditorGUI.indentLevel++;

			float y = headerRect.y + EditorGUIUtility.singleLineHeight + 6f;
			float lineHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2f;

			// Helper to draw a boxed section for each effect
			void DrawEffectSection( string title, SerializedProperty enabledProp, SerializedProperty dataProp, Color bg )
			{
				var sectionRect = new Rect( position.x + 6, y, position.width - 12, lineHeight );

				// title + toggle
				EditorGUI.PropertyField( sectionRect, enabledProp, new GUIContent( title ) );
				y += lineHeight + spacing;

				if ( enabledProp.boolValue )
				{
					float dataHeight = EditorGUI.GetPropertyHeight( dataProp, true );
					var dataRect = new Rect( position.x + 12, y, position.width - 24, dataHeight );

					// slight background for the nested data
					var bgRect = new Rect( position.x + 10, y - 2, position.width - 20, dataHeight + 4 );
					EditorGUI.DrawRect( bgRect, bg );
					EditorGUI.PropertyField( dataRect, dataProp, true );
					y += dataHeight + spacing;
				}
			}

			// SpawnPrefab
			SerializedProperty spawnPrefabProp = property.FindPropertyRelative( "spawnPrefab" );
			SerializedProperty spawnPrefabDataProp = property.FindPropertyRelative( "spawnPrefabEffectData" );
			DrawEffectSection( "Spawn Prefab", spawnPrefabProp, spawnPrefabDataProp, subPanelColor );

			// screenShake
			SerializedProperty screenShakeProp = property.FindPropertyRelative( "screenShake" );
			SerializedProperty screenShakeDataProp = property.FindPropertyRelative( "screenShakeEffectData" );
			DrawEffectSection( "Screen Shake", screenShakeProp, screenShakeDataProp, subPanelColor );

			// cameraZoom
			SerializedProperty cameraZoomProp = property.FindPropertyRelative( "cameraZoom" );
			SerializedProperty cameraZoomDataProp = property.FindPropertyRelative( "cameraZoomEffectData" );
			DrawEffectSection( "Camera Zoom", cameraZoomProp, cameraZoomDataProp, subPanelColor );

			// timeSlow
			SerializedProperty timeSlowProp = property.FindPropertyRelative( "timeSlow" );
			SerializedProperty timeSlowDataProp = property.FindPropertyRelative( "timeSlowEffectData" );
			DrawEffectSection( "Time Slow", timeSlowProp, timeSlowDataProp, subPanelColor );

			// vignettePulse
			SerializedProperty vignetteProp = property.FindPropertyRelative( "vignettePulse" );
			SerializedProperty vignetteDataProp = property.FindPropertyRelative( "vignettePulseEffectData" );
			DrawEffectSection( "Vignette Pulse", vignetteProp, vignetteDataProp, subPanelColor );

			// greyscale
			SerializedProperty greyscaleProp = property.FindPropertyRelative( "greyscale" );
			SerializedProperty greyscaleDataProp = property.FindPropertyRelative( "greyscaleEffectData" );
			DrawEffectSection( "Greyscale", greyscaleProp, greyscaleDataProp, subPanelColor );

			// Preview button
			var previewRect = new Rect( position.x + 8, y, position.width - 16, lineHeight + 4 );
			GUI.enabled = Application.isPlaying && EffectCore.Instance;
			if ( GUI.Button( previewRect, "Preview Selected" ) )
			{
				// Build a runtime EffectPlayer from serialized props and play it via EffectCore
				EffectPlayer runtime = BuildRuntimeFromSerialized( property );
				runtime.Play( );
			}

			GUI.enabled = true;

			EditorGUI.indentLevel--;

			EditorGUI.EndProperty( );
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			float height = EditorGUIUtility.singleLineHeight + 2f; // foldout

			if ( !property.isExpanded ) return height;

			float lineHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2f;

			SerializedProperty spawnPrefabProp = property.FindPropertyRelative( "spawnPrefab" );
			SerializedProperty spawnPrefabDataProp = property.FindPropertyRelative( "spawnPrefabEffectData" );
			height += lineHeight + spacing;
			if ( spawnPrefabProp.boolValue ) height += EditorGUI.GetPropertyHeight( spawnPrefabDataProp, true ) + spacing;

			// screenShake
			SerializedProperty screenShakeProp = property.FindPropertyRelative( "screenShake" );
			SerializedProperty screenShakeDataProp = property.FindPropertyRelative( "screenShakeEffectData" );
			height += lineHeight + spacing;
			if ( screenShakeProp.boolValue ) height += EditorGUI.GetPropertyHeight( screenShakeDataProp, true ) + spacing;

			// cameraZoom
			SerializedProperty cameraZoomProp = property.FindPropertyRelative( "cameraZoom" );
			SerializedProperty cameraZoomDataProp = property.FindPropertyRelative( "cameraZoomEffectData" );
			height += lineHeight + spacing;
			if ( cameraZoomProp.boolValue ) height += EditorGUI.GetPropertyHeight( cameraZoomDataProp, true ) + spacing;

			// timeSlow
			SerializedProperty timeSlowProp = property.FindPropertyRelative( "timeSlow" );
			SerializedProperty timeSlowDataProp = property.FindPropertyRelative( "timeSlowEffectData" );
			height += lineHeight + spacing;
			if ( timeSlowProp.boolValue ) height += EditorGUI.GetPropertyHeight( timeSlowDataProp, true ) + spacing;

			// vignette
			SerializedProperty vignetteProp = property.FindPropertyRelative( "vignettePulse" );
			SerializedProperty vignetteDataProp = property.FindPropertyRelative( "vignettePulseEffectData" );
			height += lineHeight + spacing;
			if ( vignetteProp.boolValue ) height += EditorGUI.GetPropertyHeight( vignetteDataProp, true ) + spacing;

			// timeSlow
			SerializedProperty greyscaleProp = property.FindPropertyRelative( "greyscale" );
			SerializedProperty greyscaleDataProp = property.FindPropertyRelative( "greyscaleEffectData" );
			height += lineHeight + spacing;
			if ( greyscaleProp.boolValue ) height += EditorGUI.GetPropertyHeight( greyscaleDataProp, true ) + spacing;

			// add preview button height
			height += lineHeight + spacing + 6f;

			// outer padding
			height += 12f;

			return height;
		}

		// Helper: build an EffectPlayer struct from the serialized property values
		private static EffectPlayer BuildRuntimeFromSerialized( SerializedProperty prop )
		{
			var ep = new EffectPlayer
			{
				spawnPrefab = prop.FindPropertyRelative( "spawnPrefab" ).boolValue
			};

			var sp = new SpawnPrefabEffectData( );
			SerializedProperty spProp = prop.FindPropertyRelative( "spawnPrefabEffectData" );
			if ( spProp != null )
			{
				// sp.prefabToSpawn = spProp.FindPropertyRelative("prefabToSpawn").objectReferenceValue.GetComponent<SpawnPrefabEffectInstance>().prefabToSpawn;
			}

			ep.spawnPrefabEffectData = sp;

			ep.screenShake = prop.FindPropertyRelative( "screenShake" ).boolValue;
			var ss = new CumulativeEffectInstanceData( );
			SerializedProperty ssProp = prop.FindPropertyRelative( "screenShakeEffectData" );
			if ( ssProp != null )
			{
				ss.intensityCurve = ssProp.FindPropertyRelative( "intensityCurve" ).animationCurveValue;
				ss.duration = ssProp.FindPropertyRelative( "duration" ).floatValue;
				ss.intensity = ssProp.FindPropertyRelative( "intensity" ).floatValue;
			}

			ep.screenShakeEffectData = ss;

			ep.cameraZoom = prop.FindPropertyRelative( "cameraZoom" ).boolValue;
			var cz = new CumulativeEffectInstanceData( );
			SerializedProperty czProp = prop.FindPropertyRelative( "cameraZoomEffectData" );
			if ( czProp != null )
			{
				cz.intensityCurve = czProp.FindPropertyRelative( "intensityCurve" ).animationCurveValue;
				cz.duration = czProp.FindPropertyRelative( "duration" ).floatValue;
				cz.intensity = czProp.FindPropertyRelative( "intensity" ).floatValue;
			}

			ep.cameraZoomEffectData = cz;

			ep.timeSlow = prop.FindPropertyRelative( "timeSlow" ).boolValue;
			var ts = new CumulativeEffectInstanceData( );
			SerializedProperty tsProp = prop.FindPropertyRelative( "timeSlowEffectData" );
			if ( tsProp != null )
			{
				ts.intensityCurve = tsProp.FindPropertyRelative( "intensityCurve" ).animationCurveValue;
				ts.duration = tsProp.FindPropertyRelative( "duration" ).floatValue;
				ts.intensity = tsProp.FindPropertyRelative( "intensity" ).floatValue;
			}

			ep.timeSlowEffectData = ts;

			ep.vignettePulse = prop.FindPropertyRelative( "vignettePulse" ).boolValue;
			var vp = new VignettePulseEffectInstanceData( );
			SerializedProperty vpProp = prop.FindPropertyRelative( "vignettePulseEffectData" );
			if ( vpProp != null )
			{
				vp.intensityCurve = vpProp.FindPropertyRelative( "intensityCurve" ).animationCurveValue;
				vp.duration = vpProp.FindPropertyRelative( "duration" ).floatValue;
				vp.intensity = vpProp.FindPropertyRelative( "intensity" ).floatValue;
				vp.color = vpProp.FindPropertyRelative( "color" ).colorValue;
			}

			ep.vignettePulseEffectData = vp;

			return ep;
		}
	}

	#endif

	#endregion
}
