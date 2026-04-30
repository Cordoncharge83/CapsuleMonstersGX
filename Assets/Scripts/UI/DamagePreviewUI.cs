using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamagePreviewUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider hpSlider;

    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private float visibleDuration = 0.8f;

    private Coroutine currentRoutine;

    public IEnumerator ShowRoutine(Unit unit, int oldHp, int newHp)
    {
        panel.SetActive(true);

        int maxHp = unit.MaxHp;

        hpSlider.maxValue = maxHp;
        hpSlider.value = oldHp;
        hpText.text = $"{oldHp}/   {maxHp}";

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / animationDuration;
            float currentValue = Mathf.Lerp(oldHp, newHp, progress);

            hpSlider.value = currentValue;
            hpText.text = $"{Mathf.RoundToInt(currentValue)}/   {maxHp}";

            yield return null;
        }

        hpSlider.value = newHp;
        hpText.text = $"{newHp}/   {maxHp}";

        yield return new WaitForSeconds(visibleDuration);

        panel.SetActive(false);
    }
}