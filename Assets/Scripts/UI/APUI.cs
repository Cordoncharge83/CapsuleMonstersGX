using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class APUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI currentAPText;
    [SerializeField] private TextMeshProUGUI gainText;

    [Header("Bar")]
    [SerializeField] private Image fillImage;

    [Header("Animation")]
    [SerializeField] private float fillDuration = 0.35f;
    [SerializeField] private float numberStepDelay = 0.02f;

    private Coroutine fillCoroutine;
    private Coroutine numberCoroutine;

    private int displayedAP = -1;

    public void UpdateAP(int currentAP, int maxAP)
    {
        gainText.text = $"(+{maxAP})";

        AnimateNumber(currentAP);
        AnimateBar(currentAP, maxAP);
    }

    private void AnimateNumber(int targetAP)
    {
        if (numberCoroutine != null)
        {
            StopCoroutine(numberCoroutine);
        }

        numberCoroutine = StartCoroutine(AnimateNumberRoutine(targetAP));
    }

    private IEnumerator AnimateNumberRoutine(int targetAP)
    {
        if (displayedAP < 0)
        {
            displayedAP = targetAP;
            currentAPText.text = displayedAP.ToString();
            yield break;
        }

        while (displayedAP != targetAP)
        {
            displayedAP += displayedAP < targetAP ? 1 : -1;
            currentAPText.text = displayedAP.ToString();

            yield return new WaitForSeconds(numberStepDelay);
        }

        numberCoroutine = null;
    }

    private void AnimateBar(int currentAP, int maxAP)
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }

        float targetFill = maxAP > 0 ? (float)currentAP / maxAP : 0f;
        fillCoroutine = StartCoroutine(AnimateFill(targetFill));
    }

    private IEnumerator AnimateFill(float target)
    {
        float start = fillImage.fillAmount;
        float time = 0f;

        while (time < fillDuration)
        {
            time += Time.deltaTime;
            fillImage.fillAmount = Mathf.Lerp(start, target, time / fillDuration);
            yield return null;
        }

        fillImage.fillAmount = target;
        fillCoroutine = null;
    }
}