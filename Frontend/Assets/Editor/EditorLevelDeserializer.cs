using Newtonsoft.Json;
using Sludge.Shared;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorLevelDeserializer : MonoBehaviour
{
    [MenuItem("Tools/Sludge/Load Level")]
    private static void Load()
    {
        var levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        var levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();

        string filePath = EditorUtility.OpenFilePanel("Load level", "Assets/Resources/Levels", "json");
        string json = File.ReadAllText(filePath);
        var levelData = JsonConvert.DeserializeObject<LevelData>(json);
        LevelDeserializer.Run(levelData, levelElements, levelSettings);
    }
}
