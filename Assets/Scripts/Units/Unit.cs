using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Vector3Int currentCellPosition;

    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;
    [SerializeField] private int attackPower = 3;
    [SerializeField] private int moveRange = 3;

    private void Start()
    {
        currentHp = maxHp;
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

    public bool CanMoveTo(Vector3Int targetCellPosition)
    {
        int distanceX = Mathf.Abs(currentCellPosition.x - targetCellPosition.x);
        int distanceY = Mathf.Abs(currentCellPosition.y - targetCellPosition.y);

        int totalDistance = distanceX + distanceY;

        return totalDistance <= moveRange;
    }

    public Vector3Int GetCurrentCellPosition()
    {
        return currentCellPosition;
    }

    public int GetAttackPower()
    {
        return attackPower;
    }

    public bool IsAdjacentTo(Unit otherUnit)
    {
        Vector3Int otherCell = otherUnit.GetCurrentCellPosition();

        int distanceX = Mathf.Abs(currentCellPosition.x - otherCell.x);
        int distanceY = Mathf.Abs(currentCellPosition.y - otherCell.y);

        return distanceX + distanceY == 1;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} was defeated.");
        Destroy(gameObject);
    }
}