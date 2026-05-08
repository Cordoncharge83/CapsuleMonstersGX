using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panel;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text apText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text moveText;
    [SerializeField] private TMP_Text attackRangeText;

    [Header("Images")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image elementIconImage;
    [SerializeField] private Image movePatternIconImage;
    [SerializeField] private Image attackPatternIconImage;

    [Header("Element Icons")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite windIcon;
    [SerializeField] private Sprite earthIcon;
    [SerializeField] private Sprite lightIcon;
    [SerializeField] private Sprite darkIcon;

    [Header("Pattern Icons")]
    [SerializeField] private Sprite crossIcon;
    [SerializeField] private Sprite diagonalIcon;
    [SerializeField] private Sprite diamondIcon;

    public void Show(Unit unit)
    {
        if (unit == null)
        {
            Hide();
            return;
        }

        panel.SetActive(true);

        nameText.text = unit.UnitId;
        levelText.text = "1";
        apText.text = unit.GetCostToSummon().ToString();

        attackText.text = unit.GetAttackPower().ToString();
        defenseText.text = unit.GetDefense().ToString();
        hpText.text = $"{unit.CurrentHp}/{unit.MaxHp}";

        moveText.text = unit.GetMoveRange().ToString();
        attackRangeText.text = unit.GetAttackRange().ToString();

        portraitImage.sprite = unit.Portrait;
        portraitImage.enabled = unit.Portrait != null;

        elementIconImage.sprite = GetElementIcon(unit.GetElementType());
        elementIconImage.enabled = elementIconImage.sprite != null;

        movePatternIconImage.sprite = GetPatternIcon(unit.GetMovePattern());
        movePatternIconImage.enabled = movePatternIconImage.sprite != null;

        attackPatternIconImage.sprite = GetPatternIcon(unit.GetAttackPattern());
        attackPatternIconImage.enabled = attackPatternIconImage.sprite != null;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private Sprite GetElementIcon(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return fireIcon;

            case ElementType.Water:
                return waterIcon;

            case ElementType.Wind:
                return windIcon;

            case ElementType.Earth:
                return earthIcon;

            case ElementType.Light:
                return lightIcon;

            case ElementType.Dark:
                return darkIcon;

            default:
                return null;
        }
    }

    private Sprite GetPatternIcon(GridPatternType pattern)
    {
        switch (pattern)
        {
            case GridPatternType.Cross:
                return crossIcon;

            case GridPatternType.Diagonal:
                return diagonalIcon;

            case GridPatternType.Diamond:
                return diamondIcon;

            default:
                return null;
        }
    }
}