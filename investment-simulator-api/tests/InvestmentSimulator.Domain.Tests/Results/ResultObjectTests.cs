using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Domain.Tests.Results;

public class ContributionDetailTests
{
    [Fact]
    public void Constructor_ShouldAssignPerContributionFieldsFromErsSections9And20()
    {
        var date = new DateOnly(2026, 1, 1);
        const decimal amount = 10_000m;
        const decimal balance = 11_250.50m;
        const decimal yield = 1_250.50m;
        const int daysInvested = 365;
        const decimal incomeTax = 187.575m;
        const decimal iof = 0m;

        var detail = new ContributionDetail(
            date,
            amount,
            balance,
            yield,
            daysInvested,
            incomeTax,
            iof);

        Assert.Equal(date, detail.Date);
        Assert.Equal(amount, detail.Amount);
        Assert.Equal(balance, detail.Balance);
        Assert.Equal(yield, detail.Yield);
        Assert.Equal(daysInvested, detail.DaysInvested);
        Assert.Equal(incomeTax, detail.IncomeTax);
        Assert.Equal(iof, detail.Iof);
    }
}

public class SimulationResultTests
{
    [Fact]
    public void Constructor_ShouldAssignFinalSummaryFromErsSection19()
    {
        var contributionDetails = new List<ContributionDetail>
        {
            new(
                date: new DateOnly(2026, 1, 1),
                amount: 10_000m,
                balance: 11_000m,
                yield: 1_000m,
                daysInvested: 365,
                incomeTax: 150m,
                iof: 0m),
            new(
                date: new DateOnly(2026, 2, 1),
                amount: 900m,
                balance: 980m,
                yield: 80m,
                daysInvested: 334,
                incomeTax: 16m,
                iof: 0m),
        };

        var result = new SimulationResult(
            initialAmount: 10_000m,
            contributionsAmount: 900m,
            totalInvested: 10_900m,
            grossAmount: 11_980m,
            grossReturn: 0.0990825688m,
            costs: 12.50m,
            incomeTax: 166m,
            iof: 0m,
            netAmount: 11_801.50m,
            netReturn: 0.0827064220m,
            netProfit: 901.50m,
            inflationAdjustedAmount: 11_239.52m,
            contributionDetails: contributionDetails);

        Assert.Equal(10_000m, result.InitialAmount);
        Assert.Equal(900m, result.ContributionsAmount);
        Assert.Equal(10_900m, result.TotalInvested);
        Assert.Equal(11_980m, result.GrossAmount);
        Assert.Equal(0.0990825688m, result.GrossReturn);
        Assert.Equal(12.50m, result.Costs);
        Assert.Equal(166m, result.IncomeTax);
        Assert.Equal(0m, result.Iof);
        Assert.Equal(11_801.50m, result.NetAmount);
        Assert.Equal(0.0827064220m, result.NetReturn);
        Assert.Equal(901.50m, result.NetProfit);
        Assert.Equal(11_239.52m, result.InflationAdjustedAmount);
        Assert.Equal(2, result.ContributionDetails.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), result.ContributionDetails[0].Date);
        Assert.Equal(900m, result.ContributionDetails[1].Amount);
    }

    [Fact]
    public void Constructor_ShouldSupportEmptyContributionDetails()
    {
        var result = new SimulationResult(
            initialAmount: 5_000m,
            contributionsAmount: 0m,
            totalInvested: 5_000m,
            grossAmount: 5_000m,
            grossReturn: 0m,
            costs: 0m,
            incomeTax: 0m,
            iof: 0m,
            netAmount: 5_000m,
            netReturn: 0m,
            netProfit: 0m,
            inflationAdjustedAmount: 5_000m,
            contributionDetails: []);

        Assert.Empty(result.ContributionDetails);
        Assert.Equal(0m, result.ContributionsAmount);
    }
}
