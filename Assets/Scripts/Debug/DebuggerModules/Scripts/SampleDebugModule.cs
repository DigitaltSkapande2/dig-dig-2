using UnityEngine;

namespace DigDig2.Debugging
{
    // Attribute to mark this class as "for Debug"
    [Debug]
    public class SampleDebugModule : MonoBehaviour
    {
        // Will appear and be editable through the Debug Menu
        [DebugSerialized] private string name = "walter white";

        // private variable
        private int beans; 

    }
}
