using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlashUI : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public IEnumerator Flash()
    {
        flashImage.gameObject.SetActive(true);

        SetAlpha(1f);

        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);
        flashImage.gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }
}