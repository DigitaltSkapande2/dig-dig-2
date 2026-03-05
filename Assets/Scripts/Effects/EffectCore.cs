using System;
using UnityEngine;

namespace DigDig2.Effects
{
    [RequireComponent(typeof(SpawnPrefabEffect))]
    [RequireComponent(typeof(ScreenShakeEffect))]
    [RequireComponent(typeof(CameraZoomEffect))]
    [RequireComponent(typeof(TimeSlowEffect))]
    [RequireComponent(typeof(VignettePulseEffect))]
    [RequireComponent(typeof(GreyscalePulseEffect))]
    public class EffectCore : Singleton<EffectCore>
    {
        [NonSerialized] public SpawnPrefabEffect spawnPrefabEffect;
        [NonSerialized] public ScreenShakeEffect screenShakeEffect;
        [NonSerialized] public CameraZoomEffect cameraZoomEffect;
        [NonSerialized] public TimeSlowEffect timeSlowEffect;
        [NonSerialized] public VignettePulseEffect vignettePulseEffect;
        [NonSerialized] public GreyscalePulseEffect GreyscalePulseEffect;

        protected override void Awake()
        {
            base.Awake();
            spawnPrefabEffect = GetComponent<SpawnPrefabEffect>();
            screenShakeEffect = GetComponent<ScreenShakeEffect>();
            cameraZoomEffect = GetComponent<CameraZoomEffect>();
            timeSlowEffect = GetComponent<TimeSlowEffect>();
            vignettePulseEffect = GetComponent<VignettePulseEffect>();
            GreyscalePulseEffect = GetComponent<GreyscalePulseEffect>();
        }
    }
}
