using System.Collections;
using TMPro;
using UnityEngine;

public class TurnIndicatorUI : MonoBehaviour
{
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private float displayDuration = 1f;

    private Coroutine hideCoroutine;

    public void ShowPlacementPhase()
    {
        ShowTemporaryMessage("Placement Phase");
    }

    public void ShowPlayerTurn()
    {
        ShowTemporaryMessage("Player Turn");
    }

    public void ShowEnemyTurn()
    {
        ShowTemporaryMessage("Enemy Turn");
    }

    private void ShowTemporaryMessage(string message)
    {
        turnText.gameObject.SetActive(true);
        turnText.text = message;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        turnText.gameObject.SetActive(false);
    }
}