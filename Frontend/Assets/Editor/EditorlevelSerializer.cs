using Newtonsoft.Json;
using Sludge.Shared;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorLevelSerializer : MonoBehaviour
{
    static string LevelFolder = "Assets/Resources/Levels";

    public static void SaveLevel(LevelData levelData, bool refreshAssetDb = true)
    {
        string fileName = "(select name)";
        foreach (var c in Path.GetInvalidFileNameChars()) { fileName = fileName.Replace(c, '-'); }
        string path = Path.Combine(LevelFolder, fileName + ".json");
        string json = JsonConvert.SerializeObject(levelData);
        Save(path, json, refreshAssetDb);
    }

    static void Save(string filePath, string json, bool refreshAssetDb = true)
    {
        File.WriteAllText(filePath, json);
        File.Delete(filePath + ".meta"); // I couldn't get Unity to recognize changes in text files unless deleting this.
        if (refreshAssetDb)
            AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Sludge/Save Level")]
    private static void Save()
    {
        var levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        var levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();

        LevelData level = LevelSerializer.Run(levelElements, levelSettings);
        string json = JsonConvert.SerializeObject(level);

        string fileName = level.FileNameFromNamespaceAndId();

        string filePath = EditorUtility.SaveFilePanel("Save level", LevelFolder, fileName, "json");
        if (string.IsNullOrEmpty(filePath))
            return;

        Save(filePath, json);
    }
}
