using TMPro;
using UnityEngine;

public class APUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI apText;

    private void Awake()
    {
        if (apText == null)
        {
            apText = GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateAP(int currentAP, int maxAP)
    {
        apText.text = $"AP: {currentAP}/{maxAP}";
    }
}