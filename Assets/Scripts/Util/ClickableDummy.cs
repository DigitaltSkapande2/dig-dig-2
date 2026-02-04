using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DigDig2
{
    public class ClickableDummy : MonoBehaviour 
    {
        public UnityEvent mouseEnter = new UnityEvent();
        public UnityEvent mouseExit = new UnityEvent();
        void OnMouseOver()
        {
            mouseEnter.Invoke();
            Debug.Log("its in");
        }

        void OnMouseExit()
        {
            mouseExit.Invoke();
            Debug.Log("Its Out");
        }
    }
}