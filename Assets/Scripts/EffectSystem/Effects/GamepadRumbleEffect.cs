using System;
using System.Collections.Generic;
using System.Linq;
using DigDig2.Debugging;
using DigDig2.Game;
using DigDig2.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.EffectSystem.Effects
{
    [System.Serializable]
    public class GamepadRumbleEffectInstanceData : CumulativeEffectInstanceData
    {
        public int inputPlayerId;
        public float highFrequencyIntensity;

        public override object Clone() => MemberwiseClone();
    }

    public class GamepadRumbleEffect : Effect<CumulativeEffectInstanceData>
    {
        private new Dictionary<Gamepad, List<GamepadRumbleEffectInstanceData>> effectInstances = new();


        public void PlayEffectInstance(GamepadRumbleEffectInstanceData effectInstance, int id)
        {
            BetterDebug.Log("Added GamepadRuimble");

            List<InputDevice> affectedDevices;
            if (id == -1)
            {
                InputManager inputManager = InputManager.Instance;
                affectedDevices = GameManager.Instance.playerControllers.Where(p => p).Select(p => inputManager.GetInputPlayersDevices(p.inputPlayerIndex)[0]).ToList();
            }
            else
            {
                affectedDevices = InputManager.Instance.GetInputPlayersDevices(id);
            }
            
            
            
            foreach (InputDevice inputPlayersDevice in affectedDevices)
            {
                if (!(inputPlayersDevice is Gamepad)) continue;

                Gamepad gamePad = (Gamepad)inputPlayersDevice;
                
                GamepadRumbleEffectInstanceData copy = (GamepadRumbleEffectInstanceData)effectInstance.Clone( );
                
                if (effectInstances.TryGetValue(gamePad, out var effectInstanceDatas))
                {
                    if (effectInstanceDatas.Count == 0)
                    {
                        gamePad.ResumeHaptics();
                    }
                    
                    effectInstanceDatas.Add(copy);
                }
                else
                {
                    effectInstances.Add(gamePad, new List<GamepadRumbleEffectInstanceData>() { copy });
                }
            }
            
            
        }

        internal override void OnEffectStart(CumulativeEffectInstanceData effect)
        {
            // THIS IS NEVER CALLED, IMLEMENT IF NEEDED
        }

        public new void Update()
        {
            foreach (Gamepad gamepad in effectInstances.Keys)
            {
                float power = 0f;
                float highFrequencyPower = 0f;
                if (effectInstances.TryGetValue(gamepad, out List<GamepadRumbleEffectInstanceData> datalist))
                {
                    for (int i = datalist.Count - 1; i >= 0; i--)
                    {
                        GamepadRumbleEffectInstanceData effect = datalist[i];
                        if (effect == null)
                        {
                            datalist.RemoveAt(i);
                            continue;
                        }
                        
                        effect.durationPassed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                        // expired?
                        if (effect.durationPassed > effect.duration)
                        {
                            OnEffectEnd(gamepad, effect);
                            datalist.RemoveAt(i);
                        }
                        
                        float normalized = Mathf.Clamp01( effect.durationPassed / Mathf.Max( float.Epsilon, effect.duration ) );
                        float curveValue = effect.intensityCurve.Evaluate( normalized );
                        power += curveValue * effect.intensity;
                        highFrequencyPower += curveValue * effect.highFrequencyIntensity;
                    }
                }

                power = Mathf.Clamp01(power);
                highFrequencyPower = Mathf.Clamp01(highFrequencyPower);
                
                if (datalist.Count > 0) BetterDebug.Log($"Gamepad Rumble ({gamepad.name}, low {power}, high {highFrequencyPower}) effects: {datalist.Count}");
                gamepad.SetMotorSpeeds(power, highFrequencyPower);
            }
        }

        internal void OnEffectEnd(Gamepad gamepad, GamepadRumbleEffectInstanceData effect)
        {

            if (effectInstances[gamepad].Count == 0)
            {
                gamepad.ResetHaptics();
                gamepad.PauseHaptics();
            }
            base.OnEffectEnd(effect);
        }
    }
}