using Sludge.Modifiers;

public class ModCellFollower : SludgeModifier
{
    public override void EngineTick()
    {
        byte myCell = LevelCells.Instance.GetCellValue(transform.position);
        DebugLinesScript.Show("myCell", myCell);
    }
}
