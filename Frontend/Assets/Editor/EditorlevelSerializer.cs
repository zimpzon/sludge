using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
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
        string filePath = EditorUtility.SaveFilePanel("Save level", "Assets/Resources/Levels", "mylevel", "json");
        File.WriteAllText(filePath, json, Encoding.UTF8);
    }
}
