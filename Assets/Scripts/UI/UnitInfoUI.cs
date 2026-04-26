using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text elementText;
    [SerializeField] private Image portraitImage;

    public void Show(Unit unit)
    {
        panel.SetActive(true);

        nameText.text = unit.UnitId;
        hpText.text = $"HP: {unit.CurrentHp}/{unit.MaxHp}";
        statsText.text = $"ATK: {unit.GetAttackPower()}  MOV: {unit.GetMoveRange()}  RNG: {unit.GetAttackRange()}";
        elementText.text = $"Element: {unit.GetElementType()}";

        portraitImage.sprite = unit.Portrait;
        portraitImage.enabled = unit.Portrait != null;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}