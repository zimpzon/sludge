using UnityEngine;

interface IConveyorBeltPassenger
{
    void OnConveyorBeltEnter(Vector2 beltDirection);
    void OnConveyorBeltExit(Vector2 beltDirection);
    void AddConveyorPulse(Vector2 pulse);
}
