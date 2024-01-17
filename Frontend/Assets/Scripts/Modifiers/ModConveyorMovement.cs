using Assets.Scripts.Levels;
using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModConveyorMovement : SludgeModifier, ICustomSerialized
    {
        public bool DisableSuction = false;

        // it is important to keep these values consistent between levels so not public
        float SuctionPower = 30.0f;
        float ConveyorSpeed = 25.0f;

        SpriteRenderer spriteRenderer;
        Vector2 beltDirection;
        Vector2 centerLineA;
        Vector2 centerLineB;
        Transform trans;
        List<GameObject> passengerList = new ();

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
            var passenger = collision.gameObject.GetComponent<IConveyorBeltPassenger>();
            if (passenger == null)
                return;

            passenger.OnConveyorBeltEnter(beltDirection);
            passengerList.Add(collision.gameObject);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var passenger = collision.gameObject.GetComponent<IConveyorBeltPassenger>();
            if (passenger == null)
                return;

            passenger.OnConveyorBeltExit(beltDirection);
            passengerList.Remove(collision.gameObject);
        }

        public override void EngineTick()
        {
            // Pull passengers towards center line
            for (int i = 0; i < passengerList.Count; ++i)
            {
                var passengerGo = passengerList[i];
                var passenger = passengerGo.GetComponent<IConveyorBeltPassenger>();
                if (!DisableSuction)
                {
                    var closestPointOnCenterLine = SludgeUtil.StabilizeVector(SludgeUtil.GetClosestPointOnInfiniteLine(passengerGo.transform.position, centerLineA, centerLineB));
                    var directionToCenter = closestPointOnCenterLine - passengerGo.transform.position;
                    passengerGo.GetComponent<IConveyorBeltPassenger>().AddConveyorPulse(directionToCenter * SuctionPower);
                }

                // Move along the belt
                passenger.AddConveyorPulse(beltDirection * ConveyorSpeed);
            }
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
