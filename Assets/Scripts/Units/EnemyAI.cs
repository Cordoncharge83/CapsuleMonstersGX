using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private List<Unit> enemyUnits;
    [SerializeField] private List<Capsule> enemyCapsules = new List<Capsule>();
    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GridManager gridManager;

    [SerializeField] private DamagePreviewUI damagePreviewUI;

    [SerializeField] private float startTurnDelay = 0.7f;
    [SerializeField] private float delayBetweenEnemyActions = 0.4f;

    private Dictionary<Unit, Vector3Int> previousPositions = new Dictionary<Unit, Vector3Int>();


    private bool TrySummonEnemyCapsule()
    {
        foreach (Capsule capsule in enemyCapsules)
        {
            if (capsule == null)
            {
                continue;
            }

            int summonCost = capsule.GetSummonCost();

            if (!turnManager.EnemyHasEnoughAP(summonCost))
            {
                continue;
            }

            Vector3Int cellPosition = capsule.GetCurrentCellPosition();
            Unit unitPrefab = capsule.GetContainedUnitPrefab();

            turnManager.EnemySpendAP(summonCost);

            Unit spawnedUnit = Instantiate(unitPrefab);
            spawnedUnit.SetCombatTilemap(combatTilemap);
            spawnedUnit.SnapToCell(cellPosition);
            spawnedUnit.MarkActed();

            AddEnemyUnit(spawnedUnit);
            gridManager.AddEnemyUnit(spawnedUnit);

            turnManager.IncreaseMaxEnemyAP(summonCost);

            enemyCapsules.Remove(capsule);
            Destroy(capsule.gameObject);

            Debug.Log($"Enemy summoned unit: {spawnedUnit.name}");

            return true;
        }

        return false;
    }
    public IEnumerator TakeTurnCoroutine()
    {
        ResetEnemyUnitsTurnState();

        yield return new WaitForSeconds(startTurnDelay);

        while (TrySummonEnemyCapsule())
        {
            yield return new WaitForSeconds(startTurnDelay);
        }

        foreach (Unit enemy in enemyUnits)
        {
       
            if (enemy == null)
            {
                continue;
            }

            if (!enemy.CanAct())
            {
                continue;
            }

            if (!turnManager.EnemyHasEnoughAP(enemy.GetActionAPCost()))
            {
                break;
            }
            enemy.SetSelectedVisual(true);

            Unit targetInRange = GetBestTargetInRange(enemy);

            if (targetInRange != null)
            {
                gridManager.ShowAttackRange(enemy);
                yield return new WaitForSeconds(0.3f);
                gridManager.ClearHighlights();
                turnManager.EnemySpendAP(enemy.GetActionAPCost());
                yield return AttackSequence(enemy, targetInRange);
            }
            else
            {
                Vector3Int? bestCell = GetBestAttackPositionAgainstAnyTarget(enemy);

                if (bestCell.HasValue)
                {
                    gridManager.ShowMovementRange(enemy);
                    yield return new WaitForSeconds(0.3f);
                    gridManager.ClearHighlights();
                    turnManager.EnemySpendAP(enemy.GetActionAPCost());
                    yield return MoveToCellCoroutine(enemy, bestCell.Value);
                }
                else
                {
                    Unit targetPlayer = GetBestTarget(enemy);

                    if (targetPlayer == null)
                    {
                        continue;
                    }
                    Vector3Int? fallbackCell = GetBestFallbackMovePosition(enemy, targetPlayer);

                    if (fallbackCell.HasValue)
                    {
                        gridManager.ShowMovementRange(enemy);
                        yield return new WaitForSeconds(0.3f);
                        gridManager.ClearHighlights();
                        turnManager.EnemySpendAP(enemy.GetActionAPCost());
                        yield return MoveToCellCoroutine(enemy, fallbackCell.Value);
                    }
                }

                Unit postMoveTarget = GetBestTargetInRange(enemy);

                if (postMoveTarget != null)
                {
                    yield return new WaitForSeconds(delayBetweenEnemyActions);
                    gridManager.ShowAttackRange(enemy);
                    yield return new WaitForSeconds(0.25f);
                    gridManager.ClearHighlights();
                    yield return AttackSequence(enemy, postMoveTarget);
                }
            }
            enemy.SetSelectedVisual(false);
            yield return new WaitForSeconds(delayBetweenEnemyActions);
        }

        turnManager.EndEnemyTurn();
    }

    private Unit GetBestTarget(Unit enemy)
    {
        Unit bestTarget = null;
        int bestScore = int.MinValue;

        foreach (Unit player in playerUnits)
        {
            if (player == null) continue;

            int score = 0;

            // Distance (closer is better)
            int distance = GetDistance(enemy, player);
            score -= distance * 2;

            // Low HP priority
            score += (player.MaxHp - player.CurrentHp);

            // Kill potential bonus
            int damage = enemy.CalculateDamageAgainst(player);
            if (damage >= player.CurrentHp)
            {
                score += 100;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = player;
            }
        }

        return bestTarget;
    }

    private int GetDistance(Unit a, Unit b)
    {
        Vector3Int aCell = a.GetCurrentCellPosition();
        Vector3Int bCell = b.GetCurrentCellPosition();

        return Mathf.Abs(aCell.x - bCell.x) + Mathf.Abs(aCell.y - bCell.y);
    }

    // for now this is useless, only here for potential debugging whilst working on EnemyAI v2
    private Unit GetClosestPlayerUnit(Unit enemy)
    {
        Unit closestPlayer = null;
        int closestDistance = int.MaxValue;

        foreach (Unit player in playerUnits)
        {
            if (player == null)
            {
                continue;
            }

            Vector3Int enemyCell = enemy.GetCurrentCellPosition();
            Vector3Int playerCell = player.GetCurrentCellPosition();

            int distance =
                Mathf.Abs(enemyCell.x - playerCell.x) +
                Mathf.Abs(enemyCell.y - playerCell.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    private Vector3Int? GetBestAttackPosition(Unit enemy, Unit target)
    {
        Vector3Int enemyCell = enemy.GetCurrentCellPosition();

        List<Vector3Int> movementCells = GridPatternUtility.GetCellsInPattern(
            enemyCell,
            enemy.GetMoveRange(),
            enemy.GetMovePattern()
        );

        Vector3Int? bestCell = null;
        int bestScore = int.MinValue;

        foreach (Vector3Int candidate in movementCells)
        {
            if (!combatTilemap.HasTile(candidate))
            {
                continue;
            }

            if (gridManager.IsCellOccupied(candidate) || gridManager.IsCellBlocked(candidate))
            {
                continue;
            }

            if (!enemy.CanMoveTo(candidate))
            {
                continue;
            }

            if (!IsCellInAttackRange(enemy, target, candidate))
            {
                continue;
            }
            int distanceFromStart = GetDistanceBetweenCells(enemyCell, candidate);

            int score = 0;
            score += 150;
            score -= GetDistanceFromCell(candidate, target) * 2;
            //score -= distanceFromStart;
            score -= CountNearbyPlayerUnits(candidate) * 5;
            score -= CountPlayerUnitsThreateningCell(candidate) * 4;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidate;
            }
        }

        return bestCell;
    }

    private Vector3Int? GetBestAttackPositionAgainstAnyTarget(Unit enemy)
    {
        Vector3Int? bestCell = null;
        int bestScore = int.MinValue;

        foreach (Unit player in playerUnits)
        {
            if (player == null)
            {
                continue;
            }

            Vector3Int? candidateCell = GetBestAttackPosition(enemy, player);

            if (!candidateCell.HasValue)
            {
                continue;
            }

            int damage = enemy.CalculateDamageAgainst(player);

            int score = 0;

            // Prefer kills
            if (damage >= player.CurrentHp)
            {
                score += 100;
            }

            // Prefer damaged / low HP targets
            score += player.MaxHp - player.CurrentHp;

            // Prefer higher damage
            score += damage * 2;

            // Prefer closer target after movement
            score -= GetDistanceFromCell(candidateCell.Value, player) * 2;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidateCell.Value;
            }
        }

        return bestCell;
    }

    private int CountNearbyPlayerUnits(Vector3Int cell)
    {
        int count = 0;

        foreach (Unit player in playerUnits)
        {
            if (player == null)
            {
                continue;
            }

            Vector3Int playerCell = player.GetCurrentCellPosition();

            int distance =
                Mathf.Abs(cell.x - playerCell.x) +
                Mathf.Abs(cell.y - playerCell.y);

            if (distance <= 1)
            {
                count++;
            }
        }

        return count;
    }

    private int CountPlayerUnitsThreateningCell(Vector3Int cell)
    {
        int count = 0;

        foreach (Unit player in playerUnits)
        {
            if (player == null)
            {
                continue;
            }

            Vector3Int playerCell = player.GetCurrentCellPosition();

            int distance =
                Mathf.Abs(cell.x - playerCell.x) +
                Mathf.Abs(cell.y - playerCell.y);

            if (distance <= player.GetAttackRange())
            {
                count++;
            }
        }

        return count;
    }

    private bool IsCellInAttackRange(Unit attacker, Unit target, Vector3Int fromCell)
    {
        Vector3Int targetCell = target.GetCurrentCellPosition();

        int distance =
            Mathf.Abs(fromCell.x - targetCell.x) +
            Mathf.Abs(fromCell.y - targetCell.y);

        return distance <= attacker.GetAttackRange();
    }

    private int GetDistanceFromCell(Vector3Int fromCell, Unit target)
    {
        Vector3Int targetCell = target.GetCurrentCellPosition();

        return Mathf.Abs(fromCell.x - targetCell.x) +
               Mathf.Abs(fromCell.y - targetCell.y);
    }

    private Unit GetBestTargetInRange(Unit enemy)
    {
        Unit bestTarget = null;
        int bestScore = int.MinValue;

        foreach (Unit player in playerUnits)
        {
            if (player == null)
            {
                continue;
            }

            if (!enemy.IsInAttackRange(player))
            {
                continue;
            }

            int damage = enemy.CalculateDamageAgainst(player);

            int score = 0;

            // Strongly prefer kills
            if (damage >= player.CurrentHp)
            {
                score += 100;
            }

            // Prefer lower HP targets
            score += player.MaxHp - player.CurrentHp;

            // Prefer dealing more damage
            score += damage * 2;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = player;
            }
        }

        return bestTarget;
    }

    private Vector3Int? GetBestFallbackMovePosition(Unit enemy, Unit targetPlayer)
    {
        Vector3Int enemyCell = enemy.GetCurrentCellPosition();
        Vector3Int targetCell = targetPlayer.GetCurrentCellPosition();

        List<Vector3Int> goalCells = GetAttackGoalCells(enemy, targetPlayer);

        if (goalCells.Count == 0)
        {
            return null;
        }

        int currentPathDistance = GetShortestPathDistanceToAnyCell(enemyCell, goalCells);

        List<Vector3Int> movementCells = GridPatternUtility.GetCellsInPattern(
            enemyCell,
            enemy.GetMoveRange(),
            enemy.GetMovePattern()
        );

        Vector3Int? bestCell = null;
        int bestScore = int.MinValue;

        foreach (Vector3Int candidate in movementCells)
        {
            if (!combatTilemap.HasTile(candidate))
            {
                continue;
            }

            if (gridManager.IsCellOccupied(candidate) || gridManager.IsCellBlocked(candidate))
            {
                continue;
            }

            if (!enemy.CanMoveTo(candidate))
            {
                continue;
            }

            int candidatePathDistance = GetShortestPathDistanceToAnyCell(candidate, goalCells);

            if (candidatePathDistance == int.MaxValue)
            {
                continue;
            }

            int score = 0;

            // Strongly reward real path progress around obstacles
            int progress = currentPathDistance - candidatePathDistance;
            score += progress * 50;

            if (previousPositions.TryGetValue(enemy, out Vector3Int previousPosition))
            {
                int previousDistance = GetShortestPathDistanceToAnyCell(previousPosition, goalCells);

                // Discourage undoing previous progress
                if (candidatePathDistance >= previousDistance)
                {
                    score -= 20;
                }

                // Strongly discourage direct backtracking
                if (candidate == previousPosition)
                {
                    score -= 50;
                }
            }

            // Prefer tiles with shorter remaining path
            score -= candidatePathDistance * 5;

            // Avoid dangerous positions
            score -= CountNearbyPlayerUnits(candidate) * 5;
            score -= CountPlayerUnitsThreateningCell(candidate) * 10;

            // Small tie-breaker: prefer using movement
            score += GetDistanceBetweenCells(enemyCell, candidate);

            Debug.Log(
                $"{enemy.UnitId} candidate {candidate} | " +
                $"progress: {progress} | " +
                $"pathDist: {candidatePathDistance} | " +
                $"nearby: {CountNearbyPlayerUnits(candidate)} | " +
                $"threat: {CountPlayerUnitsThreateningCell(candidate)} | " +
                $"score: {score}"
            );
            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidate;
            }
        }

        return bestCell;
    }

    private IEnumerator MoveToCellCoroutine(Unit enemy, Vector3Int targetCell)
    {
        previousPositions[enemy] = enemy.GetCurrentCellPosition();
        enemy.MoveTo(targetCell);

        while (enemy != null && enemy.IsMoving)
        {
            yield return null;
        }
    }

    private IEnumerator AttackSequence(Unit attacker, Unit target)
    {
        Vector3 targetWorldPosition = target.transform.position;

        // Lunge
        yield return attacker.AttackLunge(targetWorldPosition);

        // Damage
        int damage = attacker.CalculateDamageAgainst(target);

        int oldHp = target.CurrentHp;
        target.TakeDamage(damage);
        int newHp = target.CurrentHp;

        yield return damagePreviewUI.ShowRoutine(target, oldHp, newHp);

        target.ResolveDeathIfNeeded();

        // Small pause for readability
        yield return new WaitForSeconds(0.1f);
    }

    private List<Vector3Int> GetWalkableNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
    {
        cell + Vector3Int.up,
        cell + Vector3Int.down,
        cell + Vector3Int.left,
        cell + Vector3Int.right
    };

        List<Vector3Int> validNeighbors = new List<Vector3Int>();

        foreach (Vector3Int neighbor in neighbors)
        {
            if (!combatTilemap.HasTile(neighbor))
            {
                continue;
            }

            if (gridManager.IsCellBlocked(neighbor))
            {
                continue;
            }

            if (gridManager.IsCellOccupied(neighbor))
            {
                continue;
            }

            validNeighbors.Add(neighbor);
        }

        return validNeighbors;
    }

    private int GetShortestPathDistanceToAnyCell(Vector3Int start, List<Vector3Int> goals)
    {
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distance = new Dictionary<Vector3Int, int>();

        frontier.Enqueue(start);
        distance[start] = 0;

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (goals.Contains(current))
            {
                return distance[current];
            }

            foreach (Vector3Int next in GetWalkableNeighbors(current))
            {
                if (distance.ContainsKey(next))
                {
                    continue;
                }

                distance[next] = distance[current] + 1;
                frontier.Enqueue(next);
            }
        }

        return int.MaxValue;
    }

    private List<Vector3Int> GetAttackGoalCells(Unit attacker, Unit target)
    {
        Vector3Int targetCell = target.GetCurrentCellPosition();

        List<Vector3Int> goalCells = GridPatternUtility.GetCellsInPattern(
            targetCell,
            attacker.GetAttackRange(),
            attacker.GetAttackPattern()
        );

        goalCells.RemoveAll(cell =>
            !combatTilemap.HasTile(cell) ||
            gridManager.IsCellBlocked(cell) ||
            gridManager.IsCellOccupied(cell)
        );

        return goalCells;
    }

    public Capsule GetEnemyCapsuleAtCell(Vector3Int cellPosition)
    {
        foreach (Capsule capsule in enemyCapsules)
        {
            if (capsule != null && capsule.GetCurrentCellPosition() == cellPosition)
            {
                return capsule;
            }
        }

        return null;
    }

    private void ResetEnemyUnitsTurnState()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
            {
                continue;
            }

            enemy.ResetTurnState();
        }
    }

    private int GetDistanceBetweenCells(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

    public void AddEnemyCapsule(Capsule capsule)
    {
        enemyCapsules.Add(capsule);
    }
}