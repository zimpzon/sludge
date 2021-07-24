using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditorLevelDeserializer : MonoBehaviour
{
    [MenuItem("Tools/Sludge/Load Level")]
    private static void Load()
    {
        var levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        var levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();

        var level = LevelSerializer.Run(levelElements, levelSettings);
        string json = JsonUtility.ToJson(level);
        File.WriteAllText("Assets/Resources/Levels/1.txt", json, Encoding.UTF8);
    }
}
