using System;
using UnityEngine;
using DigDig2.Audio;
using DigDig2.EffectSystem.Effects;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DigDig2.EffectSystem
{
    
    [CreateAssetMenu( fileName = "NewEffectPlayer" )]
	public class EffectPlayer : ScriptableObject
	{
		public bool spawnPrefab;
		public SpawnPrefabEffectData spawnPrefabEffectData;

		public bool playSound;
		public AudioClip[] audioClipsToPlay;

		public bool screenShake;
		public CumulativeEffectInstanceData screenShakeEffectData;

		public bool cameraZoom;
		public CumulativeEffectInstanceData cameraZoomEffectData;

		public bool timeSlow;
		public CumulativeEffectInstanceData timeSlowEffectData;

		public bool vignettePulse;
		public VignettePulseEffectInstanceData vignettePulseEffectData;

		public bool gamepadRumble;
		public GamepadRumbleEffectInstanceData gamepadRumbleEffectData;

		public bool greyscale;
		public CumulativeEffectInstanceData greyscaleEffectData;

		public void Play( Vector3 position = default, Quaternion rotation = default,
		                  Vector3 scale = default, Transform parent = null, int inputPlayerID = -1 )
		{
			if ( scale == Vector3.zero ) scale = Vector3.one;

            EffectCore effectCore = EffectCore.Instance;

			if ( spawnPrefab )
			{
				SpawnPrefabEffect spawnPrefabEffect = effectCore.spawnPrefabEffect;
				if ( spawnPrefabEffect )
				{
					SpawnPrefabEffectData effectInstance = spawnPrefabEffectData;
					effectInstance.position = position;
					effectInstance.rotation = rotation;
					effectInstance.scale    = scale;
					effectInstance.parent   = parent;
					spawnPrefabEffect.PlayEffectInstance( effectInstance );
				}
			}

			if ( playSound )
			{
				foreach ( var clip in audioClipsToPlay )
					AudioManager.Instance?.PlaySound( clip );
			}

			if ( screenShake )
			{
				ScreenShakeEffect fx = effectCore.screenShakeEffect;
				if ( fx ) fx.PlayEffectInstance( screenShakeEffectData );
			}

			if ( cameraZoom )
			{
				CameraZoomEffect fx = effectCore.cameraZoomEffect;
				if ( fx ) fx.PlayEffectInstance( cameraZoomEffectData );
			}

			if ( timeSlow )
			{
				TimeSlowEffect fx = effectCore.timeSlowEffect;
				if ( fx ) fx.PlayEffectInstance( timeSlowEffectData );
			}

			if ( vignettePulse )
			{
				VignettePulseEffect fx = effectCore.vignettePulseEffect;
				if ( fx ) fx.PlayEffectInstance( vignettePulseEffectData );
			}

			if ( gamepadRumble )
			{
                effectCore.gamepadRumbleEffect.PlayEffectInstance( gamepadRumbleEffectData, inputPlayerID );
			}

			if ( greyscale )
			{
				TimeSlowEffect fx = effectCore.timeSlowEffect;
				if ( fx ) fx.PlayEffectInstance( greyscaleEffectData );
			}
		}
	}
    
#if UNITY_EDITOR
    
	[CustomEditor( typeof( EffectPlayer ) )]
	public class EffectPlayerAssetEditor : Editor
	{
		static readonly Color k_BgDark        = new( 0.13f, 0.13f, 0.13f, 1f );
		static readonly Color k_HeaderEnabled  = new( 0.22f, 0.48f, 0.32f, 1f );   // muted green
		static readonly Color k_HeaderDisabled = new( 0.22f, 0.22f, 0.22f, 1f );   // grey
		static readonly Color k_BodyBg         = new( 0.17f, 0.17f, 0.17f, 1f );
		static readonly Color k_Outline        = new( 0.08f, 0.08f, 0.08f, 1f );
		static readonly Color k_PreviewBtn     = new( 0.20f, 0.40f, 0.60f, 1f );
		static readonly Color k_PreviewHot     = new( 0.26f, 0.52f, 0.78f, 1f );
 
		const float k_Radius   = 4f;
		const float k_Pad      = 6f;
		const float k_HdrH     = 22f;
		const float k_Spacing  = 4f;
        
		struct Section
		{
			public string Label;
			public string EnabledField;
			public string DataField;
		}
 
		static readonly Section[] k_Sections =
		{
			new() { Label = "⬡  Spawn Prefab",   EnabledField = "spawnPrefab",    DataField = "spawnPrefabEffectData" },
			new() { Label = "♪  Play Sound",      EnabledField = "playSound",      DataField = "audioClipsToPlay"      },
			new() { Label = "⌖  Screen Shake",    EnabledField = "screenShake",    DataField = "screenShakeEffectData" },
			new() { Label = "⊙  Camera Zoom",     EnabledField = "cameraZoom",     DataField = "cameraZoomEffectData"  },
			new() { Label = "⧗  Time Slow",       EnabledField = "timeSlow",       DataField = "timeSlowEffectData"    },
			new() { Label = "◉  Vignette Pulse",  EnabledField = "vignettePulse",  DataField = "vignettePulseEffectData"},
			new() { Label = "⌨  Gamepad Rumble",  EnabledField = "gamepadRumble",  DataField = "gamepadRumbleEffectData"},
			new() { Label = "◫  Greyscale",       EnabledField = "greyscale",      DataField = "greyscaleEffectData"   },
		};
        
		bool[] _expanded;
 
		void OnEnable()
		{
			_expanded = new bool[k_Sections.Length];
		}
        
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
            
			var bgRect = GUILayoutUtility.GetRect( 0, 0 );
 
			EditorGUILayout.Space( 4 );
 
			for ( int i = 0; i < k_Sections.Length; i++ )
				DrawSection( i );
 
			EditorGUILayout.Space( 6 );
			DrawPreviewButton();
			EditorGUILayout.Space( 4 );
 
			serializedObject.ApplyModifiedProperties();
		}
 
		void DrawSection( int i )
		{
			var section    = k_Sections[ i ];
			var enabledProp = serializedObject.FindProperty( section.EnabledField );
			var dataProp    = serializedObject.FindProperty( section.DataField );
			bool enabled    = enabledProp.boolValue;
            
			EditorGUILayout.BeginVertical();
 
			// Header row
			var headerRect = GUILayoutUtility.GetRect( 0, k_HdrH + k_Pad );
			headerRect.x     += k_Pad;
			headerRect.width -= k_Pad * 2;
 
			Color hdrColor = enabled ? k_HeaderEnabled : k_HeaderDisabled;
			DrawRoundRect( headerRect, hdrColor, k_Radius );
 
			// toggle
			EditorGUI.BeginChangeCheck();
			bool newVal = EditorGUI.Toggle(
				new Rect( headerRect.x + 4, headerRect.y + 3, 16, k_HdrH ),
				enabled );
			if ( EditorGUI.EndChangeCheck() )
			{
				enabledProp.boolValue = newVal;
				enabled               = newVal;
				serializedObject.ApplyModifiedProperties();
			}
 
			// label
			var labelStyle = new GUIStyle( EditorStyles.boldLabel )
			{
				normal    = { textColor = enabled ? Color.white : new Color( 0.55f, 0.55f, 0.55f ) },
				fontSize  = 11,
				alignment = TextAnchor.MiddleLeft,
			};
			GUI.Label(
				new Rect( headerRect.x + 24, headerRect.y, headerRect.width - 70, k_HdrH + k_Pad ),
				section.Label, labelStyle );
 
			// expand chevron
			if ( enabled )
			{
				string chevron   = _expanded[ i ] ? "▲" : "▼";
				var    chevStyle = new GUIStyle( EditorStyles.miniLabel )
				{
					normal    = { textColor = new Color( 0.8f, 0.8f, 0.8f ) },
					alignment = TextAnchor.MiddleRight,
				};
				GUI.Label(
					new Rect( headerRect.xMax - 28, headerRect.y, 20, k_HdrH + k_Pad ),
					chevron, chevStyle );
 
				// click to expand
				if ( Event.current.type == EventType.MouseDown && headerRect.Contains( Event.current.mousePosition ) )
				{
					_expanded[ i ] = !_expanded[ i ];
					Event.current.Use();
					Repaint();
				}
			}
			else
			{
				_expanded[ i ] = false;
			}

			if ( enabled && _expanded[ i ] && dataProp != null )
			{
				const float inset      = 8f;
				const float innerPadX  = 20f;
				const float innerPadY  = 8f;
 
				float dataH    = EditorGUI.GetPropertyHeight( dataProp, true );
				float cardH    = dataH + innerPadY * 2;
 
				// Reserve the full card rect in the layout
				var cardRect = GUILayoutUtility.GetRect( 0, cardH + k_Spacing );
				cardRect.x     += inset;
				cardRect.width -= inset * 2;
				cardRect.height = cardH;
                
				DrawRoundRect( new Rect( cardRect.x - 1, cardRect.y - 1,
				                        cardRect.width + 2, cardRect.height + 2 ),
				               k_Outline, k_Radius );
				DrawRoundRect( cardRect, k_BodyBg, k_Radius );
 

				var propRect = new Rect(
					cardRect.x      + innerPadX,
					cardRect.y      + innerPadY,
					cardRect.width  - innerPadX * 2,
					dataH );
                
                
				int prevIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				EditorGUI.PropertyField( propRect, dataProp, GUIContent.none, true );
				EditorGUI.indentLevel = prevIndent;
			}
 
			EditorGUILayout.Space( k_Spacing );
			EditorGUILayout.EndVertical();
		}
 
		void DrawPreviewButton()
		{
			bool canPreview = Application.isPlaying && EffectCore.Instance;
 
			var  btnRect = GUILayoutUtility.GetRect( 0, 28 );
			btnRect.x     += k_Pad;
			btnRect.width -= k_Pad * 2;
 
			Color btnColor = canPreview ? k_PreviewBtn : new Color( 0.25f, 0.25f, 0.25f );
 
			// hover tint
			if ( canPreview && btnRect.Contains( Event.current.mousePosition ) )
				btnColor = k_PreviewHot;
 
			DrawRoundRect( btnRect, btnColor, k_Radius );
 
			var btnStyle = new GUIStyle( EditorStyles.boldLabel )
			{
				alignment = TextAnchor.MiddleCenter,
				normal    = { textColor = canPreview ? Color.white : new Color( 0.45f, 0.45f, 0.45f ) },
				fontSize  = 11,
			};
			GUI.Label( btnRect, canPreview ? "▶  Preview" : "▶  Preview  (play mode only)", btnStyle );
 
			if ( canPreview && Event.current.type == EventType.MouseDown && btnRect.Contains( Event.current.mousePosition ) )
			{
				( (EffectPlayer) target ).Play();
				Event.current.Use();
			}
		}
        
		static void DrawRoundRect( Rect r, Color color, float radius )
		{
			var prev = GUI.color;
			GUI.color = color;
 
			// centre fill
			GUI.DrawTexture( new Rect( r.x + radius, r.y, r.width - radius * 2, r.height ), Texture2D.whiteTexture );
			// left / right strips
			GUI.DrawTexture( new Rect( r.x, r.y + radius, radius, r.height - radius * 2 ), Texture2D.whiteTexture );
			GUI.DrawTexture( new Rect( r.xMax - radius, r.y + radius, radius, r.height - radius * 2 ), Texture2D.whiteTexture );
			// corners
			GUI.DrawTexture( new Rect( r.x,          r.y,          radius, radius ), Texture2D.whiteTexture );
			GUI.DrawTexture( new Rect( r.xMax-radius, r.y,          radius, radius ), Texture2D.whiteTexture );
			GUI.DrawTexture( new Rect( r.x,          r.yMax-radius, radius, radius ), Texture2D.whiteTexture );
			GUI.DrawTexture( new Rect( r.xMax-radius, r.yMax-radius, radius, radius ), Texture2D.whiteTexture );
 
			GUI.color = prev;
		}
	}
    
#endif
}