using UnityEngine;
using UnityEngine.UI;

public class ActionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button finishButton;

    [SerializeField] private GridManager gridManager;

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

        cancelButton.onClick.AddListener(gridManager.CancelAction);

        Hide();
    }

    public void Show(bool canMove, bool canAttack, bool canFuse, bool canFinish, Vector3 worldPosition)
    {
        panel.SetActive(true);

        moveButton.gameObject.SetActive(canMove);
        attackButton.gameObject.SetActive(canAttack);
        fuseButton.gameObject.SetActive(canFuse);
        finishButton.gameObject.SetActive(canFinish);
        cancelButton.gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        SetPosition(worldPosition);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void SetPosition(Vector3 worldPosition)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        screenPos.y += 70f;

        panelRect.position = screenPos;

        ClampToScreen();
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
}