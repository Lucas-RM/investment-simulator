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
        const int calendarDaysInvested = 365;
        const int businessDaysInvested = 252;
        const decimal incomeTax = 187.575m;
        const decimal iof = 0m;

        var detail = new ContributionDetail(
            date,
            amount,
            balance,
            yield,
            calendarDaysInvested,
            businessDaysInvested,
            incomeTax,
            iof);

        Assert.Equal(date, detail.Date);
        Assert.Equal(amount, detail.Amount);
        Assert.Equal(balance, detail.GrossBalance);
        Assert.Equal(yield, detail.GrossYield);
        Assert.Equal(calendarDaysInvested, detail.CalendarDaysInvested);
        Assert.Equal(businessDaysInvested, detail.BusinessDaysInvested);
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
                grossBalance: 11_000m,
                grossYield: 1_000m,
                calendarDaysInvested: 365,
                businessDaysInvested: 252,
                incomeTax: 150m,
                iof: 0m),
            new(
                date: new DateOnly(2026, 2, 1),
                amount: 900m,
                grossBalance: 980m,
                grossYield: 80m,
                calendarDaysInvested: 334,
                businessDaysInvested: 230,
                incomeTax: 16m,
                iof: 0m),
        };

        var result = new SimulationResult(
            startDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2027, 1, 1),
            initialAmount: 10_000m,
            totalAdditionalContributions: 900m,
            totalInvested: 10_900m,
            grossAmount: 11_980m,
            grossReturnPercentage: 0.0990825688m,
            totalGrossYield: 1_080m,
            costs: 12.50m,
            incomeTax: 166m,
            iof: 0m,
            netAmount: 11_801.50m,
            netReturnPercentage: 0.0827064220m,
            totalNetYield: 901.50m,
            netAmountInflationAdjusted: 11_239.52m,
            contributionDetails: contributionDetails);

        Assert.Equal(new DateOnly(2026, 1, 1), result.StartDate);
        Assert.Equal(new DateOnly(2027, 1, 1), result.EndDate);
        Assert.Equal(10_000m, result.InitialAmount);
        Assert.Equal(900m, result.TotalAdditionalContributions);
        Assert.Equal(10_900m, result.TotalInvested);
        Assert.Equal(11_980m, result.GrossAmount);
        Assert.Equal(0.0990825688m, result.GrossReturnPercentage);
        Assert.Equal(1_080m, result.TotalGrossYield);
        Assert.Equal(12.50m, result.Costs);
        Assert.Equal(166m, result.IncomeTax);
        Assert.Equal(0m, result.Iof);
        Assert.Equal(11_801.50m, result.NetAmount);
        Assert.Equal(0.0827064220m, result.NetReturnPercentage);
        Assert.Equal(901.50m, result.TotalNetYield);
        Assert.Equal(11_239.52m, result.NetAmountInflationAdjusted);
        Assert.Equal(2, result.ContributionDetails.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), result.ContributionDetails[0].Date);
        Assert.Equal(900m, result.ContributionDetails[1].Amount);
    }

    [Fact]
    public void Constructor_ShouldSupportEmptyContributionDetails()
    {
        var result = new SimulationResult(
            startDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2026, 6, 1),
            initialAmount: 5_000m,
            totalAdditionalContributions: 0m,
            totalInvested: 5_000m,
            grossAmount: 5_000m,
            grossReturnPercentage: 0m,
            totalGrossYield: 0m,
            costs: 0m,
            incomeTax: 0m,
            iof: 0m,
            netAmount: 5_000m,
            netReturnPercentage: 0m,
            totalNetYield: 0m,
            netAmountInflationAdjusted: 5_000m,
            contributionDetails: []);

        Assert.Empty(result.ContributionDetails);
        Assert.Equal(0m, result.TotalAdditionalContributions);
    }
}
