using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TesteFeriados.Api.BackgroundServices;
using TesteFeriados.Api.Middleware;
using TesteFeriados.Infrastructure;
using TesteFeriados.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FeriadosDb")
    ?? throw new InvalidOperationException("Connection string 'FeriadosDb' não configurada.");

var feriadosApiBaseUrl = builder.Configuration["FeriadosApi:BaseUrl"]
    ?? throw new InvalidOperationException("Configuração 'FeriadosApi:BaseUrl' não definida.");

builder.Services.AddInfrastructure(connectionString, feriadosApiBaseUrl);

builder.Services.AddHostedService<FeriadosSyncBackgroundService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TesteFeriados API",
        Version = "v1",
        Description = "API de gestão de feriados nacionais, sincronizados periodicamente "
            + "a partir da fonte pública (dadosbr.github.io) e expostos via CRUD."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FeriadosDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
