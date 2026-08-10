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

    [Test]
    public void Allocate_ReturnsAllItemsWhenStackCapacityIsNotPositive()
    {
        var slots = new List<InventoryStackAllocation>
        {
            new InventoryStackAllocation(isCompatible: true, isEmpty: false, quantity: 1)
        };

        int remainder = InventoryStackAllocator.Allocate(4, 0, slots);

        Assert.That(remainder, Is.EqualTo(4));
        Assert.That(slots[0].Quantity, Is.EqualTo(1));
    }
}
