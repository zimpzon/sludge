using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class EnergyPillDetectorScript : MonoBehaviour
{
    public float radius = 0.5f;
    float armMinDistance = 1.5f;
    public bool CanArmUnarmed = false;

    public UnityEvent OnArmedHit;

    public TileBase UnarmedTile;
    public TileBase PendingTile;
    public TileBase ArmedTile;

    private List<Vector3Int> _pendingArming = new List<Vector3Int>();

    Transform trans;

    private void Awake()
    {
        trans = transform;
    }

    public void Reset()
    {
        // only necessary to reset if can arm unarmed (currently player only)
        _pendingArming.Clear();
    }

    private void ArmPending()
    {
        for (int i = _pendingArming.Count - 1; i >= 0; i--)
        {
            Vector3Int tilePos = _pendingArming[i];
            bool readyToArm = Vector3.Distance(GameManager.I.EnergyTilemap.CellToWorld(tilePos), trans.position) >= armMinDistance;
            if (readyToArm)
            {
                SoundManager.Play(FxList.Instance.ArmEnergy);
                GameManager.I.EnergyTilemap.SetTile(tilePos, ArmedTile);
                ParticleEmitter.I.EmitEnergyArm(GameManager.I.EnergyTilemap.CellToWorld(tilePos) + GameManager.I.EnergyTilemap.tileAnchor, 11);
                _pendingArming.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        Vector3Int centerTile = GameManager.I.EnergyTilemap.WorldToCell(trans.position);

        int radiusInTiles = Mathf.CeilToInt(radius / GameManager.I.EnergyTilemap.cellSize.x);
        for (int x = -radiusInTiles; x <= radiusInTiles; x++)
        {
            for (int y = -radiusInTiles; y <= radiusInTiles; y++)
            {
                Vector3Int tilePos = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);

                if (Vector3.Distance(GameManager.I.EnergyTilemap.CellToWorld(tilePos), trans.position) <= radius)
                {
                    TileBase tile = GameManager.I.EnergyTilemap.GetTile<TileBase>(tilePos);
                    if (tile == null)
                        continue;

                    if (tile == UnarmedTile && CanArmUnarmed)
                    {
                        if (!_pendingArming.Contains(tilePos))
                        {
                            SoundManager.Play(FxList.Instance.TriggerEnergy);
                            _pendingArming.Add(tilePos);
                            GameManager.I.EnergyTilemap.SetTile(tilePos, PendingTile);
                        }
                    }
                    else if (tile == ArmedTile)
                    {
                        OnArmedHit?.Invoke();
                    }
                }
            }
        }

        if (CanArmUnarmed)
            ArmPending();
    }
}
