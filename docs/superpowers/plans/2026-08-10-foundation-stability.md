# Foundation Stability Sprint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make combat targeting, inventory stacking, shop persistence notifications, and inventory-slot event cleanup deterministic and regression-tested.

**Architecture:** Add one small `Foundation` assembly for deterministic gameplay rules and an EditMode test assembly that references it. Existing MonoBehaviours retain ownership of Unity physics, UI, audio, instantiation, and save notifications; they call the pure rules and apply their results.

**Tech Stack:** Unity 6.1.1f1, C#, Unity Test Framework 1.5.1, NUnit, Git LFS.

## Global Constraints

- Preserve `GameManager.persistentObjects`; it is out of scope.
- Do not add content, dependencies, scene edits, Firebase changes, input migration, or cross-scene loot persistence.
- Use test-first red-green-refactor for every new rule.
- Keep generated folders and `TestResults/` out of Git.
- Push every completed commit to `origin/main`.

---

## File Structure

- Create: `Assets/Scripts/Foundation/Foundation.asmdef` — production assembly for pure rules.
- Create: `Assets/Scripts/Foundation/CombatTargetResolver.cs` — filters a physics-query result into distinct non-trigger colliders.
- Create: `Assets/Scripts/Foundation/InventoryStackAllocation.cs` and `InventoryStackAllocator.cs` — data and allocation rule for inventory quantities.
- Create: `Assets/Tests/EditMode/Foundation.EditMode.Tests.asmdef`, `CombatTargetResolverTests.cs`, and `InventoryStackAllocatorTests.cs`.
- Modify: `.gitignore`, `Player_Combat.cs`, `InventoryManager.cs`, `InventorySlot.cs`, and `ShopManager.cs`.

### Task 1: Establish the EditMode test boundary

**Files:**
- Create: `Assets/Scripts/Foundation/Foundation.asmdef`
- Create: `Assets/Scripts/Foundation/CombatTargetResolver.cs`
- Create: `Assets/Tests/EditMode/Foundation.EditMode.Tests.asmdef`
- Create: `Assets/Tests/EditMode/FoundationSmokeTests.cs`
- Modify: `.gitignore`

**Interfaces:**
- Produces assembly `Foundation` for Tasks 2 and 3.
- Produces test assembly `Foundation.EditMode.Tests` with `optionalUnityReferences: ["TestAssemblies"]`.

- [ ] **Step 1: Write the failing discovery test**

Create `Assets/Tests/EditMode/Foundation.EditMode.Tests.asmdef`:

```json
{
  "name": "Foundation.EditMode.Tests",
  "references": ["Foundation"],
  "includePlatforms": ["Editor"],
  "optionalUnityReferences": ["TestAssemblies"]
}
```

Create `FoundationSmokeTests.cs`:

```csharp
using NUnit.Framework;

public class FoundationSmokeTests
{
    [Test]
    public void FoundationAssemblyLoads()
    {
        Assert.That(typeof(CombatTargetResolver), Is.Not.Null);
    }
}
```

- [ ] **Step 2: Run the test and verify red**

Run:

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testResults TestResults/editmode.xml
```

Expected: compile failure because `CombatTargetResolver` and assembly `Foundation` do not exist.

- [ ] **Step 3: Add the minimal production boundary**

Create `Foundation.asmdef`:

```json
{
  "name": "Foundation",
  "rootNamespace": "EchoesOfDawn.Foundation"
}
```

Append `TestResults/` to `.gitignore`. Create the minimal type:

```csharp
public static class CombatTargetResolver
{
}
```

- [ ] **Step 4: Verify green**

Re-run the batch command. Expected: one passing EditMode test and local `TestResults/editmode.xml`.

- [ ] **Step 5: Commit**

```bash
git add .gitignore Assets/Scripts/Foundation/Foundation.asmdef Assets/Scripts/Foundation/CombatTargetResolver.cs Assets/Tests/EditMode
git commit -m "test: add foundation EditMode test boundary"
git push
```

### Task 2: Resolve each valid combat collider once

**Files:**
- Modify: `Assets/Scripts/Foundation/CombatTargetResolver.cs`
- Modify: `Assets/Scripts/PlayerScripts/Player_Combat.cs:39-55`
- Create: `Assets/Tests/EditMode/CombatTargetResolverTests.cs`

**Interfaces:**
- Produces `IReadOnlyList<Collider2D> CombatTargetResolver.GetDistinctDamageableColliders(IEnumerable<Collider2D> colliders)`.
- `Player_Combat.DealDamage()` applies one health and knockback operation for each returned collider.

- [ ] **Step 1: Write failing target-filtering tests**

```csharp
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
```

- [ ] **Step 2: Run tests and verify red**

Run the Task 1 batch command. Expected: compile failure because `GetDistinctDamageableColliders` is missing.

- [ ] **Step 3: Implement the minimal resolver and integration**

```csharp
using System.Collections.Generic;
using UnityEngine;

