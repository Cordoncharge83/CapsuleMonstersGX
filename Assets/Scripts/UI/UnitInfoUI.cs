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
    [SerializeField] private Image portraitFrameImage;

    public void Show(Unit unit)
    {
        panel.SetActive(true);

        nameText.text = unit.UnitId;
        hpText.text = $"HP: {unit.CurrentHp}/{unit.MaxHp}";
        statsText.text = $"ATK: {unit.GetAttackPower()}  MOV: {unit.GetMoveRange()}  RNG: {unit.GetAttackRange()}";
        elementText.text = $"Element: {unit.GetElementType()}";

        portraitImage.sprite = unit.Portrait;
        portraitImage.enabled = unit.Portrait != null;

        Color elementColor = GetElementColor(unit.GetElementType());

        panel.GetComponent<Image>().color = new Color(elementColor.r * 0.5f, elementColor.g * 0.5f, elementColor.b * 0.5f, 0.85f);
        portraitFrameImage.color = elementColor;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color(0.5f, 0.1f, 0.1f, 0.8f);
            case ElementType.Water: return new Color(0.1f, 0.1f, 0.5f, 0.8f);
            case ElementType.Wind: return new Color(0.1f, 0.5f, 0.3f, 0.8f);
            case ElementType.Earth: return new Color(0.4f, 0.3f, 0.1f, 0.8f);
            case ElementType.Light: return new Color(1f, 0.95f, 0.75f, 0.8f);
            case ElementType.Dark: return new Color(0.3f, 0.1f, 0.4f, 0.8f);
            default: return Color.gray;
        }
    }
}