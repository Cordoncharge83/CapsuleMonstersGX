using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButtonHover : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private ActionUI actionUI;
    [SerializeField] private Button button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        actionUI.SetHighlight(button);
    }
}
