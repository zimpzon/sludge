using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModPortal : SludgeModifier
{
    public string MatchId;

    bool receivedPlayer;
    ModPortal sibling;

    // Start is called before the first frame update
    void Start()
    {
        FindSibling();
    }

    private void FindSibling()
    {
        var portals = FindObjectsOfType<ModPortal>();
        for (int i = 0; i < portals.Length; ++i)
        {
            var other = portals[i];
            if (other != this && other.MatchId == MatchId)
            {
                sibling = other;
                break;
            }
        }
        Debug.LogWarning($"No sibling found for portal {this.gameObject.name} (probably due to not having another frame started after SetActive)");
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

        SoundManager.Play(FxList.Instance.PortalEnter);
        GameManager.I.Player.Teleport(sibling.transform.position);
        sibling.ReceivePlayer();
    }

    public override void EngineTick()
    {
    }
}
