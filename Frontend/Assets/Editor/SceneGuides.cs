using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneGuides
{
    static SceneGuides()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sv)
    {
        Handles.color = Color.green;

        // Vertical
        for (int i = -30; i <= 30; i += 5)
            Handles.DrawLine(new Vector3(i, -1000, 0), new Vector3(i, 1000, 0));

        // Horizontal
        for (int i = -20; i <= 20; i += 5)
            Handles.DrawLine(new Vector3(-1000, i, 0), new Vector3(1000, i, 0));
    }
}
