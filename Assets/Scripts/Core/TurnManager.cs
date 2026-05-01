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
    [SerializeField] private GridManager gridManager;

    [Header("Action Points")]
    [SerializeField] private int maxPlayerAP = 2;
    [SerializeField] private APUI apUI;

    public TurnState currentTurn = TurnState.PlayerTurn;

    private int currentPlayerAP;
    private bool battleEnded;

    public int CurrentPlayerAP => currentPlayerAP;
    public int MaxPlayerAP => maxPlayerAP;

    private void Start()
    {
        RestorePlayerAP();
        apUI.UpdateAP(currentPlayerAP, maxPlayerAP);
        gridManager.ResetPlayerUnitsTurnState();
    }

    public bool IsPlayerTurn()
    {
        return currentTurn == TurnState.PlayerTurn;
    }

    public bool HasEnoughAP(int cost)
    {
        return currentPlayerAP >= cost;
    }

    public void SpendAP(int cost)
    {
        currentPlayerAP -= cost;
        currentPlayerAP = Mathf.Max(0, currentPlayerAP);
        apUI.UpdateAP(currentPlayerAP, maxPlayerAP);

        Debug.Log($"AP: {currentPlayerAP}/{maxPlayerAP}");
    }

    public void RefundAP(int amount)
    {
        currentPlayerAP += amount;
        currentPlayerAP = Mathf.Min(currentPlayerAP, maxPlayerAP);

        Debug.Log($"AP refunded: {currentPlayerAP}/{maxPlayerAP}");

        apUI.UpdateAP(currentPlayerAP, maxPlayerAP);
    }

    private void RestorePlayerAP()
    {
        currentPlayerAP = maxPlayerAP;
        apUI.UpdateAP(currentPlayerAP, maxPlayerAP);
        Debug.Log($"Player AP restored: {currentPlayerAP}/{maxPlayerAP}");
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
        RestorePlayerAP();
        gridManager.ResetPlayerUnitsTurnState();

        Debug.Log("Player Turn");
        turnIndicatorUI.ShowPlayerTurn();
    }

    public void EndBattle()
    {
        battleEnded = true;
    }
}