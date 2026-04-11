using System;
using DigDig2.SaveSystem;
using UnityEngine;

namespace DigDig2.Util
{
    public class MultiplayerOnlyObject : MonoBehaviour
    {
        private void Start()
        {
            if (!SaveManager.Instance.isMultiplayer) Destroy(gameObject);
        }
    }
}