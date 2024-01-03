using Assets.Scripts.Levels;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModConveyorMovement : SludgeModifier, ICustomSerialized
    {
        const float SuctionPower = 0.5f;
        const float ConveyorSpeed = 14;

        public int Length = 1;

        SpriteRenderer spriteRenderer;
        Vector2 beltDirection;
        Vector2 centerLineA;
        Vector2 centerLineB;
        Transform trans;
        bool hasPlayer;

        void Awake()
        {
            trans = transform;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnValidate()
        {
            SetSize();
        }

        void SetSize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnLoaded()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetSize();
        }

        public override void Reset()
        {
            SetSize();
        }

        private void Start()
        {
            SetSize();
            centerLineA = SludgeUtil.StabilizeVector(trans.TransformPoint(Vector2.left * 0.5f));
            centerLineB = SludgeUtil.StabilizeVector(trans.TransformPoint(Vector2.right * 0.5f));
            beltDirection = (centerLineB - centerLineA).normalized;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var entity = SludgeUtil.GetEntityType(collision.gameObject);
            if (entity != EntityType.Player)
                return;

            OnPlayerEnter();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var entity = SludgeUtil.GetEntityType(collision.gameObject);
            if (entity != EntityType.Player)
                return;

            OnPlayerExit();
        }

        public void OnPlayerEnter()
        {
            GameManager.I.Player.ConveyourBeltEnter();
            hasPlayer = true;
        }

        public void OnPlayerExit()
        {
            GameManager.I.Player.ConveyourBeltExit();
            hasPlayer = false;
        }

        public override void EngineTick()
        {
            if (!hasPlayer)
                return;

            // Pull player towards center line
            var closestPointOnCenterLine = SludgeUtil.StabilizeVector(SludgeUtil.GetClosestPointOnInfiniteLine(Player.Position, centerLineA, centerLineB));
            var directionToCenter = closestPointOnCenterLine - Player.Position;
            GameManager.I.Player.AddPositionImpulse(directionToCenter.x * SuctionPower, directionToCenter.y * SuctionPower);

            // Move along the belt
            GameManager.I.Player.AddPositionImpulse(beltDirection.x * ConveyorSpeed, beltDirection.y * ConveyorSpeed);
        }

        public string SerializeCustomData()
        {
            return JsonUtility.ToJson(spriteRenderer.size);
        }

        public void DeserializeCustomData(string customData)
        {
            if (string.IsNullOrWhiteSpace(customData))
                return;

            spriteRenderer.size = JsonUtility.FromJson<Vector2>(customData);
        }
    }
}
