using UnityEngine;

namespace Sludge.Tiles
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AntAnim", order = 4)]
    public class AntAnimScriptableObject : ScriptableObject
    {
        public Sprite[] Sprites;
    }
}