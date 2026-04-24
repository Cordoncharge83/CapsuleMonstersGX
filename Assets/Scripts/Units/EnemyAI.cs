using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Unit enemyUnit;
    [SerializeField] private Unit playerUnit;
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private TurnManager turnManager;

    public void TakeTurn()
    {
        if (enemyUnit == null || playerUnit == null)
        {
            turnManager.EndEnemyTurn();
            return;
        }

        if (enemyUnit.IsInAttackRange(playerUnit))
        {
            playerUnit.TakeDamage(enemyUnit.GetAttackPower());
            turnManager.EndEnemyTurn();
            return;
        }

        MoveTowardPlayer();
        turnManager.EndEnemyTurn();
    }

    private void MoveTowardPlayer()
    {
        for (int i = 0; i < enemyUnit.GetMoveRange(); i++)
        {
            if (enemyUnit.IsInAttackRange(playerUnit))
            {
                return;
            }

            Vector3Int enemyCell = enemyUnit.GetCurrentCellPosition();
            Vector3Int playerCell = playerUnit.GetCurrentCellPosition();

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

            if (targetCell == playerUnit.GetCurrentCellPosition())
            {
                return;
            }

            enemyUnit.MoveTo(targetCell);
        }
    }
}