using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Unit playerUnit;
    [SerializeField] private Unit enemyUnit;
    [SerializeField] private TurnManager turnManager;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!turnManager.IsPlayerTurn())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DetectClickedCell();
        }
    }

    private void DetectClickedCell()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = combatTilemap.WorldToCell(mouseWorldPosition);

        if (!combatTilemap.HasTile(cellPosition))
        {
            return;
        }

        Debug.Log($"Clicked cell: {cellPosition}");

        if (enemyUnit != null && cellPosition == enemyUnit.GetCurrentCellPosition())
        {
            TryAttackEnemy();
            return;
        }

        if (!playerUnit.CanMoveTo(cellPosition))
        {
            Debug.Log("Target cell is outside movement range.");
            return;
        }

        playerUnit.MoveTo(cellPosition);
        turnManager.EndPlayerTurn();
    }

    private void TryAttackEnemy()
    {
        if (!playerUnit.IsAdjacentTo(enemyUnit))
        {
            Debug.Log("Enemy is not adjacent. Cannot attack.");
            return;
        }

        enemyUnit.TakeDamage(playerUnit.GetAttackPower());
        turnManager.EndPlayerTurn();
    }
}