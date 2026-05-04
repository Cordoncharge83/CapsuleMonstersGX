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

            Unit targetPlayer = GetBestTarget(enemy);

            if (targetPlayer == null)
            {
                continue;
            }

            if (enemy.IsInAttackRange(targetPlayer))
            {
                yield return AttackSequence(enemy, targetPlayer);
            }
            else
            {
                Vector3Int? bestCell = GetBestAttackPosition(enemy, targetPlayer);

                if (bestCell.HasValue)
                {
                    yield return MoveToCellCoroutine(enemy, bestCell.Value);
                }
                else
                {
                    yield return MoveTowardPlayerCoroutine(enemy, targetPlayer);
                }

                if (enemy != null && targetPlayer != null && enemy.IsInAttackRange(targetPlayer))
                {
                    yield return new WaitForSeconds(delayBetweenEnemyActions);
                    yield return AttackSequence(enemy, targetPlayer);
                }
            }

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

        Vector3Int? bestCell = null;
        int bestDistance = int.MaxValue;

        int moveRange = enemy.GetMoveRange();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                Vector3Int candidate = enemyCell + new Vector3Int(x, y, 0);

                // skip invalid tiles
                if (!combatTilemap.HasTile(candidate)) continue;
                if (gridManager.IsCellOccupied(candidate)) continue;

                // skip cells outside movement range (diamond)
                int distanceFromStart = Mathf.Abs(x) + Mathf.Abs(y);
                if (distanceFromStart > moveRange) continue;

                // check if from this tile we can attack target
                if (IsCellInAttackRange(enemy, target, candidate))
                {
                    int distToTarget = GetDistanceFromCell(candidate, target);

                    if (distToTarget < bestDistance)
                    {
                        bestDistance = distToTarget;
                        bestCell = candidate;
                    }
                }
            }
        }

        return bestCell;
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

    private IEnumerator MoveTowardPlayerCoroutine(Unit enemy, Unit targetPlayer)
    {
        for (int i = 0; i < enemy.GetMoveRange(); i++)
        {
            if (enemy == null || targetPlayer == null)
            {
                yield break;
            }

            if (enemy.IsInAttackRange(targetPlayer))
            {
                yield break;
            }

            Vector3Int enemyCell = enemy.GetCurrentCellPosition();
            Vector3Int playerCell = targetPlayer.GetCurrentCellPosition();

            Vector3Int direction = Vector3Int.zero;

            int deltaX = playerCell.x - enemyCell.x;
            int deltaY = playerCell.y - enemyCell.y;

            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                direction.x = deltaX > 0 ? 1 : -1;
            }
            else if (deltaY != 0)
            {
                direction.y = deltaY > 0 ? 1 : -1;
            }

            Vector3Int targetCell = enemyCell + direction;

            if (!combatTilemap.HasTile(targetCell))
            {
                yield break;
            }

            if (gridManager.IsCellOccupied(targetCell))
            {
                yield break;
            }

            enemy.MoveTo(targetCell);

            while (enemy != null && enemy.IsMoving)
            {
                yield return null;
            }
        }
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