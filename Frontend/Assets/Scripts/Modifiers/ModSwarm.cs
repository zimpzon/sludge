using Sludge.Easing;
using Sludge.Modifiers;
using Sludge.SludgeObjects;
using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ModSwarm : SludgeModifier
{
    public int Count = 10;

    public double Width = 5;
    public double Height = 5;

    public double TimeXMul = 1.0;
    public double TimeXAdd = 0.0;
    public double TimeXAddPerItem = 0.1;
    public Easings EasingPositionX = Easings.Linear;
    public bool PingPongX = true;

    public double TimeYMul = 1.0;
    public double TimeYAdd = 0.0;
    public double TimeYAddPerItem = 0.1;
    public Easings EasingPositionY = Easings.Linear;
    public bool PingPongY = true;

    public double TimeRotMul = 1.0;
    public double TimeRotAdd = 0.0;
    public double TimeRotAddPerItem = 0.1;
    public Easings EasingRotation = Easings.Linear;

    public double GizmoT = 0.0;

    GameObject Prototype;
    List<Transform> members = new List<Transform>();
    List<SludgeObject> memberSludgeObjectComponents = new List<SludgeObject>();
    Transform trans;

    public override void Reset()
    {
        trans = transform;
        if (Prototype == null)
            Prototype = trans.Find("SwarmElement").gameObject;

        CreateMembers();
        UpdateMembers(t: 0);
    }

    void CreateMembers()
    {
        if (members.Count > 0)
            return;

        for (int i = 0; i < Count; ++i)
        {
            var member = GameObject.Instantiate(Prototype, Vector3.zero, Quaternion.identity, transform);
            memberSludgeObjectComponents.Add(member.GetComponent<SludgeObject>());

            member.SetActive(true);
            members.Add(member.transform);
        }

        Prototype.SetActive(false);
    }

    void UpdateMembers(double t)
    {
        for (int i = 0; i < Count; ++i)
        {
            double tx = SludgeUtil.TimeMod(t * TimeXMul + TimeXAddPerItem * i + TimeXAdd);
            if (PingPongX) tx = Ease.PingPong(tx);
            double x = SludgeUtil.Stabilize(Ease.Apply(EasingPositionX, tx) * Width);

            double ty = SludgeUtil.TimeMod(t * TimeYMul + TimeYAddPerItem * i + TimeYAdd);
            if (PingPongY) ty = Ease.PingPong(ty);
            double y = SludgeUtil.Stabilize(Ease.Apply(EasingPositionY, ty) * Height);
            var pos = new Vector2((float)x, (float)y);

            double tRot = SludgeUtil.TimeMod(t * TimeRotMul + TimeRotAddPerItem * i + TimeRotAdd);
            double rotZ = SludgeUtil.Stabilize(Ease.Apply(EasingRotation, tRot));
            var rotation = Quaternion.Euler(0, 0, (float)rotZ * 360);

            pos += (Vector2)trans.position; // Add parent world position
            members[i].transform.SetPositionAndRotation(pos, rotation);

            memberSludgeObjectComponents[i]?.EngineTick();
        }
    }

    private void OnDrawGizmos()
    {
        double t = GizmoT;

        for (int i = 0; i < Count; ++i)
        {
            double tx = SludgeUtil.TimeMod(t * TimeXMul + TimeXAddPerItem * i + TimeXAdd);
            if (PingPongX) tx = Ease.PingPong(tx);
            double x = SludgeUtil.Stabilize(Ease.Apply(EasingPositionX, tx) * Width);

            double ty = SludgeUtil.TimeMod(t * TimeYMul + TimeYAddPerItem * i + TimeYAdd);
            if (PingPongY) ty = Ease.PingPong(ty);
            double y = SludgeUtil.Stabilize(Ease.Apply(EasingPositionY, ty) * Height);
            var pos = new Vector2((float)x, (float)y);

            double tRot = SludgeUtil.TimeMod(t * TimeRotMul + TimeRotAddPerItem * i + TimeRotAdd);
            double rotZ = SludgeUtil.Stabilize(Ease.Apply(EasingRotation, tRot));
            var rotation = Quaternion.Euler(0, 0, (float)rotZ * 360);

            pos += (Vector2)transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(pos, Vector3.one * 0.5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, (Vector3)pos + rotation * Vector2.up * 0.5f);
        }
    }

    public override void EngineTick()
    {
        UpdateMembers(GameManager.Instance.EngineTime);
    }
}
