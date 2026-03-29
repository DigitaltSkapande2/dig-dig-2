using System;
using System.Collections.Generic;
using DigDig2.Debugging;
using UnityEngine;
using Newtonsoft.Json;

namespace DigDig2.SaveSystem
{
    
    public class OrderedPropSaver: MonoBehaviour, ISaveable
    {
        [SerializeField] private string savePropKey;
        [SerializeField] private GameObject[] orderedPropSaveables;

        public int activeIndex { get; private set; }

        private void Start()
        {
            SaveManager.Instance.RegisterSavable(savePropKey, this);
        }

        public object CollectData()
        {
            return activeIndex;
        }

        public void RestoreState(object dataObject)
        {
            if (dataObject == null)
            {
                activeIndex = 0;
            }
            else
            {
                activeIndex = JsonConvert.DeserializeObject<int>(dataObject.ToString());
            }
            
            
            for (int i = orderedPropSaveables.Length-1; i >= 0; i--)
            {
                if (orderedPropSaveables[i].TryGetComponent<IOrderedPropSaveable>(out var orderedProp))
                {
                    orderedProp.OnLoaded(i, activeIndex, this);
                }
                else
                {
                    BetterDebug.Log("GameObject in Array does not have a IOrderedPropSaveable attached", LogSeverity.Error);
                }
                
            }
        }

        public void SetActiveIndex(int i)
        {
            activeIndex = i;
        }

        public void IncrementActiveIndex()
        {
            activeIndex++;
        }
    }
}