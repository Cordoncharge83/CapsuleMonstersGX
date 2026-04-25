using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase moveHighlightTile;
    [SerializeField] private TileBase attackHighlightTile;

    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private List<Unit> enemyUnits;

    [SerializeField] private TurnManager turnManager;

    private Unit selectedUnit;

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
            Unit clickedUnit = GetPlayerUnitAtCell(cellPosition);

            if (clickedUnit != null)
            {
                selectedUnit = clickedUnit;
                isPlayerSelected = true;

                ShowMovementRange();
                ShowAttackRange();

                Debug.Log("Player selected.");
            }

            return;
        }

        Unit clickedEnemy = GetEnemyUnitAtCell(cellPosition);

        if (clickedEnemy != null)
        {
            TryAttackEnemy(clickedEnemy);
            DeselectPlayer();
            return;
        }

        if (!selectedUnit.CanMoveTo(cellPosition))
        {
            Debug.Log("Target cell is outside movement range.");
            DeselectPlayer();
            return;
        }

        selectedUnit.MoveTo(cellPosition);
        DeselectPlayer();
        turnManager.EndPlayerTurn();
    }

    private Unit GetPlayerUnitAtCell(Vector3Int cellPosition)
    {
        foreach (Unit unit in playerUnits)
        {
            if (unit.GetCurrentCellPosition() == cellPosition)
            {
                return unit;
            }
        }

        return null;
    }

    private Unit GetEnemyUnitAtCell(Vector3Int cellPosition)
    {
        foreach (Unit unit in enemyUnits)
        {
            if (unit != null && unit.GetCurrentCellPosition() == cellPosition)
            {
                return unit;
            }
        }

        return null;
    }

    private void ShowMovementRange()
    {
        highlightTilemap.ClearAllTiles();

        Vector3Int currentPosition = selectedUnit.GetCurrentCellPosition();
        int range = selectedUnit.GetMoveRange();

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
        Vector3Int currentPosition = selectedUnit.GetCurrentCellPosition();
        int range = selectedUnit.GetAttackRange();

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

    private void TryAttackEnemy(Unit targetEnemy)
    {
        if (!selectedUnit.IsInAttackRange(targetEnemy))
        {
            Debug.Log("Enemy is outside attack range. Cannot attack.");
            return;
        }

        targetEnemy.TakeDamage(selectedUnit.GetAttackPower());
        turnManager.EndPlayerTurn();
    }

    private void DeselectPlayer()
    {
        isPlayerSelected = false;
        highlightTilemap.ClearAllTiles();
    }
}