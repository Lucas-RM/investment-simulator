using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Domain.Tests.Enums;

public class InvestmentTypeTests
{
    [Fact]
    public void InvestmentType_ShouldExposeCdbAndTesouroSelic()
    {
        Assert.Equal(1, (int)InvestmentType.Cdb);
        Assert.Equal(2, (int)InvestmentType.TesouroSelic);
        Assert.Equal(2, Enum.GetValues<InvestmentType>().Length);
    }
}
