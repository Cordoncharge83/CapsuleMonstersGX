using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    public TurnState currentTurn = TurnState.PlayerTurn;

    public bool IsPlayerTurn()
    {
        return currentTurn == TurnState.PlayerTurn;
    }

    public void EndPlayerTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("Enemy Turn");
    }

    public void EndEnemyTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Player Turn");
    }
}
