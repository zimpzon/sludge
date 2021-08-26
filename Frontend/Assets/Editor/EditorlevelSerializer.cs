using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorLevelSerializer : MonoBehaviour
{
    [MenuItem("Tools/Sludge/Save Level")]
    private static void Save()
    {
        var levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        var levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();

        var level = LevelSerializer.Run(levelElements, levelSettings);
        string json = JsonConvert.SerializeObject(level);

        string fileName = level.Name;
        foreach (var c in Path.GetInvalidFileNameChars()) { fileName = fileName.Replace(c, '-'); }

        string filePath = EditorUtility.SaveFilePanel("Save level", "Assets/Resources/Levels", fileName, "json");
        if (string.IsNullOrEmpty(filePath))
            return;

        File.WriteAllText(filePath, json);
        File.Delete(filePath + ".meta"); // FU, Unity
        AssetDatabase.Refresh();
    }
}
