using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DigDig2.Effects
{
    [Serializable]
    public struct EffectPlayer
    {
        public bool screenShake;
        public EffectInstanceData screenShakeEffectData;
        public bool cameraZoom;
        public EffectInstanceData cameraZoomEffectData;
        public bool timeSlow;
        public EffectInstanceData timeSlowEffectData;
        public bool vignettePulse;
        public VignettePulseEffectInstanceData vignettePulseEffectData;

        public void Play()
        {
            if (screenShake)
            {
                ScreenShakeEffect screenShakeEffect = EffectCore.Instance.screenShakeEffect;
                if (screenShakeEffect != null)
                {
                    screenShakeEffect.PlayEffectInstance(screenShakeEffectData);
                }
            }

            if (cameraZoom)
            {
                CameraZoomEffect cameraZoomEffect = EffectCore.Instance.cameraZoomEffect;
                if (cameraZoomEffect != null)
                {
                    cameraZoomEffect.PlayEffectInstance(cameraZoomEffectData);
                }
            }

            if (timeSlow)
            {
                TimeSlowEffect timeSlowEffect = EffectCore.Instance.timeSlowEffect;
                if (timeSlowEffect != null)
                {
                    timeSlowEffect.PlayEffectInstance(timeSlowEffectData);
                }
            }

            if (vignettePulse)
            {
                VignettePulseEffect vignettePulseEffect = EffectCore.Instance.vignettePulseEffect;
                if (vignettePulseEffect != null)
                {
                    vignettePulseEffect.PlayEffectInstance(vignettePulseEffectData);
                }
            }
        }
    }

#region Editor
    // #if UNITY_EDITOR
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EffectPlayer))]
    public class EffectPlayerDrawer : PropertyDrawer
    {
        readonly Color backPanelColor = new Color(0.12f, 0.12f, 0.12f, 0.20f);
        readonly Color subPanelColor = new Color(0.12f, 0.12f, 0.12f, 0.35f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw a background box for the whole EffectPlayer
            var boxRect = new Rect(position.x, position.y, position.width, position.height);
            EditorGUI.DrawRect(boxRect, backPanelColor);

            // Foldout header
            var headerRect = new Rect(position.x + 4, position.y + 4, position.width - 8, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            float y = headerRect.y + EditorGUIUtility.singleLineHeight + 6f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            // Helper to draw a boxed section for each effect
            void DrawEffectSection(string title, SerializedProperty enabledProp, SerializedProperty dataProp, Color bg)
            {
                var sectionRect = new Rect(position.x + 6, y, position.width - 12, lineHeight);
                // title + toggle
                EditorGUI.PropertyField(sectionRect, enabledProp, new GUIContent(title));
                y += lineHeight + spacing;

                if (enabledProp.boolValue)
                {
                    var dataHeight = EditorGUI.GetPropertyHeight(dataProp, true);
                    var dataRect = new Rect(position.x + 12, y, position.width - 24, dataHeight);
                    // slight background for the nested data
                    var bgRect = new Rect(position.x + 10, y - 2, position.width - 20, dataHeight + 4);
                    EditorGUI.DrawRect(bgRect, bg);
                    EditorGUI.PropertyField(dataRect, dataProp, true);
                    y += dataHeight + spacing;
                }
            }

            // screenShake
            var screenShakeProp = property.FindPropertyRelative("screenShake");
            var screenShakeDataProp = property.FindPropertyRelative("screenShakeEffectData");
            DrawEffectSection("Screen Shake", screenShakeProp, screenShakeDataProp, subPanelColor);

            // cameraZoom
            var cameraZoomProp = property.FindPropertyRelative("cameraZoom");
            var cameraZoomDataProp = property.FindPropertyRelative("cameraZoomEffectData");
            DrawEffectSection("Camera Zoom", cameraZoomProp, cameraZoomDataProp, subPanelColor);

            // timeSlow
            var timeSlowProp = property.FindPropertyRelative("timeSlow");
            var timeSlowDataProp = property.FindPropertyRelative("timeSlowEffectData");
            DrawEffectSection("Time Slow", timeSlowProp, timeSlowDataProp, subPanelColor);

            // vignettePulse
            var vignetteProp = property.FindPropertyRelative("vignettePulse");
            var vignetteDataProp = property.FindPropertyRelative("vignettePulseEffectData");
            DrawEffectSection("Vignette Pulse", vignetteProp, vignetteDataProp, subPanelColor);

            // Preview button
            var previewRect = new Rect(position.x + 8, y, position.width - 16, lineHeight + 4);
            GUI.enabled = Application.isPlaying && DigDig2.Effects.EffectCore.Instance != null;
            if (GUI.Button(previewRect, "Preview Selected"))
            {
                // Build a runtime EffectPlayer from serialized props and play it via EffectCore
                EffectPlayer runtime = BuildRuntimeFromSerialized(property);
                runtime.Play();
            }
            GUI.enabled = true;

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 2f; // foldout

            if (!property.isExpanded) return height;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            // screenShake
            var screenShakeProp = property.FindPropertyRelative("screenShake");
            var screenShakeDataProp = property.FindPropertyRelative("screenShakeEffectData");
            height += lineHeight + spacing;
            if (screenShakeProp.boolValue) height += EditorGUI.GetPropertyHeight(screenShakeDataProp, true) + spacing;

            // cameraZoom
            var cameraZoomProp = property.FindPropertyRelative("cameraZoom");
            var cameraZoomDataProp = property.FindPropertyRelative("cameraZoomEffectData");
            height += lineHeight + spacing;
            if (cameraZoomProp.boolValue) height += EditorGUI.GetPropertyHeight(cameraZoomDataProp, true) + spacing;

            // timeSlow
            var timeSlowProp = property.FindPropertyRelative("timeSlow");
            var timeSlowDataProp = property.FindPropertyRelative("timeSlowEffectData");
            height += lineHeight + spacing;
            if (timeSlowProp.boolValue) height += EditorGUI.GetPropertyHeight(timeSlowDataProp, true) + spacing;

            // vignette
            var vignetteProp = property.FindPropertyRelative("vignettePulse");
            var vignetteDataProp = property.FindPropertyRelative("vignettePulseEffectData");
            height += lineHeight + spacing;
            if (vignetteProp.boolValue) height += EditorGUI.GetPropertyHeight(vignetteDataProp, true) + spacing;
            // add preview button height
            height += lineHeight + spacing + 6f;
            // outer padding
            height += 12f;

            return height;
        }
    
    
        // Helper: build an EffectPlayer struct from the serialized property values
        private static EffectPlayer BuildRuntimeFromSerialized(SerializedProperty prop)
        {
            EffectPlayer ep = new EffectPlayer();

            ep.screenShake = prop.FindPropertyRelative("screenShake").boolValue;
            var ss = new EffectInstanceData();
            var ssProp = prop.FindPropertyRelative("screenShakeEffectData");
            if (ssProp != null)
            {
                ss.intensityCurve = ssProp.FindPropertyRelative("intensityCurve").animationCurveValue;
                ss.duration = ssProp.FindPropertyRelative("duration").floatValue;
                ss.intensity = ssProp.FindPropertyRelative("intensity").floatValue;
            }
            ep.screenShakeEffectData = ss;

            ep.cameraZoom = prop.FindPropertyRelative("cameraZoom").boolValue;
            var cz = new EffectInstanceData();
            var czProp = prop.FindPropertyRelative("cameraZoomEffectData");
            if (czProp != null)
            {
                cz.intensityCurve = czProp.FindPropertyRelative("intensityCurve").animationCurveValue;
                cz.duration = czProp.FindPropertyRelative("duration").floatValue;
                cz.intensity = czProp.FindPropertyRelative("intensity").floatValue;
            }
            ep.cameraZoomEffectData = cz;

            ep.timeSlow = prop.FindPropertyRelative("timeSlow").boolValue;
            var ts = new EffectInstanceData();
            var tsProp = prop.FindPropertyRelative("timeSlowEffectData");
            if (tsProp != null)
            {
                ts.intensityCurve = tsProp.FindPropertyRelative("intensityCurve").animationCurveValue;
                ts.duration = tsProp.FindPropertyRelative("duration").floatValue;
                ts.intensity = tsProp.FindPropertyRelative("intensity").floatValue;
            }
            ep.timeSlowEffectData = ts;

            ep.vignettePulse = prop.FindPropertyRelative("vignettePulse").boolValue;
            var vp = new VignettePulseEffectInstanceData();
            var vpProp = prop.FindPropertyRelative("vignettePulseEffectData");
            if (vpProp != null)
            {
                vp.intensityCurve = vpProp.FindPropertyRelative("intensityCurve").animationCurveValue;
                vp.duration = vpProp.FindPropertyRelative("duration").floatValue;
                vp.intensity = vpProp.FindPropertyRelative("intensity").floatValue;
                vp.color = vpProp.FindPropertyRelative("color").colorValue;
            }
            ep.vignettePulseEffectData = vp;

            return ep;
        }
    }

    #endif
    #endregion
}
