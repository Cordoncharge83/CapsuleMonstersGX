using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CapsuleManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private List<Unit> capsuleUnitPrefabs;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GridManager gridManager;

    private int currentCapsuleIndex = 0;

    public bool HasCapsulesLeft()
    {
        return currentCapsuleIndex < capsuleUnitPrefabs.Count;
    }

    public void PlaceNextCapsule(Vector3Int cellPosition)
    {
        if (!HasCapsulesLeft())
        {
            Debug.Log("No capsules left.");
            return;
        }

        Unit unitPrefab = capsuleUnitPrefabs[currentCapsuleIndex];

        Unit spawnedUnit = Instantiate(unitPrefab);
        spawnedUnit.SetCombatTilemap(combatTilemap);
        spawnedUnit.SnapToCell(cellPosition);

        gridManager.AddPlayerUnit(spawnedUnit);
        enemyAI.AddPlayerUnit(spawnedUnit);

        currentCapsuleIndex++;

        Debug.Log($"Placed capsule unit: {spawnedUnit.name}");
    }
}