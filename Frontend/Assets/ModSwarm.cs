using Sludge.Easing;
using Sludge.Modifiers;
using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ModSwarm : SludgeModifier
{
    public GameObject Prototype;
    public int Count = 10;

    public double Width = 5;
    public double Height = 5;
    public double TimeMultiplierPositionX = 1;
    public double TimeOffsetPositionX = 0.1;
    public Easings EasingPositionX = Easings.Linear;
    public bool PingPongX = true;

    public double TimeMultiplierPositionY = 1;
    public double TimeOffsetPositionY = 0.1;
    public Easings EasingPositionY = Easings.Linear;
    public bool PingPongY = true;

    public double TimeMultiplierRotation = 1;
    public double TimeOffsetRotation;
    public Easings EasingRotation = Easings.Linear;

    List<Transform> members = new List<Transform>();
    Transform trans;

    public override void Reset()
    {
        trans = transform;
        UpdateMembers(t: 0);
    }

    public override void OnLoaded()
    {
        CreateMembers();
    }

    void CreateMembers()
    {
        if (members.Count > 0)
            return;

        for (int i = 0; i < Count; ++i)
        {
            var member = GameObject.Instantiate(Prototype, Vector3.zero, Quaternion.identity, transform);
            members.Add(member.transform);
        }
    }

    void UpdateMembers(double t)
    {
        for (int i = 0; i < Count; ++i)
        {
            double tx = SludgeUtil.TimeMod(t * TimeMultiplierPositionX + TimeOffsetPositionX * i);
            if (PingPongX) tx = Ease.PingPong(tx);
            double x = SludgeUtil.Stabilize(Ease.Apply(EasingPositionX, tx) * Width);

            double ty = SludgeUtil.TimeMod(t * TimeMultiplierPositionY + TimeOffsetPositionY * i);
            if (PingPongY) ty = Ease.PingPong(ty);
            double y = SludgeUtil.Stabilize(Ease.Apply(EasingPositionY, ty) * Height);
            var pos = new Vector2((float)x, (float)y);

            double trot = SludgeUtil.TimeMod(t * TimeMultiplierRotation + TimeOffsetRotation * i);
            double rotZ = SludgeUtil.Stabilize(Ease.Apply(EasingRotation, trot));
            var rotation = Quaternion.Euler(0, 0, (float)rotZ * 360);

            pos += (Vector2)trans.position; // Add parent world position
            members[i].transform.SetPositionAndRotation(pos, rotation);
        }
    }

    public override void EngineTick()
    {
        UpdateMembers(GameManager.Instance.EngineTime);
    }
}
