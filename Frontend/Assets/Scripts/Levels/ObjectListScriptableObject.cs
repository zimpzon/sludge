using UnityEditor;
using UnityEngine;

namespace Sludge.Tiles
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ObjectList", order = 3)]
    public class ObjectListScriptableObject : ScriptableObject
    {
        public GameObject[] ObjectPrefabs;

#if UNITY_EDITOR
        public int GetObjectIndex(GameObject obj)
        {
            // This can only build in Editor.
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            for (int i = 0; i < ObjectPrefabs.Length; ++i)
            {
                if (ObjectPrefabs[i] == prefab)
                    return i;
            }

            Debug.LogError($"Object not found in ObjectListScriptableObject.ObjectPrefabs: {obj.name}");
            return 0;
        }
#endif
    }
}
