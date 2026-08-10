using NUnit.Framework;

public class FoundationSmokeTests
{
    [Test]
    public void FoundationAssemblyLoads()
    {
        Assert.That(typeof(CombatTargetResolver), Is.Not.Null);
    }
}
