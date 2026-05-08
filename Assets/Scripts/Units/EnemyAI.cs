using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private List<Unit> enemyUnits;
    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GridManager gridManager;

    [SerializeField] private DamagePreviewUI damagePreviewUI;

    [SerializeField] private float startTurnDelay = 0.7f;
    [SerializeField] private float delayBetweenEnemyActions = 0.4f;

    public IEnumerator TakeTurnCoroutine()
    {
        yield return new WaitForSeconds(startTurnDelay);

        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
            {
                continue;
            }
            enemy.SetSelectedVisual(true);

            Unit targetInRange = GetBestTargetInRange(enemy);

            if (targetInRange != null)
            {
                gridManager.ShowAttackRange(enemy);
                yield return new WaitForSeconds(0.3f);
                gridManager.ClearHighlights();
                yield return AttackSequence(enemy, targetInRange);
            }
            else
            {
                Unit targetPlayer = GetBestTarget(enemy);

                if (targetPlayer == null)
                {
                    continue;
                }

                Vector3Int? bestCell = GetBestAttackPosition(enemy, targetPlayer);

                if (bestCell.HasValue)
                {
                    gridManager.ShowMovementRange(enemy);
                    yield return new WaitForSeconds(0.3f);
                    gridManager.ClearHighlights();
                    yield return MoveToCellCoroutine(enemy, bestCell.Value);
                }
                else
                {
                    
                    Vector3Int? fallbackCell = GetBestFallbackMovePosition(enemy, targetPlayer);

                    if (fallbackCell.HasValue)
                    {
                        gridManager.ShowMovementRange(enemy);
                        yield return new WaitForSeconds(0.3f);
                        gridManager.ClearHighlights();
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

            if (gridManager.IsCellOccupied(candidate))
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

            score -= GetDistanceFromCell(candidate, target) * 2;
            score -= distanceFromStart;
            score -= CountNearbyPlayerUnits(candidate) * 5;
            score -= CountPlayerUnitsThreateningCell(candidate) * 10;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidate;
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

            if (gridManager.IsCellOccupied(candidate))
            {
                continue;
            }

            if (!enemy.CanMoveTo(candidate))
            {
                continue;
            }

            int score = 0;

            // Main goal: get closer to target
            score -= GetDistanceFromCell(candidate, targetPlayer) * 10;

            // Avoid standing next to multiple players
            score -= CountNearbyPlayerUnits(candidate) * 5;

            // Avoid ending in threatened cells
            score -= CountPlayerUnitsThreateningCell(candidate) * 10;

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
}