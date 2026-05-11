using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform actionPanelRect;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button finishButton;
    [SerializeField] private Button summonButton;

    [SerializeField] private GridManager gridManager;

    [Header("Highlight")]
    [SerializeField] private RectTransform highlight;
    [SerializeField] private Button[] actionButtons;

    private Coroutine defaultHighlightCoroutine;

    private void Awake()
    {
        moveButton.onClick.AddListener(() =>
        {
            Hide();
            gridManager.EnterMoveMode();
        });

        attackButton.onClick.AddListener(() =>
        {
            Hide();
            gridManager.EnterAttackMode();
        });

        fuseButton.onClick.AddListener(() =>
        {
            Hide();
            gridManager.EnterFuseMode();
        });

        finishButton.onClick.AddListener(() =>
        {
            Hide();
            gridManager.FinishSelectedUnitAction();
        });

        summonButton.onClick.AddListener(() =>
        {
            Hide();
            gridManager.SummonSelectedCapsule();
        });

        cancelButton.onClick.AddListener(gridManager.CancelAction);

        Hide();
    }

    public void Show(bool canMove, bool canAttack, bool canFuse, bool canFinish, bool canSummon, Vector3 worldPosition)
    {
        SetPosition(worldPosition); // move while still hidden

        panel.SetActive(true);

        moveButton.gameObject.SetActive(canMove);
        summonButton.gameObject.SetActive(canSummon);
        attackButton.gameObject.SetActive(canAttack);
        fuseButton.gameObject.SetActive(canFuse);
        finishButton.gameObject.SetActive(canFinish);
        cancelButton.gameObject.SetActive(true);

        if (defaultHighlightCoroutine != null)
        {
            StopCoroutine(defaultHighlightCoroutine);
        }

        defaultHighlightCoroutine = StartCoroutine(RebuildAndShowNextFrame(worldPosition));
    }

    public void Hide()
    {
        highlight.gameObject.SetActive(false);
        panel.SetActive(false);
    }

    private void SetDefaultHighlight()
    {
        foreach (Button button in actionButtons)
        {
            if (button.gameObject.activeSelf)
            {
                SetHighlight(button);
                return;
            }
        }

        highlight.gameObject.SetActive(false);
    }

    private IEnumerator RebuildAndShowNextFrame(Vector3 worldPosition)
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(actionPanelRect);
        Canvas.ForceUpdateCanvases();

        ResizeBackgroundToButtons();

        yield return null;

        SetPosition(worldPosition);
        SetDefaultHighlight();
    }

    public void SetPosition(Vector3 worldPosition)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        screenPos.y += 70f;

        panelRect.position = screenPos;

        ClampToScreen();
    }

    private void ResizeBackgroundToButtons()
    {
        float paddingX = 36f;
        float paddingY = 72f;

        float width = LayoutUtility.GetPreferredWidth(actionPanelRect);
        float height = LayoutUtility.GetPreferredHeight(actionPanelRect);

        panelRect.sizeDelta = new Vector2(width + paddingX, height + paddingY);
    }

    private void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        panelRect.GetWorldCorners(corners);

        float minX = corners[0].x;
        float minY = corners[0].y;
        float maxX = corners[2].x;
        float maxY = corners[2].y;

        Vector3 position = panelRect.position;

        if (minX < 0)
        {
            position.x += -minX;
        }

        if (maxX > Screen.width)
        {
            position.x -= maxX - Screen.width;
        }

        if (minY < 0)
        {
            position.y += -minY;
        }

        if (maxY > Screen.height)
        {
            position.y -= maxY - Screen.height;
        }

        panelRect.position = position;
    }

    public void SetHighlight(Button button)
    {
        if (button == null || !button.gameObject.activeSelf)
        {
            highlight.gameObject.SetActive(false);
            return;
        }

        highlight.gameObject.SetActive(true);

        RectTransform target = button.GetComponent<RectTransform>();

        highlight.position = target.position;
        highlight.sizeDelta = target.sizeDelta;
    }
}