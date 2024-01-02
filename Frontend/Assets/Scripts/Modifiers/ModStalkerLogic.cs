using Sludge.Modifiers;
using UnityEngine;

public class ModStalkerLogic : SludgeModifier
{
    public float ChaseForce = 1000.0f;

    Transform trans;
    Rigidbody2D rigidBody;
    AnimatedAnt ant;
    Vector3 basePos;
    Quaternion baseRot;

    private void Awake()
    {
        trans = transform;
        rigidBody = GetComponent<Rigidbody2D>();
        ant = GetComponentInChildren<AnimatedAnt>();
        ant.animationOffset = Mathf.Clamp01((float)(basePos.x * 0.117 + basePos.y * 0.3311));
        ant.animationSpeedScale = 1;
    }

    public override void OnLoaded()
    {
        trans = transform;
        basePos = trans.position;
        baseRot = trans.rotation;
    }

    public override void Reset()
    {
        ant.animationSpeedScale = 1;
        trans.position = basePos;
        trans.rotation = baseRot;
    }

    public override void EngineTick()
    {
        var playerDir = Player.Position - trans.position;
        trans.rotation = Quaternion.LookRotation(Vector3.forward, playerDir);

        float playerDistance = playerDir.magnitude;
        ant.animationSpeedScale = playerDistance > 5 ? 1 : 2;

        rigidBody.AddForce(playerDir.normalized * ChaseForce * (float)GameManager.TickSize);
    }
}
