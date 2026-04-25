using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySetupManager : MonoBehaviour
{
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private List<EnemySpawnData> enemySpawns;
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
            Unit spawnedEnemy = Instantiate(spawnData.enemyPrefab);

            spawnedEnemy.SetCombatTilemap(combatTilemap);
            spawnedEnemy.SnapToCell(spawnData.spawnCell);

            gridManager.AddEnemyUnit(spawnedEnemy);
            enemyAI.AddEnemyUnit(spawnedEnemy);
        }
    }
}