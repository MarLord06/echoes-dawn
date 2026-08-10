public sealed class InventoryStackAllocation
{
    public bool IsCompatible { get; }
    public bool IsEmpty { get; }
    public int Quantity { get; set; }

    public InventoryStackAllocation(bool isCompatible, bool isEmpty, int quantity)
    {
        IsCompatible = isCompatible;
        IsEmpty = isEmpty;
        Quantity = quantity;
    }
}
