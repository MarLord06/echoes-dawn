using System.Collections.Generic;
using UnityEngine;

public static class CombatTargetResolver
{
    public static IReadOnlyList<Collider2D> GetDistinctDamageableColliders(IEnumerable<Collider2D> colliders)
    {
        var result = new List<Collider2D>();
        var seen = new HashSet<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            if (collider != null && !collider.isTrigger && seen.Add(collider))
            {
                result.Add(collider);
            }
        }

        return result;
    }
}
