using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

public class PillCollectorScript : MonoBehaviour
{
    List<ContactPoint2D> _contacts = new List<ContactPoint2D>();

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            int hits = collision.GetContacts(_contacts);
            for (int i = 0; i < hits; ++i)
            {
                PillManager.EatPill(_contacts[i].point, PillEater.Player);
            }
        }
    }
}
