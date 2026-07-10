using System.Reflection;
using System.Text.Json.Serialization;
using InvestmentSimulator.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Investment Simulator API",
        Version = "v1",
        Description = "API REST do Simulador de Investimentos (CDB e Tesouro Selic).",
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddInvestmentSimulator();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Investment Simulator API v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

/// <summary>Exposes the entry point for integration tests via WebApplicationFactory.</summary>
public partial class Program;
