using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvestmentSimulator.Application.Export;
using InvestmentSimulator.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InvestmentSimulator.Api.Tests.Endpoints;

public class ApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public ApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostSimularCdb_ShouldReturnOkWithSummary()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 10_000m,
            initialContributionDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = new[]
            {
                new { date = "2026-01-06", amount = 1_000m },
            },
            annualRates = new[] { new { year = 2026, rate = 0.15m } },
            ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
            profitabilityPercentage = 1.10m,
            costs = 0m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal(10_000m, root.GetProperty("initialAmount").GetDecimal());
        Assert.Equal(1_000m, root.GetProperty("contributionsAmount").GetDecimal());
        Assert.Equal(11_000m, root.GetProperty("totalInvested").GetDecimal());
        Assert.True(root.GetProperty("grossAmount").GetDecimal() > 11_000m);
        Assert.Equal(2, root.GetProperty("contributionDetails").GetArrayLength());
    }

    [Fact]
    public async Task PostSimularCdb_WithInvalidAmount_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 0m,
            initialContributionDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            annualRates = new[] { new { year = 2026, rate = 0.15m } },
            ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
            profitabilityPercentage = 1.10m,
            costs = 0m,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostSimularTesouro_ShouldReturnOkWithSummary()
    {
        var response = await _client.PostAsJsonAsync("/simular/tesouro", new
        {
            initialAmount = 10_000m,
            initialContributionDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            annualRates = new[] { new { year = 2026, rate = 0.15m } },
            ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
            annualAgioRate = 0.001m,
            costs = 0m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal(10_000m, root.GetProperty("initialAmount").GetDecimal());
        Assert.Equal(10_000m, root.GetProperty("totalInvested").GetDecimal());
        Assert.True(root.GetProperty("grossAmount").GetDecimal() >= 10_000m);
        Assert.Equal(1, root.GetProperty("contributionDetails").GetArrayLength());
    }

    [Fact]
    public async Task PostComparar_ShouldReturnSideBySideMetrics()
    {
        var response = await _client.PostAsJsonAsync("/comparar", new
        {
            left = new
            {
                type = "Cdb",
                initialAmount = 10_000m,
                initialContributionDate = "2026-01-02",
                endDate = "2026-01-09",
                contributions = Array.Empty<object>(),
                annualRates = new[] { new { year = 2026, rate = 0.15m } },
                ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
                profitabilityPercentage = 1.10m,
                costs = 0m,
            },
            right = new
            {
                type = "TesouroSelic",
                initialAmount = 10_000m,
                initialContributionDate = "2026-01-02",
                endDate = "2026-01-09",
                contributions = Array.Empty<object>(),
                annualRates = new[] { new { year = 2026, rate = 0.15m } },
                ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
                annualAgioRate = 0.001m,
                costs = 0m,
            },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal("Cdb", root.GetProperty("left").GetProperty("type").GetString());
        Assert.Equal("TesouroSelic", root.GetProperty("right").GetProperty("type").GetString());
        Assert.True(root.TryGetProperty("netAmountDifference", out _));
        Assert.True(root.TryGetProperty("incomeTaxDifference", out _));
        Assert.True(root.TryGetProperty("costsDifference", out _));
        Assert.True(root.TryGetProperty("netProfitDifference", out _));
        Assert.True(root.TryGetProperty("netReturnDifference", out _));
        Assert.True(root.TryGetProperty("inflationAdjustedAmountDifference", out _));
    }

    [Fact]
    public async Task PostExportar_ShouldReturnCsvFile()
    {
        var simulateResponse = await _client.PostAsJsonAsync("/simular/cdb", CreateMinimalCdbBody());
        simulateResponse.EnsureSuccessStatusCode();

        var resultJson = await simulateResponse.Content.ReadAsStringAsync();

        var exportBody = new
        {
            format = ExportFormat.Csv,
            result = JsonSerializer.Deserialize<JsonElement>(resultJson),
        };

        var response = await _client.PostAsJsonAsync("/exportar", exportBody, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        Assert.Contains(
            "simulation-result.csv",
            response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? string.Empty);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
        Assert.Contains("Total Investido", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task Historico_SaveListAndGet_ShouldRoundTrip()
    {
        var saveResponse = await _client.PostAsJsonAsync("/historico", new
        {
            name = "CDB 110%",
            date = "2026-07-10",
            observations = "Cenário de teste",
            type = "Cdb",
            initialAmount = 10_000m,
            initialContributionDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            annualRates = new[] { new { year = 2026, rate = 0.15m } },
            ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
            profitabilityPercentage = 1.10m,
            costs = 0m,
        });

        Assert.Equal(HttpStatusCode.Created, saveResponse.StatusCode);

        using var savedDocument = await JsonDocument.ParseAsync(await saveResponse.Content.ReadAsStreamAsync());
        var saved = savedDocument.RootElement;
        var id = saved.GetProperty("id").GetGuid();

        Assert.Equal("CDB 110%", saved.GetProperty("name").GetString());
        Assert.Equal("Cdb", saved.GetProperty("type").GetString());
        Assert.Equal(InvestmentType.Cdb.ToString(), saved.GetProperty("simulation").GetProperty("type").GetString());

        var listResponse = await _client.GetAsync("/historico");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var listDocument = await JsonDocument.ParseAsync(await listResponse.Content.ReadAsStreamAsync());
        Assert.Contains(
            listDocument.RootElement.EnumerateArray(),
            e => e.GetProperty("id").GetGuid() == id);

        var getResponse = await _client.GetAsync($"/historico/{id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var getDocument = await JsonDocument.ParseAsync(await getResponse.Content.ReadAsStreamAsync());
        Assert.Equal(id, getDocument.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Cenário de teste", getDocument.RootElement.GetProperty("observations").GetString());
    }

    [Fact]
    public async Task GetHistorico_WhenMissing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/historico/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static object CreateMinimalCdbBody() => new
    {
        initialAmount = 10_000m,
        initialContributionDate = "2026-01-02",
        endDate = "2026-01-09",
        contributions = Array.Empty<object>(),
        annualRates = new[] { new { year = 2026, rate = 0.15m } },
        ipcaRates = new[] { new { year = 2026, rate = 0.05m } },
        profitabilityPercentage = 1.10m,
        costs = 0m,
    };
}
