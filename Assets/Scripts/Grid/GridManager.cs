using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectClickedCell();
        }
    }

    private void DetectClickedCell()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = groundTilemap.WorldToCell(mouseWorldPosition);

        Debug.Log($"Clicked cell: {cellPosition}");
    }
}
