using TMPro;
using UnityEngine;

public class BattleResultUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text resultText;


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

}