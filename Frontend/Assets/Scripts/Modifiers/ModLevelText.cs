using Sludge.Modifiers;
using TMPro;

public class ModLevelText : SludgeModifier
{
    public string Text;

    private void OnValidate() => SetText();
    public override void OnLoaded() => SetText();

    void SetText() => GetComponent<TextMeshPro>().text = Text;

    public override void EngineTick()
    {
    }
}