public static class CombatTargetResolver
{
    public static IReadOnlyList<Collider2D> GetDistinctDamageableColliders(IEnumerable<Collider2D> colliders)
    {
        var result = new List<Collider2D>();
        var seen = new HashSet<Collider2D>();

        foreach (Collider2D collider in colliders)
            if (collider != null && !collider.isTrigger && seen.Add(collider))
                result.Add(collider);

        return result;
    }
}
```

Update `Player_Combat.DealDamage` to iterate this resolver output. Use the loop collider, not `enemies[0]`; only call `ChangeHealth` and `Knockback` after `TryGetComponent` obtains both `Enemy_Health` and `Enemy_Knockback`.

- [ ] **Step 4: Verify green and perform the runtime smoke test**

Re-run the EditMode suite. In Unity Play Mode, place two enemies inside the attack circle; a completed attack must damage each one once and log no exception.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Foundation/CombatTargetResolver.cs Assets/Scripts/PlayerScripts/Player_Combat.cs Assets/Tests/EditMode/CombatTargetResolverTests.cs
git commit -m "fix: apply melee damage to each valid enemy"
git push
```

### Task 3: Allocate every incoming inventory unit

**Files:**
- Create: `Assets/Scripts/Foundation/InventoryStackAllocation.cs`
- Create: `Assets/Scripts/Foundation/InventoryStackAllocator.cs`
- Create: `Assets/Tests/EditMode/InventoryStackAllocatorTests.cs`
- Modify: `Assets/Scripts/Inventory & Shop/InventoryManager.cs:37-81`

**Interfaces:**
- `InventoryStackAllocation` exposes `bool IsCompatible`, `bool IsEmpty`, and mutable `int Quantity`.
- `int InventoryStackAllocator.Allocate(int quantity, int stackCapacity, IList<InventoryStackAllocation> allocations)` mutates quantities and returns the exact remainder.
- `InventoryManager.AddItem(ItemSO itemSO, int quantity)` maps allocation results back to slots and invokes `DropLoot(itemSO, remainder)` only for a positive remainder.

- [ ] **Step 1: Write failing allocation tests**

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class InventoryStackAllocatorTests
{
    [Test]
    public void Allocate_FillsCompatibleStacksBeforeEmptySlots()
    {
        var slots = new List<InventoryStackAllocation>
        {
            new InventoryStackAllocation(isCompatible: true, isEmpty: false, quantity: 2),
            new InventoryStackAllocation(isCompatible: false, isEmpty: true, quantity: 0),
            new InventoryStackAllocation(isCompatible: false, isEmpty: true, quantity: 0)
        };

        int remainder = InventoryStackAllocator.Allocate(8, 3, slots);

        Assert.That(remainder, Is.EqualTo(1));
        Assert.That(slots[0].Quantity, Is.EqualTo(3));
        Assert.That(slots[1].Quantity, Is.EqualTo(3));
        Assert.That(slots[2].Quantity, Is.EqualTo(3));
    }

    [Test]
    public void Allocate_ReturnsExactRemainderWhenInventoryIsFull()
    {
        var slots = new List<InventoryStackAllocation>
        {
            new InventoryStackAllocation(isCompatible: true, isEmpty: false, quantity: 3),
            new InventoryStackAllocation(isCompatible: false, isEmpty: false, quantity: 2)
        };

        int remainder = InventoryStackAllocator.Allocate(4, 3, slots);

        Assert.That(remainder, Is.EqualTo(4));
    }
}
```

- [ ] **Step 2: Run tests and verify red**

Run the Task 1 batch command. Expected: compilation failure because the allocation types are missing.

- [ ] **Step 3: Implement the minimal allocator and map it to slots**

`InventoryStackAllocation` constructor sets its three properties. Implement allocation in this exact order:

```csharp
foreach (var slot in allocations)
    if (slot.IsCompatible)
        quantity = Fill(slot, stackCapacity, quantity);

foreach (var slot in allocations)
    if (slot.IsEmpty && quantity > 0)
        quantity = Fill(slot, stackCapacity, quantity);

