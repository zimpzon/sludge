using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class EnergyPillDetectorScript : MonoBehaviour
{
    public float radius = 0.5f;
    public bool CanArmUnarmed = false;

    public UnityEvent OnArmedHit;

    public TileBase UnarmedTile;
    public TileBase ArmedTile;

    private List<Vector3Int> _pendingCurrenUpdate = new List<Vector3Int>();
    private List<Vector3Int> _pendingPrevUpdate = new List<Vector3Int>();

    private void ArmPending()
    {
        DebugLinesScript.Show("pending-now", _pendingCurrenUpdate.Count);
        DebugLinesScript.Show("pending-prev", _pendingPrevUpdate.Count);
        DebugLinesScript.Show("cap", _pendingCurrenUpdate.Capacity);

        // find all that were pending last frame but not this frame and arm them
        for (int i = _pendingPrevUpdate.Count - 1; i >= 0; i--)
        {
            bool readyToArm = !_pendingCurrenUpdate.Contains(_pendingPrevUpdate[i]);
            if (readyToArm)
            {
                GameManager.I.EnergyTilemap.SetTile(_pendingPrevUpdate[i], ArmedTile);
                ParticleEmitter.I.EmitEnergyArm(GameManager.I.EnergyTilemap.CellToWorld(_pendingPrevUpdate[i]) + GameManager.I.EnergyTilemap.tileAnchor, 11);
                _pendingPrevUpdate.RemoveAt(i);
            }
        }

        var temp = _pendingPrevUpdate;
        _pendingPrevUpdate = _pendingCurrenUpdate;
        _pendingCurrenUpdate = temp;
    }

    private void Update()
    {
        Vector3Int centerTile = GameManager.I.EnergyTilemap.WorldToCell(transform.position);

        _pendingCurrenUpdate.Clear();

        int radiusInTiles = Mathf.CeilToInt(radius / GameManager.I.EnergyTilemap.cellSize.x);
        for (int x = -radiusInTiles; x <= radiusInTiles; x++)
        {
            for (int y = -radiusInTiles; y <= radiusInTiles; y++)
            {
                Vector3Int tilePos = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);

                if (Vector3.Distance(GameManager.I.EnergyTilemap.CellToWorld(tilePos), transform.position) <= radius)
                {
                    TileBase tile = GameManager.I.EnergyTilemap.GetTile<TileBase>(tilePos);
                    if (tile == null)
                        continue;

                    if (tile == UnarmedTile && CanArmUnarmed)
                    {
                        _pendingCurrenUpdate.Add(tilePos);
                    }
                    else if (tile == ArmedTile)
                    {
                        OnArmedHit?.Invoke();
                    }
                    else
                    {
                        Debug.LogError("Unknown tile: " + tile.name);
                    }
                }
            }
        }

        if (CanArmUnarmed)
            ArmPending();
    }
}
