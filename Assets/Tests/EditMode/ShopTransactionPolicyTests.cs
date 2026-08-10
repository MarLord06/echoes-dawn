using NUnit.Framework;

public class ShopTransactionPolicyTests
{
    [Test]
    public void CanBuy_RequiresEnoughGoldAndSpace()
    {
        Assert.That(ShopTransactionPolicy.CanBuy(5, 5, true), Is.True);
        Assert.That(ShopTransactionPolicy.CanBuy(4, 5, true), Is.False);
        Assert.That(ShopTransactionPolicy.CanBuy(5, 5, false), Is.False);
        Assert.That(ShopTransactionPolicy.CanBuy(5, -1, true), Is.False);
    }

    [Test]
    public void CanSell_RequiresTheItemToBeListed()
    {
        Assert.That(ShopTransactionPolicy.CanSell(true), Is.True);
        Assert.That(ShopTransactionPolicy.CanSell(false), Is.False);
    }
}
