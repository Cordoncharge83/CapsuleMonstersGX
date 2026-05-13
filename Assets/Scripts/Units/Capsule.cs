using UnityEngine;
using UnityEngine.Tilemaps;

public class Capsule : MonoBehaviour
{
    [SerializeField] private Unit containedUnitPrefab;
    [SerializeField] private Unit.Team ownerTeam;

    public Unit.Team OwnerTeam => ownerTeam;

    private Vector3Int currentCellPosition;
    private Tilemap combatTilemap;

    public void Initialize(Unit unitPrefab, Tilemap tilemap, Vector3Int cellPosition, Unit.Team team)
    {
        containedUnitPrefab = unitPrefab;
        combatTilemap = tilemap;
        currentCellPosition = cellPosition;
        ownerTeam = team;

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