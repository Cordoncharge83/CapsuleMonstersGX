using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySetupManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private List<EnemySpawnData> enemySpawns;
    [SerializeField] private Capsule capsulePrefab;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private EnemyAI enemyAI;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        foreach (EnemySpawnData spawnData in enemySpawns)
        {
            Capsule spawnedCapsule = Instantiate(capsulePrefab);

            spawnedCapsule.Initialize(
                spawnData.enemyPrefab,
                combatTilemap,
                spawnData.spawnCell,
                Unit.Team.Enemy
            );

            enemyAI.AddEnemyCapsule(spawnedCapsule);
        }
    }
}