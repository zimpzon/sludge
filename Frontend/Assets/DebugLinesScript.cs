using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugLinesScript : MonoBehaviour
{
    public static DebugLinesScript Instance;

    public static void Show(object key, object value)
    {
        Instance.SetLine(key.ToString(), value);
    }

    Dictionary<string, string> lines_ = new Dictionary<string, string>();
    TextMeshProUGUI text_;

    public void Clear()
    {
        lines_.Clear();
        text_.text = "";
        text_.enabled = false;
    }

    public void RemoveLine(string key)
    {
        if (lines_.ContainsKey(key))
            lines_.Remove(key);

        if (lines_.Count == 0)
            text_.text = "";
    }

    public void SetLine(string key, object value)
    {
        lines_[key] = value?.ToString() ?? "<null>";
    }

    void Awake()
    {
        Instance = this;
        text_ = GetComponent<TextMeshProUGUI>();
        text_.enabled = false;
    }

    void Update()
    {
        if (lines_.Count == 0)
            return;

        StringBuilder sb = new StringBuilder();
        foreach(var pair in lines_)
        {
            sb.AppendLine($"{pair.Key}: {pair.Value}");
        }

        text_.text = sb.ToString();
        text_.enabled = true;
    }
}
