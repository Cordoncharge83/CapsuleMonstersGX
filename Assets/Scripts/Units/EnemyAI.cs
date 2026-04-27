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

            Unit targetPlayer = GetClosestPlayerUnit(enemy);

            if (targetPlayer == null)
            {
                continue;
            }

            if (enemy.IsInAttackRange(targetPlayer))
            {
                int damage = enemy.CalculateDamageAgainst(targetPlayer);
                targetPlayer.TakeDamage(damage);
            }
            else
            {
                MoveTowardPlayer(enemy, targetPlayer);
            }

            yield return new WaitForSeconds(delayBetweenEnemyActions);
        }

        turnManager.EndEnemyTurn();
    }

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

    private void MoveTowardPlayer(Unit enemy, Unit targetPlayer)
    {
        for (int i = 0; i < enemy.GetMoveRange(); i++)
        {
            if (enemy.IsInAttackRange(targetPlayer))
            {
                return;
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
                return;
            }

            if (gridManager.IsCellOccupied(targetCell))
            {
                return;
            }

            enemy.MoveTo(targetCell);
        }
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