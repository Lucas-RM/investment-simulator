using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvestmentSimulator.Application.Export;
using InvestmentSimulator.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InvestmentSimulator.Api.Tests.Controllers;

public class ApiControllersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public ApiControllersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSwaggerJson_ShouldReturnOpenApiDocument()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal("3.0.4", root.GetProperty("openapi").GetString());
        Assert.Equal("Investment Simulator API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.True(root.GetProperty("paths").TryGetProperty("/simular/cdb", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/simular/tesouro", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/comparar", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/exportar", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/historico", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/historico/{id}", out _));
    }

    [Fact]
    public async Task PostSimularCdb_ShouldReturnOkWithSummary()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 10_000m,
            startDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = new[]
            {
                new { date = "2026-01-06", amount = 1_000m },
            },
            cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
            ipcaRates = new[] { new { year = 2026, rate = 5m } },
            cdiPercentage = 1.10m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal(10_000m, root.GetProperty("initialAmount").GetDecimal());
        Assert.Equal(1_000m, root.GetProperty("totalAdditionalContributions").GetDecimal());
        Assert.Equal(11_000m, root.GetProperty("totalInvested").GetDecimal());
        Assert.True(root.GetProperty("grossAmount").GetDecimal() > 11_000m);
        Assert.Equal(2, root.GetProperty("contributionDetails").GetArrayLength());
    }

    [Fact]
    public async Task PostSimularCdb_WithPercentRates_ShouldProduceRealisticYield()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 900m,
            startDate = "2026-07-06",
            endDate = "2026-12-31",
            contributions = Array.Empty<object>(),
            cdiAnnualRates = new[] { new { year = 2026, rate = 14.15m } },
            ipcaRates = new[] { new { year = 2026, rate = 4.10m } },
            cdiPercentage = 1.20m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;
        var first = root.GetProperty("contributionDetails")[0];

        Assert.Equal(0m, root.GetProperty("costs").GetDecimal());
        // ~178 calendar days at ~17% a.a. effective → gross balance near R$970–R$990, not thousands.
        var grossBalance = first.GetProperty("grossBalance").GetDecimal();
        Assert.InRange(grossBalance, 950m, 1_050m);
        Assert.True(root.GetProperty("netAmountInflationAdjusted").GetDecimal() >
            root.GetProperty("netAmount").GetDecimal() * 0.9m);
        Assert.True(first.TryGetProperty("calendarDaysInvested", out _));
        Assert.True(first.TryGetProperty("businessDaysInvested", out _));
    }

    [Fact]
    public async Task PostSimularCdb_WithZeroInitialAmountAndContributions_ShouldReturnOk()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 0m,
            startDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = new[]
            {
                new { date = "2026-01-02", amount = 1_000m },
            },
            cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
            ipcaRates = new[] { new { year = 2026, rate = 5m } },
            cdiPercentage = 1.10m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal(0m, root.GetProperty("initialAmount").GetDecimal());
        Assert.Equal(1_000m, root.GetProperty("totalAdditionalContributions").GetDecimal());
        Assert.Equal(1, root.GetProperty("contributionDetails").GetArrayLength());
    }

    [Fact]
    public async Task PostSimularCdb_WithInvalidAmount_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/simular/cdb", new
        {
            initialAmount = 0m,
            startDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
            ipcaRates = new[] { new { year = 2026, rate = 5m } },
            cdiPercentage = 1.10m,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostSimularTesouro_ShouldReturnOkWithSummary()
    {
        var response = await _client.PostAsJsonAsync("/simular/tesouro", new
        {
            initialAmount = 10_000m,
            startDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            selicAnnualRates = new[] { new { year = 2026, rate = 15m } },
            ipcaRates = new[] { new { year = 2026, rate = 5m } },
            annualAgioRate = 0.001m,
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
                startDate = "2026-01-02",
                endDate = "2026-01-09",
                contributions = Array.Empty<object>(),
                cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
                ipcaRates = new[] { new { year = 2026, rate = 5m } },
                cdiPercentage = 1.10m,
            },
            right = new
            {
                type = "TesouroSelic",
                initialAmount = 10_000m,
                startDate = "2026-01-02",
                endDate = "2026-01-09",
                contributions = Array.Empty<object>(),
                selicAnnualRates = new[] { new { year = 2026, rate = 15m } },
                ipcaRates = new[] { new { year = 2026, rate = 5m } },
                annualAgioRate = 0.001m,
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
        Assert.True(root.TryGetProperty("totalNetYieldDifference", out _));
        Assert.True(root.TryGetProperty("netReturnPercentageDifference", out _));
        Assert.True(root.TryGetProperty("netAmountInflationAdjustedDifference", out _));
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
            startDate = "2026-01-02",
            endDate = "2026-01-09",
            contributions = Array.Empty<object>(),
            cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
            ipcaRates = new[] { new { year = 2026, rate = 5m } },
            cdiPercentage = 1.10m,
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
        startDate = "2026-01-02",
        endDate = "2026-01-09",
        contributions = Array.Empty<object>(),
        cdiAnnualRates = new[] { new { year = 2026, rate = 15m } },
        ipcaRates = new[] { new { year = 2026, rate = 5m } },
        cdiPercentage = 1.10m,
    };
}
