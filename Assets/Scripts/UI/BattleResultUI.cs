using TMPro;
using UnityEngine;

public class BattleResultUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        Hide();
    }

    public void ShowVictory()
    {
        panel.SetActive(true);
        resultText.text = "VICTORY";
    }

    public void ShowDefeat()
    {
        panel.SetActive(true);
        resultText.text = "DEFEAT";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}