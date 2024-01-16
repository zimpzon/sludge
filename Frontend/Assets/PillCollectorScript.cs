using Assets.Scripts.Game;
using UnityEngine;

public class PillCollectorScript : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("PillCollectorScript.OnCollisionStay2D");
        for (int i = 0; i < collision.contactCount; ++i)
        {
            var contact = collision.GetContact(i);
            PillManager.EatPill(contact.point, PillEater.Player);
        }
    }
}
