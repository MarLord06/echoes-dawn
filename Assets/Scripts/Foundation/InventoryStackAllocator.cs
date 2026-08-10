using System;
using System.Collections.Generic;

public static class InventoryStackAllocator
{
    public static int Allocate(int quantity, int stackCapacity, IList<InventoryStackAllocation> allocations)
    {
        if (quantity <= 0)
        {
            return 0;
        }

        if (stackCapacity <= 0 || allocations == null)
        {
            return quantity;
        }

        foreach (InventoryStackAllocation allocation in allocations)
        {
            if (allocation != null && allocation.IsCompatible)
            {
                quantity = Fill(allocation, stackCapacity, quantity);
            }
        }

        foreach (InventoryStackAllocation allocation in allocations)
        {
            if (allocation != null && allocation.IsEmpty && quantity > 0)
            {
                quantity = Fill(allocation, stackCapacity, quantity);
            }
        }

        return quantity;
    }

    private static int Fill(InventoryStackAllocation allocation, int stackCapacity, int quantity)
    {
        int availableSpace = Math.Max(0, stackCapacity - allocation.Quantity);
        int amountToAdd = Math.Min(availableSpace, quantity);
        allocation.Quantity += amountToAdd;
        return quantity - amountToAdd;
    }
}
