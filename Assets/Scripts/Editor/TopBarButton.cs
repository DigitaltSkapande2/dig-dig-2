using UnityEditor;
using UnityEngine;

public class TopBarButton
{
    [MenuItem("Tools/Recompile Scripts 🚀 %r")]
    private static void PrintMessage()
    {
        AssetDatabase.Refresh();
    }
}