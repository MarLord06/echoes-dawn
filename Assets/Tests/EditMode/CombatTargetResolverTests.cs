using NUnit.Framework;
using UnityEngine;

public class CombatTargetResolverTests
{
    [Test]
    public void GetDistinctDamageableColliders_ReturnsOneEntryForRepeatedCollider()
    {
        GameObject target = new GameObject("Target");
        Collider2D collider = target.AddComponent<BoxCollider2D>();

        var result = CombatTargetResolver.GetDistinctDamageableColliders(new[] { collider, collider });

        Assert.That(result, Has.Count.EqualTo(1));
        Object.DestroyImmediate(target);
    }

    [Test]
    public void GetDistinctDamageableColliders_ExcludesTriggersAndNulls()
    {
        GameObject triggerObject = new GameObject("Trigger");
        Collider2D trigger = triggerObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;

        var result = CombatTargetResolver.GetDistinctDamageableColliders(new Collider2D[] { null, trigger });

        Assert.That(result, Is.Empty);
        Object.DestroyImmediate(triggerObject);
    }
}
