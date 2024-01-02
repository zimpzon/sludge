using Assets.Scripts.Game;
using Sludge.Modifiers;
using Sludge.Utility;
using TMPro;
using UnityEngine;

public class ModPickupSequence : SludgeModifier
{
    public int SequenceValue = 0;

    TextMeshPro label;

    private void OnValidate()
    {
        label = SludgeUtil.FindByName(transform, "Label").GetComponent<TextMeshPro>();
        SetLabel();
    }

    void SetLabel()
    {
        label.text = ((char)('A' + SequenceValue)).ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        PickupSequenceManager.OnPickup(this);
    }

    public void SetPickupActive(bool activate)
    {

    }

    public override void EngineTick()
    {
    }
}
