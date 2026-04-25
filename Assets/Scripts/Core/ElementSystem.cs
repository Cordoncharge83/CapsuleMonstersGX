using UnityEngine;

public static class ElementSystem
{
    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        // Fire > Wind
        if (attacker == ElementType.Fire && defender == ElementType.Wind)
            return 1.5f;

        // Wind > Earth
        if (attacker == ElementType.Wind && defender == ElementType.Earth)
            return 1.5f;

        // Earth > Water
        if (attacker == ElementType.Earth && defender == ElementType.Water)
            return 1.5f;

        // Water > Fire
        if (attacker == ElementType.Water && defender == ElementType.Fire)
            return 1.5f;

        // Light > Dark
        if (attacker == ElementType.Light && defender == ElementType.Dark)
            return 1.5f;

        // Dark > Light
        if (attacker == ElementType.Dark && defender == ElementType.Light)
            return 1.5f;

        // Weak cases (reverse)
        if (attacker == ElementType.Wind && defender == ElementType.Fire)
            return 0.5f;

        if (attacker == ElementType.Earth && defender == ElementType.Wind)
            return 0.5f;

        if (attacker == ElementType.Water && defender == ElementType.Earth)
            return 0.5f;

        if (attacker == ElementType.Fire && defender == ElementType.Water)
            return 0.5f;

        if (attacker == ElementType.Dark && defender == ElementType.Light)
            return 0.5f;

        if (attacker == ElementType.Light && defender == ElementType.Dark)
            return 0.5f;

        return 1f;
    }
}