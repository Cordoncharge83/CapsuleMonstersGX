using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private DamagePreviewUI damagePreviewUI;

    [SerializeField] private GamePhase currentPhase = GamePhase.Placement;

    [SerializeField] private int playerPlacementMaxY = 0;

    [SerializeField] private CapsuleManager capsuleManager;
    [SerializeField] private EnemyAI enemyAI;

    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Tilemap blockingTilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private Tilemap deploymentHighlightTilemap;
    [SerializeField] private TileBase moveHighlightTile;
    [SerializeField] private TileBase attackHighlightTile;
    [SerializeField] private TileBase fusionHighlightTile;
    [SerializeField] private TileBase deploymentHighlightTile;

    [SerializeField] private List<Capsule> playerCapsules = new List<Capsule>();
    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private List<Unit> enemyUnits;

    [SerializeField] private TurnManager turnManager;

    [SerializeField] private FusionManager fusionManager;

    private Unit selectedUnit;
    // for cancelling moves 
    private Vector3Int preMoveCell;
    private bool canUndoMove;

    private Capsule selectedCapsule;

    private Camera mainCamera;
    private ActionMode currentActionMode = ActionMode.None;

    private bool battleEnded;
    private bool inputLocked;

    private void Awake()
    {
        mainCamera = Camera.main;
        turnIndicatorUI.ShowPlacementPhase();
        ShowDeploymentZone();
    }

    
    private void Update()
    {
        if (inputLocked)
        {
            return;
        }

        if (battleEnded)
        {
            return;
        }

        if (!turnManager.IsPlayerTurn())
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DetectClickedCell();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleCancelInput();
            return;
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

        if (battleEnded)
        {
            return;
        }

        if (!combatTilemap.HasTile(cellPosition))
        {
            return;
        }

        Debug.Log($"Clicked cell: {cellPosition}");

        Unit clickedPlayer = GetPlayerUnitAtCell(cellPosition);
        Unit clickedEnemy = GetEnemyUnitAtCell(cellPosition);
        Capsule clickedCapsule = GetPlayerCapsuleAtCell(cellPosition);
        Capsule clickedEnemyCapsule = GetEnemyCapsuleAtCell(cellPosition);

        if (selectedUnit == null)
        {
            if (clickedPlayer != null)
            {
                SelectUnit(clickedPlayer);
                return;
            }

            if (clickedCapsule != null)
            {
                SelectCapsule(clickedCapsule);
                return;
            }

            if (clickedEnemyCapsule != null)
            {
                ShowEnemyCapsulePreview(clickedEnemyCapsule);
                return;
            }

            if (clickedEnemy != null)
            {
                enemyUnitInfoUI.Show(clickedEnemy);
                return;
            }

            return;
        }

        if (clickedEnemyCapsule != null && currentActionMode == ActionMode.None)
        {
            ShowEnemyCapsulePreview(clickedEnemyCapsule);
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
                Unit resultPrefab = fusionManager.GetFusionResultPrefab(selectedUnit, clickedPlayer);

                if (resultPrefab == null)
                {
                    Debug.Log("No valid fusion.");
                    return;
                }

                int fusionCost = resultPrefab.GetCostToSummon();

                if (!turnManager.HasEnoughAP(fusionCost))
                {
                    Debug.Log("Not enough AP for fusion.");
                    return;
                }

                StartCoroutine(FusionSequence(selectedUnit, clickedPlayer, fusionCost));
            }

            return;
        }

        if (currentActionMode == ActionMode.Attack)
        {
            if (clickedEnemy != null)
            {
                TryAttackEnemy(clickedEnemy);
            }

            return;
        }

        if (currentActionMode == ActionMode.Move)
        {
            if (!selectedUnit.CanMoveTo(cellPosition) || IsCellOccupied(cellPosition) || IsCellBlocked(cellPosition))
            {
                Debug.Log("Invalid move.");
                return;
            }

            StartCoroutine(MoveSelectedUnitSequence(cellPosition));
            return;
        }

        if (clickedPlayer != null)
        {
            SelectUnit(clickedPlayer);
            return;
        }

        if (clickedCapsule != null)
        {
            SelectCapsule(clickedCapsule);
            return;
        }
    }

    private Capsule GetPlayerCapsuleAtCell(Vector3Int cellPosition)
    {
        foreach (Capsule capsule in playerCapsules)
        {
            if (capsule != null && capsule.GetCurrentCellPosition() == cellPosition)
            {
                return capsule;
            }
        }

        return null;
    }

    private Capsule GetEnemyCapsuleAtCell(Vector3Int cellPosition)
    {
        return enemyAI.GetEnemyCapsuleAtCell(cellPosition);
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

    public void SummonSelectedCapsule()
    {
        if (selectedCapsule == null)
        {
            return;
        }

        int summonCost = selectedCapsule.GetSummonCost();

        if (!turnManager.HasEnoughAP(summonCost))
        {
            Debug.Log("Not enough AP to summon this capsule.");
            return;
        }

        Vector3Int cellPosition = selectedCapsule.GetCurrentCellPosition();
        Unit unitPrefab = selectedCapsule.GetContainedUnitPrefab();

        turnManager.SpendAP(summonCost);

        Unit spawnedUnit = Instantiate(unitPrefab);
        spawnedUnit.SetCombatTilemap(combatTilemap);
        spawnedUnit.SnapToCell(cellPosition);

        AddPlayerUnit(spawnedUnit);
        enemyAI.AddPlayerUnit(spawnedUnit);

        playerCapsules.Remove(selectedCapsule);
        Destroy(selectedCapsule.gameObject);
        selectedCapsule = null;
        
        spawnedUnit.MarkActed();

        turnManager.IncreaseMaxPlayerAP(summonCost);
        unitInfoUI.Hide();
        actionUI.Hide();

        Debug.Log($"Summoned unit: {spawnedUnit.name}");
    }

    public void ShowMovementRange(Unit unit)
    {
        highlightTilemap.ClearAllTiles();

        Vector3Int currentPosition = unit.GetCurrentCellPosition();

        List<Vector3Int> cells = GridPatternUtility.GetCellsInPattern(
            currentPosition,
            unit.GetMoveRange(),
            unit.GetMovePattern()
        );

        foreach (Vector3Int targetCell in cells)
        {
            if (!combatTilemap.HasTile(targetCell))
            {
                continue;
            }

            if (IsCellOccupied(targetCell) || IsCellBlocked(targetCell))
            {
                continue;
            }

            highlightTilemap.SetTile(targetCell, moveHighlightTile);
        }

        Debug.Log("Showing movement range");
    }

    public void ShowAttackRange(Unit unit)
    {
        highlightTilemap.ClearAllTiles();

        Vector3Int currentPosition = unit.GetCurrentCellPosition();

        List<Vector3Int> cells = GridPatternUtility.GetCellsInPattern(
            currentPosition,
            unit.GetAttackRange(),
            unit.GetAttackPattern()
        );

        foreach (Vector3Int targetCell in cells)
        {
            if (!combatTilemap.HasTile(targetCell))
            {
                continue;
            }

            highlightTilemap.SetTile(targetCell, attackHighlightTile);
        }

        Debug.Log("Showing attack range");
    }

    private void ShowDeploymentZone()
    {
        deploymentHighlightTilemap.ClearAllTiles();

        BoundsInt bounds = combatTilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!combatTilemap.HasTile(cell))
            {
                continue;
            }

            if (cell.y > playerPlacementMaxY)
            {
                continue;
            }

            if (IsCellBlocked(cell))
            {
                continue;
            }

            if (IsCellOccupied(cell))
            {
                continue;
            }

            deploymentHighlightTilemap.SetTile(cell, deploymentHighlightTile);
        }
    }

    private void TryAttackEnemy(Unit targetEnemy)
    {
        StartCoroutine(AttackSequence(targetEnemy));
    }

    private IEnumerator AttackSequence(Unit targetEnemy)
    {
        Unit attacker = selectedUnit;

        if (!attacker.IsInAttackRange(targetEnemy))
        {
            Debug.Log("Enemy is outside attack range. Cannot attack.");
            yield break;
        }

        highlightTilemap.ClearAllTiles();
        actionUI.Hide();
        inputLocked = true;

        Vector3 targetWorldPosition = targetEnemy.transform.position;

        // Lunge
        yield return attacker.AttackLunge(targetWorldPosition);

        // Damage
        int damage = attacker.CalculateDamageAgainst(targetEnemy);

        int oldHp = targetEnemy.CurrentHp;
        targetEnemy.TakeDamage(damage);
        int newHp = targetEnemy.CurrentHp;

        // 🔴 HIT PAUSE
        yield return StartCoroutine(HitPause(0.1f));

        yield return damagePreviewUI.ShowRoutine(targetEnemy, oldHp, newHp);

        targetEnemy.ResolveDeathIfNeeded();

        // Small pause (optional but improves feel)
        yield return new WaitForSeconds(0.1f);

        inputLocked = false;

        if (battleEnded)
        {
            DeselectPlayer();
            yield break;
        }

        // After ANY attack → unit is done
        if (!attacker.HasMovedThisTurn())
        {
            turnManager.SpendAP(attacker.GetActionAPCost());
        }
        attacker.MarkActed();

        DeselectPlayer();
        if (turnManager.CurrentPlayerAP == 0)
        {
            turnManager.EndPlayerTurn();
        }
    }

    private IEnumerator HitPause(float duration)
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }

    private IEnumerator FusionSequence(Unit unitA, Unit unitB, int fusionCost)
    {
        actionUI.Hide();
        highlightTilemap.ClearAllTiles();

        Unit fusedUnit = null;

        yield return StartCoroutine(fusionManager.TryFusionSequence(unitA, unitB, result =>
        {
            fusedUnit = result;

            if (fusedUnit != null)
            {
                DeselectPlayer();
            }
        }));

        if (fusedUnit != null)
        {
            turnManager.SpendAP(fusionCost);
            fusedUnit.MarkActed();

            if (turnManager.CurrentPlayerAP == 0)
            {
                turnManager.EndPlayerTurn();
            }
        }
    }

    private void SelectCapsule(Capsule capsule)
    {
        if (selectedUnit != null)
        {
            selectedUnit.SetSelectedVisual(false);
            selectedUnit = null;
        }

        selectedCapsule = capsule;
        currentActionMode = ActionMode.None;

        highlightTilemap.ClearAllTiles();

        Unit containedUnit = capsule.GetContainedUnitPrefab();
        unitInfoUI.ShowCapsulePreview(containedUnit);

        actionUI.Show(
            false, // canMove
            false, // canAttack
            false, // canFuse
            false, // canFinish
            true,  // canSummon
            capsule.transform.position
        );

        Debug.Log("Capsule selected.");
    }

    private void SelectUnit(Unit unit)
    {
        if (!unit.CanAct())
        {
            Debug.Log("This unit has already acted this turn.");
            return;
        }

        if (selectedUnit != null)
        {
            selectedUnit.SetSelectedVisual(false);
        }

        selectedUnit = unit;
        selectedUnit.SetSelectedVisual(true);
        currentActionMode = ActionMode.None;

        highlightTilemap.ClearAllTiles();

        unitInfoUI.Show(selectedUnit);
        if (selectedUnit.HasMovedThisTurn())
        {
            actionUI.Show(
                false, // canMove
                HasAttackTarget(selectedUnit),
                false, // canFuse
                true,  // canFinish
                false, // canSummon
                selectedUnit.transform.position
            );
        }
        else
        {
            actionUI.Show(
                true, // canMove
                HasAttackTarget(selectedUnit),
                HasFusionTarget(selectedUnit),
                false, // canFinish
                false, // canSummon
                selectedUnit.transform.position
            );
        }

        Debug.Log("Player selected.");
    }

    private void ShowEnemyCapsulePreview(Capsule capsule)
    {
        if (capsule == null)
        {
            return;
        }

        Unit containedUnit = capsule.GetContainedUnitPrefab();

        enemyUnitInfoUI.ShowCapsulePreview(containedUnit);

        Debug.Log("Enemy capsule preview shown.");
    }

    private void DeselectPlayer()
    {
        if (selectedUnit != null)
        {
            selectedUnit.SetSelectedVisual(false);
        }
        selectedUnit = null;
        currentActionMode = ActionMode.None;
        highlightTilemap.ClearAllTiles();
        unitInfoUI.Hide();
        enemyUnitInfoUI.Hide();
        actionUI.Hide();
    }

    public void FinishSelectedUnitAction()
    {
        if (selectedUnit == null)
        {
            return;
        }

        selectedUnit.MarkActed();
        canUndoMove = false;
        DeselectPlayer();
        if (turnManager.CurrentPlayerAP == 0)
        {
            turnManager.EndPlayerTurn();
        }
    }

    public void AddPlayerUnit(Unit unit)
    {
        playerUnits.Add(unit);
        unit.OnUnitDefeated += HandleUnitDefeated;
    }

    public void RemovePlayerUnit(Unit unit)
    {
        unit.OnUnitDefeated -= HandleUnitDefeated;
        playerUnits.Remove(unit);
    }

    public void AddEnemyUnit(Unit unit)
    {
        enemyUnits.Add(unit);
        unit.OnUnitDefeated += HandleUnitDefeated;
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

        foreach (Capsule capsule in playerCapsules)
        {
            if (capsule != null && capsule.GetCurrentCellPosition() == cellPosition)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCellBlocked(Vector3Int cellPosition)
    {
        return blockingTilemap != null && blockingTilemap.HasTile(cellPosition);
    }

    private void HandlePlacement(Vector3Int cellPosition)
    {
        if (!combatTilemap.HasTile(cellPosition))
        {
            return;
        }

        if (IsCellBlocked(cellPosition))
        {
            Debug.Log("Cannot place unit on blocked tile.");
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
            ClearDeploymentZone();
            currentPhase = GamePhase.Battle;
            turnIndicatorUI.ShowPlayerTurn();
            return;
        }

        Capsule placedCapsule = capsuleManager.PlaceNextCapsule(cellPosition);

        if (placedCapsule != null)
        {
            playerCapsules.Add(placedCapsule);
            ShowDeploymentZone();
        }

        if (!capsuleManager.HasCapsulesLeft())
        {
            Debug.Log("All units placed. Starting battle.");
            ClearDeploymentZone();
            currentPhase = GamePhase.Battle;
            turnIndicatorUI.ShowPlayerTurn();
        }
    }

    private void HandleUnitDefeated(Unit unit)
    {
        unit.OnUnitDefeated -= HandleUnitDefeated;

        if (unit.GetTeam() == Unit.Team.Player)
        {
            playerUnits.Remove(unit);
        }
        else
        {
            enemyUnits.Remove(unit);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (enemyUnits.Count == 0)
        {
            battleEnded = true;
            DeselectPlayer();
            turnManager.EndBattle(true);
            return;
        }

        if (playerUnits.Count == 0)
        {
            battleEnded = true;
            DeselectPlayer();
            turnManager.EndBattle(false);
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

        if (!turnManager.HasEnoughAP(selectedUnit.GetActionAPCost()))
        {
            Debug.Log("Not enough AP.");
            return;
        }

        currentActionMode = ActionMode.Move;
        highlightTilemap.ClearAllTiles();
        ShowMovementRange(selectedUnit);
    }

    public void EnterAttackMode()
    {
        if (selectedUnit == null) return;

        if (!selectedUnit.HasMovedThisTurn() &&
    !turnManager.HasEnoughAP(selectedUnit.GetActionAPCost()))
        {
            Debug.Log("Not enough AP.");
            return;
        }

        currentActionMode = ActionMode.Attack;
        highlightTilemap.ClearAllTiles();
        ShowAttackRange(selectedUnit);
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
        if (selectedCapsule != null)
        {
            selectedCapsule = null;
            actionUI.Hide();
            unitInfoUI.Hide();
            highlightTilemap.ClearAllTiles();
            UISoundManager.Instance.PlayCancel();
            return;
        }

        if (selectedUnit == null)
        {
            enemyUnitInfoUI.Hide();
            return;
        }

        if (canUndoMove && selectedUnit.HasMovedThisTurn())
        {
            selectedUnit.SnapToCell(preMoveCell);
            selectedUnit.ResetTurnState();

            turnManager.RefundAP(selectedUnit.GetActionAPCost());

            canUndoMove = false;

            currentActionMode = ActionMode.None;
            highlightTilemap.ClearAllTiles();

            unitInfoUI.Show(selectedUnit);

            actionUI.Show(
                true,
                HasAttackTarget(selectedUnit),
                HasFusionTarget(selectedUnit),
                false,
                false, // canSummon
                selectedUnit.transform.position
            );

            UISoundManager.Instance.PlayCancel();
            Debug.Log("Move undone.");
            return;
        }

        if (currentActionMode != ActionMode.None)
        {
            currentActionMode = ActionMode.None;
            highlightTilemap.ClearAllTiles();

            actionUI.Show(
                selectedUnit.HasMovedThisTurn() == false,
                HasAttackTarget(selectedUnit),
                selectedUnit.HasMovedThisTurn() == false && HasFusionTarget(selectedUnit),
                selectedUnit.HasMovedThisTurn(),
                false, // canSummon
                selectedUnit.transform.position
            );

            UISoundManager.Instance.PlayCancel();
            return;
        }

        UISoundManager.Instance.PlayCancel();
        DeselectPlayer();
    }

    private IEnumerator MoveSelectedUnitSequence(Vector3Int targetCell)
    {
        Unit movingUnit = selectedUnit;
        preMoveCell = movingUnit.GetCurrentCellPosition();
        canUndoMove = true;

        highlightTilemap.ClearAllTiles();
        actionUI.Hide();

        movingUnit.MoveTo(targetCell);

        while (movingUnit.IsMoving)
        {
            yield return null;
        }

        if (battleEnded)
        {
            DeselectPlayer();
            yield break;
        }

        movingUnit.MarkMoved();
        turnManager.SpendAP(movingUnit.GetActionAPCost());

        if (HasAttackTarget(movingUnit))
        {
            selectedUnit = movingUnit;
            currentActionMode = ActionMode.None;

            unitInfoUI.Show(selectedUnit);

            actionUI.Show(
                false, // canMove
                true,  // canAttack
                false, // canFuse
                true,  // canFinish
                false, // canSummon
                selectedUnit.transform.position
            );

            Debug.Log("Unit moved and can still attack.");
            yield break;
        }

        movingUnit.MarkActed();
        canUndoMove = false;
        DeselectPlayer();

        if (turnManager.CurrentPlayerAP == 0)
        {
            turnManager.EndPlayerTurn();
        }
    }

    public void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
    }
    private void ClearDeploymentZone()
    {
        deploymentHighlightTilemap.ClearAllTiles();
    }

    public void ResetPlayerUnitsTurnState()
    {
        foreach (Unit unit in playerUnits)
        {
            if (unit != null)
            {
                unit.ResetTurnState();
            }
        }
    }

    public void EndPlayerTurnButton()
    {
        if (battleEnded)
        {
            return;
        }

        if (!turnManager.IsPlayerTurn())
        {
            return;
        }

        DeselectPlayer();
        turnManager.EndPlayerTurn();
    }
}