return quantity;
```

`Fill` adds `Mathf.Min(stackCapacity - slot.Quantity, quantity)` only when capacity and quantity are positive.

Refactor `InventoryManager.AddItem` to build an allocation in the same order as `itemSlots`, assign `itemSO` only to empty slots that received units, update every changed slot UI, and call `DropLoot` only with the returned remainder.

- [ ] **Step 4: Verify green and perform the runtime smoke test**

Re-run EditMode tests. In Play Mode, begin with one compatible 2/3 stack and two empty slots, pick up eight units, verify 3/3/3, and verify one world loot object contains the one-unit overflow. Then fill every slot, pick up four units, and verify one world loot object contains four.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Foundation/InventoryStackAllocation.cs Assets/Scripts/Foundation/InventoryStackAllocator.cs "Assets/Scripts/Inventory & Shop/InventoryManager.cs" Assets/Tests/EditMode/InventoryStackAllocatorTests.cs
git commit -m "fix: preserve overflow when adding inventory items"
git push
```

### Task 4: Make shop state and notifications transactional

**Files:**
- Create: `Assets/Scripts/Foundation/ShopTransactionPolicy.cs`
- Create: `Assets/Tests/EditMode/ShopTransactionPolicyTests.cs`
- Modify: `Assets/Scripts/Inventory & Shop/InventorySlot.cs:24-66`
- Modify: `Assets/Scripts/Inventory & Shop/Shop/ShopManager.cs:28-72`

**Interfaces:**
- Produces `bool ShopTransactionPolicy.CanBuy(int gold, int price, bool hasSpace)` and `bool ShopTransactionPolicy.CanSell(bool isListed)`.
- Produces `bool ShopManager.TryBuyItem(ItemSO itemSO, int price)` and `bool ShopManager.TrySellItem(ItemSO itemSO)`.
- `InventorySlot.OnPointerClick` decrements only after `TrySellItem(itemSO)` returns true.
- Each successful transaction invokes `GameManager.Instance.UpdateData()` once; failed transactions invoke it zero times.

- [ ] **Step 1: Write failing transaction-policy tests**

```csharp
using NUnit.Framework;

public class ShopTransactionPolicyTests
{
    [Test]
    public void CanBuy_RequiresEnoughGoldAndSpace()
    {
        Assert.That(ShopTransactionPolicy.CanBuy(5, 5, true), Is.True);
        Assert.That(ShopTransactionPolicy.CanBuy(4, 5, true), Is.False);
        Assert.That(ShopTransactionPolicy.CanBuy(5, 5, false), Is.False);
    }

    [Test]
    public void CanSell_RequiresTheItemToBeListed()
    {
        Assert.That(ShopTransactionPolicy.CanSell(true), Is.True);
        Assert.That(ShopTransactionPolicy.CanSell(false), Is.False);
    }
}
```

- [ ] **Step 2: Run EditMode tests and verify red**

Run the Task 1 batch command.

Expected: compilation failure because `ShopTransactionPolicy` does not exist.

- [ ] **Step 3: Implement minimal transaction results and cleanup**

Implement `ShopTransactionPolicy` as pure static methods:

```csharp
public static class ShopTransactionPolicy
{
    public static bool CanBuy(int gold, int price, bool hasSpace) => price >= 0 && gold >= price && hasSpace;
    public static bool CanSell(bool isListed) => isListed;
}
```

Change `TryBuyItem` to return `false` for a null item or `!ShopTransactionPolicy.CanBuy(inventoryManager.gold, price, HasSpaceForItem(itemSO))`. On success: deduct gold, update its text, call `InventoryManager.AddItem`, call `GameManager.Instance.UpdateData()` once, and return `true`.

Replace `SellItem` with `TrySellItem`: find whether the item is listed, call `ShopTransactionPolicy.CanSell(isListed)`, and return `false` when it is not. On success: add the sale gold, update its text, call `GameManager.Instance.UpdateData()` once, and return `true`.

In `InventorySlot.OnDisable`, replace `+=` with:

```csharp
ShopKeeper.OnShopStateChanged -= HandleShopStateChanged;
```

In left-click shop mode, decrement quantity and update UI only after a true return from `TrySellItem`.

- [ ] **Step 4: Verify green and perform transaction smoke checks**

Run the EditMode batch suite. In Unity Play Mode, make one purchase and sale, save/load, and verify gold and item quantities persist. Reopen shop and inventory repeatedly; verify one data-change notification per completed transaction and no duplicated callback symptoms or Console errors.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Foundation/ShopTransactionPolicy.cs Assets/Tests/EditMode/ShopTransactionPolicyTests.cs "Assets/Scripts/Inventory & Shop/InventorySlot.cs" "Assets/Scripts/Inventory & Shop/Shop/ShopManager.cs"
git commit -m "fix: persist successful shop transactions"
git push
```

## Final Verification

- [ ] Run the complete EditMode suite with the Task 1 batch command.
- [ ] Complete the four runtime smoke checks from Tasks 2–4 without Console exceptions.
- [ ] Confirm `git status --short` is clean.
- [ ] Confirm `git log --oneline -4` contains the four sprint commits and `git push` says `Everything up-to-date`.
