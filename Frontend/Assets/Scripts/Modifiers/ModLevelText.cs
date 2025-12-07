using Assets.Scripts.Levels;
using Sludge.Modifiers;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ModLevelTextData
{
    public Vector3 Position;
    public float Width;
    public float Height;
    public string Text;
}

public class ModLevelText : SludgeModifier, ICustomSerialized
{
    private void OnValidate() { }

    public override void OnLoaded() { }

    public override void EngineTick()
    {
    }

    public string SerializeCustomData()
    {
        var textComponent = GetComponent<TextMeshPro>();
        var data = new ModLevelTextData
        {
            Position = transform.position,
            Width = 0,
            Height = 0,
            Text = textComponent != null ? textComponent.text : ""
        };

        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            data.Width = rectTransform.rect.width;
            data.Height = rectTransform.rect.height;
        }

        return JsonUtility.ToJson(data);
    }

    public void DeserializeCustomData(string customData)
    {
        if (string.IsNullOrWhiteSpace(customData))
            return;

        var data = JsonUtility.FromJson<ModLevelTextData>(customData);
        transform.position = data.Position;

        var textComponent = GetComponent<TextMeshPro>();
        if (textComponent != null)
            textComponent.text = data.Text;

        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, data.Width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, data.Height);
        }
    }
}
