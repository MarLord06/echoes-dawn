# Foundation Stability Sprint — Design

## Goal

Remove four high-impact gameplay regressions without changing the current scene architecture or adding new game content.

## Scope

This sprint changes only the following behaviours:

1. A player attack damages each valid enemy in its hit area once, rather than repeatedly damaging the first collider.
2. Inventory slots unsubscribe from shop-state events when disabled.
3. Adding an item distributes every unit across compatible stacks and empty slots. Any remaining units are dropped into the world; none are silently lost.
4. Successful shop purchases and sales notify the game data system so the normal save flow receives the new gold/inventory state.

The `GameManager.persistentObjects` array, cross-scene dropped-loot persistence, Firebase readiness, scene routing, input migration, and new gameplay content are explicitly out of scope.

## Architecture

The existing Unity component structure remains in place. Targeted fixes stay in `Player_Combat`, `InventorySlot`, `InventoryManager`, and `ShopManager`.

For rules that need deterministic tests, small focused helpers will isolate pure logic from Unity scene references:

- `CombatTargetResolver` returns the valid components supplied by a hit query, once each.
- `InventoryStackAllocator` calculates how an incoming quantity fills existing stacks and empty slots, then reports the remaining quantity.

MonoBehaviours retain ownership of physics queries, UI updates, item dropping, audio, and save notifications. The helpers have no Unity object lookup, scene dependency, or side effect.

## Data Flow

```text
Attack animation event
  -> physics overlap query
  -> CombatTargetResolver filters duplicate/trigger/invalid targets
  -> each Enemy_Health receives one damage and knockback call

Loot or shop purchase
  -> InventoryStackAllocator assigns all units to slots
  -> InventoryManager updates each changed slot UI
  -> remaining units become a dropped Loot object
  -> GameManager.UpdateData()

Shop sale
  -> gold changes
  -> slot UI is refreshed by caller
  -> GameManager.UpdateData()
```

## Error Handling

- A hit collider without both required enemy components is ignored safely.
- Trigger colliders are ignored for melee damage.
- A null item, non-positive quantity, missing inventory slot, or invalid stack size leaves inventory state unchanged and is reported as an unallocated remainder rather than causing a silent item loss.
- Shop operations notify game data only after a successful transaction; failed purchases and invalid sales do not claim success.

## Testing

Create Unity EditMode tests for the pure helpers and the event subscription lifecycle where feasible:

- one valid enemy collider resolves once even if supplied twice;
- trigger and invalid colliders do not resolve as combat targets;
- incoming quantities fill multiple existing stacks, then empty slots;
- a full inventory reports the exact unallocated remainder;
- disabling an inventory slot removes, rather than adds, its shop-state subscription.

Run the focused EditMode tests and the full available EditMode suite before each commit. Runtime checks in Unity Play Mode will cover a multi-enemy swing, pickup into a near-full inventory, shop purchase, and shop sale.

## Acceptance Criteria

- Two enemies inside one swing each lose health exactly once.
- Re-enabling an inventory UI does not multiply shop-state callbacks.
- No item quantity disappears during stack filling.
- Shop purchases and sales result in a data-change notification exactly once per successful transaction.
- All new EditMode tests pass.
