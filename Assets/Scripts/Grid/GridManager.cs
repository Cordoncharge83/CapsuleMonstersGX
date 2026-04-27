using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{

    private enum GamePhase
    {
        Placement,
        Battle
    }
    private enum ActionMode
    {
        None,
        Move,
        Attack,
        Fuse
    }

    [SerializeField] private UnitInfoUI unitInfoUI;
    [SerializeField] private UnitInfoUI enemyUnitInfoUI;
    [SerializeField] private ActionUI actionUI;
    [SerializeField] private TurnIndicatorUI turnIndicatorUI;

    [SerializeField] private GamePhase currentPhase = GamePhase.Placement;

    [SerializeField] private int playerPlacementMaxY = 0;

    [SerializeField] private CapsuleManager capsuleManager;

    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase moveHighlightTile;
    [SerializeField] private TileBase attackHighlightTile;
    [SerializeField] private TileBase fusionHighlightTile;

    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private List<Unit> enemyUnits;

    [SerializeField] private TurnManager turnManager;

    [SerializeField] private FusionManager fusionManager;

    private Unit selectedUnit;

    private Camera mainCamera;
    private bool isPlayerSelected = false;
    private ActionMode currentActionMode = ActionMode.None;

    private void Awake()
    {
        mainCamera = Camera.main;
        turnIndicatorUI.ShowPlacementPhase();
    }

    private void Update()
    {
        if (!turnManager.IsPlayerTurn())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleCancelInput();
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

        if (currentPhase == GamePhase.Placement)
        {
            HandlePlacement(cellPosition);
            return;
        }

        if (!combatTilemap.HasTile(cellPosition))
        {
            return;
        }

        Debug.Log($"Clicked cell: {cellPosition}");

        Unit clickedPlayer = GetPlayerUnitAtCell(cellPosition);
        Unit clickedEnemy = GetEnemyUnitAtCell(cellPosition);

        if (selectedUnit == null)
        {
            if (clickedPlayer != null)
            {
                SelectUnit(clickedPlayer);
                return;
            }

            if (clickedEnemy != null)
            {
                enemyUnitInfoUI.Show(clickedEnemy);
                return;
            }

            return;
        }

        if (clickedEnemy != null && currentActionMode == ActionMode.None)
        {
            enemyUnitInfoUI.Show(clickedEnemy);
            return;
        }

        if (currentActionMode == ActionMode.Fuse)
        {
            if (clickedPlayer != null && clickedPlayer != selectedUnit)
            {
                Unit fusedUnit = fusionManager.TryFusion(selectedUnit, clickedPlayer);

                if (fusedUnit != null)
                {
                    DeselectPlayer();
                    turnManager.EndPlayerTurn();
                }
            }

            return;
        }

        if (currentActionMode == ActionMode.Attack)
        {
            if (clickedEnemy != null)
            {
                TryAttackEnemy(clickedEnemy);
                DeselectPlayer();
            }

            return;
        }

        if (currentActionMode == ActionMode.Move)
        {
            if (!selectedUnit.CanMoveTo(cellPosition) || IsCellOccupied(cellPosition))
            {
                Debug.Log("Invalid move.");
                return;
            }

            selectedUnit.MoveTo(cellPosition);
            DeselectPlayer();
            turnManager.EndPlayerTurn();
            return;
        }

        if (clickedPlayer != null)
        {
            SelectUnit(clickedPlayer);
            return;
        }
    }

    private Unit GetPlayerUnitAtCell(Vector3Int cellPosition)
    {
        foreach (Unit unit in playerUnits)
        {
            if (unit != null && unit.GetCurrentCellPosition() == cellPosition)
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

        int damage = selectedUnit.CalculateDamageAgainst(targetEnemy);
        targetEnemy.TakeDamage(damage);
        turnManager.EndPlayerTurn();
    }

    private void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        currentActionMode = ActionMode.None;

        highlightTilemap.ClearAllTiles();

        unitInfoUI.Show(selectedUnit);
        actionUI.Show(
            HasAttackTarget(selectedUnit),
            HasFusionTarget(selectedUnit),
            selectedUnit.transform.position
        );

        Debug.Log("Player selected.");
    }

    private void DeselectPlayer()
    {
        selectedUnit = null;
        currentActionMode = ActionMode.None;
        highlightTilemap.ClearAllTiles();
        unitInfoUI.Hide();
        enemyUnitInfoUI.Hide();
        actionUI.Hide();
    }

    public void AddPlayerUnit(Unit unit)
    {
        playerUnits.Add(unit);
    }

    public void RemovePlayerUnit(Unit unit)
    {
        playerUnits.Remove(unit);
    }

    public void AddEnemyUnit(Unit unit)
    {
        enemyUnits.Add(unit);
    }

    public bool IsCellOccupied(Vector3Int cellPosition)
    {
        foreach (Unit unit in playerUnits)
        {
            if (unit != null && unit.GetCurrentCellPosition() == cellPosition)
            {
                return true;
            }
        }

        foreach (Unit unit in enemyUnits)
        {
            if (unit != null && unit.GetCurrentCellPosition() == cellPosition)
            {
                return true;
            }
        }

        return false;
    }

    private void HandlePlacement(Vector3Int cellPosition)
    {
        if (!combatTilemap.HasTile(cellPosition))
        {
            return;
        }

        if (cellPosition.y > playerPlacementMaxY)
        {
            Debug.Log("Cannot place unit outside player placement zone.");
            return;
        }

        if (IsCellOccupied(cellPosition))
        {
            Debug.Log("Tile occupied.");
            return;
        }

        if (!capsuleManager.HasCapsulesLeft())
        {
            Debug.Log("All units placed. Starting battle.");
            currentPhase = GamePhase.Battle;
            turnIndicatorUI.ShowPlayerTurn();
            return;
        }

        capsuleManager.PlaceNextCapsule(cellPosition);

        if (!capsuleManager.HasCapsulesLeft())
        {
            Debug.Log("All units placed. Starting battle.");
            currentPhase = GamePhase.Battle;
            turnIndicatorUI.ShowPlayerTurn();
        }
    }

    private void ShowFusionTargets()
    {
        highlightTilemap.ClearAllTiles();

        foreach (Unit unit in playerUnits)
        {
            if (unit == null || unit == selectedUnit)
            {
                continue;
            }

            if (fusionManager.CanFuse(selectedUnit, unit))
            {
                Vector3Int unitCell = unit.GetCurrentCellPosition();
                highlightTilemap.SetTile(unitCell, fusionHighlightTile);
            }
        }
    }

    public void EnterMoveMode()
    {
        if (selectedUnit == null) return;

        currentActionMode = ActionMode.Move;
        highlightTilemap.ClearAllTiles();
        ShowMovementRange();
    }

    public void EnterAttackMode()
    {
        if (selectedUnit == null) return;

        currentActionMode = ActionMode.Attack;
        highlightTilemap.ClearAllTiles();
        ShowAttackRange();
    }

    public void EnterFuseMode()
    {
        if (selectedUnit == null) return;

        currentActionMode = ActionMode.Fuse;
        highlightTilemap.ClearAllTiles();
        ShowFusionTargets();
    }

    public void CancelAction()
    {
        DeselectPlayer();
    }

    public bool HasAttackTarget(Unit unit)
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy != null && unit.IsInAttackRange(enemy))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasFusionTarget(Unit unit)
    {
        foreach (Unit ally in playerUnits)
        {
            if (ally == null || ally == unit)
            {
                continue;
            }

            if (fusionManager.CanFuse(unit, ally))
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCancelInput()
    {
        if (selectedUnit == null)
        {
            enemyUnitInfoUI.Hide();
            return;
        }


        if (currentActionMode != ActionMode.None)
        {
            currentActionMode = ActionMode.None;
            highlightTilemap.ClearAllTiles();

            actionUI.Show(
                HasAttackTarget(selectedUnit),
                HasFusionTarget(selectedUnit),
                selectedUnit.transform.position
            );

            return;
        }

        DeselectPlayer();
    }
}