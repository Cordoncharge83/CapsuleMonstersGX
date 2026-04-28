using System.Collections.Generic;
using UnityEngine;

public static class GridPatternUtility
{
    public static List<Vector3Int> GetCellsInPattern(
        Vector3Int origin,
        int range,
        GridPatternType patternType
    )
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                Vector3Int offset = new Vector3Int(x, y, 0);

                if (IsOffsetInPattern(offset, range, patternType))
                {
                    cells.Add(origin + offset);
                }
            }
        }

        return cells;
    }

    public static bool IsCellInPattern(
        Vector3Int origin,
        Vector3Int target,
        int range,
        GridPatternType patternType
    )
    {
        Vector3Int offset = target - origin;
        return IsOffsetInPattern(offset, range, patternType);
    }

    private static bool IsOffsetInPattern(
        Vector3Int offset,
        int range,
        GridPatternType patternType
    )
    {
        int distanceX = Mathf.Abs(offset.x);
        int distanceY = Mathf.Abs(offset.y);

        switch (patternType)
        {
            case GridPatternType.Diamond:
                return distanceX + distanceY <= range;

            case GridPatternType.Cross:
                return distanceX + distanceY <= range &&
                       (offset.x == 0 || offset.y == 0);

            case GridPatternType.Diagonal:
                return distanceX == distanceY &&
                       distanceX <= range;

            default:
                return false;
        }
    }
}