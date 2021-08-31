using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Portal : SludgeObject
{
    public string MatchId = "Portal1";
    public override EntityType EntityType => EntityType.Portal;

    bool receivedPlayer;

    Portal sibling;

    public override void Reset()
    {
        FindSibling();
    }

    private void FindSibling()
    {
        var portals = FindObjectsOfType<Portal>();
        for (int i = 0; i < portals.Length; ++i)
        {
            var other = portals[i];
            if (other != this && other.MatchId == MatchId)
            {
                sibling = other;
                break;
            }
        }
        Debug.LogError($"No sibling found for portal {this.gameObject.name}");
    }

    public void ReceivePlayer()
    {
        receivedPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        receivedPlayer = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (receivedPlayer)
            return;

        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        GameManager.Instance.Player.Teleport(sibling.transform.position);
        sibling.ReceivePlayer();
    }
}
