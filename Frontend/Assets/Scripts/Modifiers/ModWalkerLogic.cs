using Sludge.Modifiers;
using UnityEngine;

public class ModWalkerLogic : SludgeModifier
{
    class S
    {
        public float forceY;
        public Vector2 impulse;
        public bool Alive = true;
    }

    public Transform TargetTransform;
    public float fallGravity = 4.0f;
    public float maxVelocity = 15.0f;
    public float EyeScaleSurprised = 1.5f;

    public float nudgeForce = 1f;     // Adjust this value to control the nudge strength
    public float attemptInterval = 1f; // Time interval in seconds between attempts to upright itself

    S s = new S();
    Transform trans;
    Vector3 basePos;
    Quaternion baseRotation;

    private Rigidbody2D _rigidbody;

    public override void OnLoaded()
    {
        trans = transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        basePos = transform.position;
        baseRotation = transform.rotation;
    }

    public override void Reset()
    {
        s = new S();
        gameObject.SetActive(true);
        transform.position = basePos;
        transform.rotation = baseRotation;

        base.Reset();
    }

    public override void EngineTick()
    {
        if (!s.Alive)
            return;

        DebugLinesScript.Show("a", Time.time);
        _rigidbody.AddForce(Vector2.down * (float)GameManager.TickSize, ForceMode2D.Impulse);
        s.impulse = Vector2.zero;
    }
}
