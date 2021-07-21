using TMPro;
using UnityEngine;

public class VirtualScrollSync : MonoBehaviour
{
    public TextMeshProUGUI Text;

    // Message callback from scrollview
    void ScrollCellIndex(int idx)
    {
        string name = "Cell " + idx.ToString();
        Text.text = name;
    }
}
