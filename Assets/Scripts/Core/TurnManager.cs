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

    private bool battleEnded;

    public bool IsPlayerTurn()
    {
        return currentTurn == TurnState.PlayerTurn;
    }

    public void EndPlayerTurn()
    {
        if (battleEnded)
        {
            return;
        }
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("Enemy Turn");
        turnIndicatorUI.ShowEnemyTurn();

        StartCoroutine(enemyAI.TakeTurnCoroutine());
    }

    public void EndEnemyTurn()
    {
        if (battleEnded)
        {
            return;
        }
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Player Turn");
        turnIndicatorUI.ShowPlayerTurn();
    }

    public void EndBattle()
    {
        battleEnded = true;
    }
}
