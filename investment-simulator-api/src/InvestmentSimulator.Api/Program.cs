using System.Text.Json.Serialization;
using InvestmentSimulator.Api;
using InvestmentSimulator.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddInvestmentSimulator();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapSimulationEndpoints();
app.MapComparisonEndpoints();
app.MapExportEndpoints();
app.MapHistoryEndpoints();

app.Run();

/// <summary>Exposes the entry point for integration tests via WebApplicationFactory.</summary>
public partial class Program;
