using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private TurnIndicatorUI turnIndicatorUI;

    public TurnState currentTurn = TurnState.PlayerTurn;

    public bool IsPlayerTurn()
    {
        return currentTurn == TurnState.PlayerTurn;
    }

    public void EndPlayerTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("Enemy Turn");
        turnIndicatorUI.ShowEnemyTurn();

        StartCoroutine(enemyAI.TakeTurnCoroutine());
    }

    public void EndEnemyTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Player Turn");
        turnIndicatorUI.ShowPlayerTurn();
    }
}
