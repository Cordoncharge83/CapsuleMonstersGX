using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Vector3Int currentCellPosition;

    private void Start()
    {
        SnapToCell(currentCellPosition);
    }

    public void SnapToCell(Vector3Int cellPosition)
    {
        currentCellPosition = cellPosition;

        Vector3 worldPosition = combatTilemap.GetCellCenterWorld(cellPosition);
        transform.position = worldPosition;
    }
    public void MoveTo(Vector3Int targetCellPosition)
    {
        SnapToCell(targetCellPosition);
    }
}