using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CapsuleManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private List<Unit> capsuleUnitPrefabs;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Capsule capsulePrefab;

    private int currentCapsuleIndex = 0;

    public bool HasCapsulesLeft()
    {
        return currentCapsuleIndex < capsuleUnitPrefabs.Count;
    }

    public Capsule PlaceNextCapsule(Vector3Int cellPosition)
    {
        if (!HasCapsulesLeft())
        {
            Debug.Log("No capsules left.");
            return null;
        }

        Unit unitPrefab = capsuleUnitPrefabs[currentCapsuleIndex];

        Capsule spawnedCapsule = Instantiate(capsulePrefab);
        spawnedCapsule.Initialize(unitPrefab, combatTilemap, cellPosition, Unit.Team.Player);

        currentCapsuleIndex++;

        Debug.Log($"Placed capsule containing: {unitPrefab.name}");

        return spawnedCapsule;
    }
}