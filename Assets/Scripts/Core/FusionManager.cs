using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FusionManager : MonoBehaviour
{
    [SerializeField] private List<FusionRecipe> fusionRecipes;
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private EnemyAI enemyAI;

    public Unit TryFusion(Unit a, Unit b)
    {
        if (!AreAdjacent(a, b))
        {
            Debug.Log("Units must be adjacent to fuse.");
            return null;
        }

        foreach (var recipe in fusionRecipes)
        {
            if (IsMatch(recipe, a, b))
            {
                return PerformFusion(recipe, a, b);
            }
        }

        Debug.Log("No valid fusion");
        return null;
    }

    private bool IsMatch(FusionRecipe recipe, Unit a, Unit b)
    {
        return (a.UnitId == recipe.unitAId && b.UnitId == recipe.unitBId) ||
               (a.UnitId == recipe.unitBId && b.UnitId == recipe.unitAId);
    }

    private Unit PerformFusion(FusionRecipe recipe, Unit a, Unit b)
    {
        Vector3Int spawnCell = a.GetCurrentCellPosition();

        gridManager.RemovePlayerUnit(a);
        gridManager.RemovePlayerUnit(b);

        enemyAI.RemovePlayerUnit(a);
        enemyAI.RemovePlayerUnit(b);

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        Unit fusedUnit = Instantiate(recipe.resultPrefab);

        fusedUnit.SetCombatTilemap(combatTilemap);
        fusedUnit.SnapToCell(spawnCell);

        gridManager.AddPlayerUnit(fusedUnit);
        enemyAI.AddPlayerUnit(fusedUnit);

        Debug.Log("Fusion successful: " + fusedUnit.name);

        return fusedUnit;
    }

    private bool AreAdjacent(Unit a, Unit b)
    {
        Vector3Int aCell = a.GetCurrentCellPosition();
        Vector3Int bCell = b.GetCurrentCellPosition();

        int distance =
            Mathf.Abs(aCell.x - bCell.x) +
            Mathf.Abs(aCell.y - bCell.y);

        return distance == 1;
    }
}