using Sludge.Shared;
using TMPro;
using UnityEngine;

public class UiLevel : MonoBehaviour
{
    public TextMeshProUGUI TextLevelId;
    public LevelData levelData;

    public void SetData(LevelData levelData)
    {
        TextLevelId.text = levelData.Id;
    }
}
