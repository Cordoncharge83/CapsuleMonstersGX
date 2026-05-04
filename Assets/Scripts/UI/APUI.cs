using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class APUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI currentAPText;
    [SerializeField] private TextMeshProUGUI gainText;

    [Header("Bar")]
    [SerializeField] private Image fillImage;

    public void UpdateAP(int currentAP, int maxAP)
    {
        // Text
        currentAPText.text = currentAP.ToString();
        gainText.text = $"(+{maxAP})";

        // Bar
        if (maxAP > 0)
        {
            StartCoroutine(AnimateFill((float)currentAP / maxAP));
        }
        else
        {
            fillImage.fillAmount = 0f;
        }
    }

    private IEnumerator AnimateFill(float target)
    {
        float start = fillImage.fillAmount;
        float time = 0f;
        float duration = 0.35f;

        while (time < duration)
        {
            time += Time.deltaTime;
            fillImage.fillAmount = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        fillImage.fillAmount = target;
    }
}