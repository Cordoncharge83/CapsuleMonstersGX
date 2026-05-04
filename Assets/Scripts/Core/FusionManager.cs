using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FusionManager : MonoBehaviour
{
    [SerializeField] private List<FusionRecipe> fusionRecipes;
    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private ScreenFlashUI screenFlashUI;

    public IEnumerator TryFusionSequence(Unit a, Unit b, System.Action<Unit> onFusionComplete)
    {
        FusionRecipe matchedRecipe = GetMatchingRecipe(a, b);

        if (matchedRecipe == null)
        {
            Debug.Log("No valid fusion.");
            onFusionComplete?.Invoke(null);
            yield break;
        }

        if (screenFlashUI != null)
        {
            yield return StartCoroutine(screenFlashUI.Flash());
        }

        Unit fusedUnit = PerformFusion(matchedRecipe, a, b);

        onFusionComplete?.Invoke(fusedUnit);

        yield return StartCoroutine(GlowUnit(fusedUnit, 1f));

        
    }

    public Unit GetFusionResultPrefab(Unit a, Unit b)
    {
        FusionRecipe matchedRecipe = GetMatchingRecipe(a, b);

        if (matchedRecipe == null)
        {
            return null;
        }

        return matchedRecipe.resultPrefab;
    }

    public bool CanFuse(Unit a, Unit b)
    {
        return GetMatchingRecipe(a, b) != null;
    }

    private FusionRecipe GetMatchingRecipe(Unit a, Unit b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        if (!AreAdjacent(a, b))
        {
            return null;
        }

        foreach (FusionRecipe recipe in fusionRecipes)
        {
            if (IsMatch(recipe, a, b))
            {
                return recipe;
            }
        }

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

    private IEnumerator GlowUnit(Unit unit, float duration)
    {
        SpriteRenderer renderer = unit.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            yield break;
        }

        Color originalColor = renderer.color;
        Color glowColor = new Color(1f, 0.85f, 0.25f);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float pulse = Mathf.PingPong(timer * 4f, 1f);
            renderer.color = Color.Lerp(originalColor, glowColor, pulse);

            yield return null;
        }

        renderer.color = originalColor;
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