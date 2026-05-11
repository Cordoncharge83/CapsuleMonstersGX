using UnityEngine;
using UnityEngine.Tilemaps;

public class Capsule : MonoBehaviour
{
    [SerializeField] private Unit containedUnitPrefab;

    private Vector3Int currentCellPosition;
    private Tilemap combatTilemap;

    public void Initialize(Unit unitPrefab, Tilemap tilemap, Vector3Int cellPosition)
    {
        containedUnitPrefab = unitPrefab;
        combatTilemap = tilemap;
        currentCellPosition = cellPosition;

        transform.position = combatTilemap.GetCellCenterWorld(cellPosition);
    }

    public Vector3Int GetCurrentCellPosition()
    {
        return currentCellPosition;
    }

    public Unit GetContainedUnitPrefab()
    {
        return containedUnitPrefab;
    }

    public int GetSummonCost()
    {
        return containedUnitPrefab.GetActionAPCost();
    }
}