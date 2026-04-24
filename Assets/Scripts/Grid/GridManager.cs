using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase moveHighlightTile;
    [SerializeField] private TileBase attackHighlightTile;

    [SerializeField] private Unit playerUnit;
    [SerializeField] private Unit enemyUnit;
    [SerializeField] private TurnManager turnManager;

    private Camera mainCamera;
    private bool isPlayerSelected = false;

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

        if (!isPlayerSelected)
        {
            if (IsPlayerCell(cellPosition))
            {
                isPlayerSelected = true;
                ShowMovementRange();
                ShowAttackRange();
                Debug.Log("Player selected.");
            }

            return;
        }

        if (enemyUnit != null && cellPosition == enemyUnit.GetCurrentCellPosition())
        {
            TryAttackEnemy();
            isPlayerSelected = false;
            highlightTilemap.ClearAllTiles();
            return;
        }

        if (!playerUnit.CanMoveTo(cellPosition))
        {
            Debug.Log("Target cell is outside movement range.");
            return;
        }

        playerUnit.MoveTo(cellPosition);
        isPlayerSelected = false;
        highlightTilemap.ClearAllTiles();
        turnManager.EndPlayerTurn();
    }

    private bool IsPlayerCell(Vector3Int cellPosition)
    {
        return cellPosition == playerUnit.GetCurrentCellPosition();
    }

    private void ShowMovementRange()
    {
        highlightTilemap.ClearAllTiles();

        Vector3Int currentPosition = playerUnit.GetCurrentCellPosition();
        int range = playerUnit.GetMoveRange();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance <= range)
                {
                    Vector3Int targetCell = currentPosition + new Vector3Int(x, y, 0);

                    if (combatTilemap.HasTile(targetCell))
                    {
                        highlightTilemap.SetTile(targetCell, moveHighlightTile);
                    }
                }
            }
        }
        Debug.Log("Showing movement range");
    }

    private void ShowAttackRange()
    {
        Vector3Int currentPosition = playerUnit.GetCurrentCellPosition();
        int range = playerUnit.GetAttackRange();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance <= range)
                {
                    Vector3Int targetCell = currentPosition + new Vector3Int(x, y, 0);

                    if (combatTilemap.HasTile(targetCell))
                    {
                        highlightTilemap.SetTile(targetCell, attackHighlightTile);
                    }
                }
            }
        }
    }

    private void TryAttackEnemy()
    {
        if (!playerUnit.IsInAttackRange(enemyUnit))
        {
            Debug.Log("Enemy is out of range. Cannot attack.");
            return;
        }

        enemyUnit.TakeDamage(playerUnit.GetAttackPower());
        turnManager.EndPlayerTurn();
    }
}