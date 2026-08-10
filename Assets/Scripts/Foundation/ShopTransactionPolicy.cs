public static class ShopTransactionPolicy
{
    public static bool CanBuy(int gold, int price, bool hasSpace)
    {
        return price >= 0 && gold >= price && hasSpace;
    }

    public static bool CanSell(bool isListed)
    {
        return isListed;
    }
}
