using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditorLevelSerializer : MonoBehaviour
{
    [MenuItem("Tools/Sludge/SerializeLevel")]
    private static void SerializeLevel()
    {
        var levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        var levelSettings = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();

        var level = LevelSerializer.Run(levelElements, levelSettings);
        string json = JsonUtility.ToJson(level);
        File.WriteAllText("Assets/Resources/Levels/1.txt", json, Encoding.UTF8);
    }
}
