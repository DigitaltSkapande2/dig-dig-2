using System.Linq;
using Mirror;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace DigDig2
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] TMP_Text debugStartGameText;
        void Start()
        {
            if (NetworkServer.active)
            {
                Destroy(debugStartGameText);
            }
        
            
        }
        
        

    }
}
