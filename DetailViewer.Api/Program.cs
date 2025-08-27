using DetailViewer.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app.log"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

var connectionString = builder.Configuration.GetConnectionString("RemoteDatabase");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'RemoteDatabase' not found.");
}

var dataSource = connectionString.Replace("Data Source=", "");
if (!Path.IsPathRooted(dataSource))
{
    dataSource = Path.Combine(AppContext.BaseDirectory, dataSource);
}
connectionString = $"Data Source={dataSource}";

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlite(connectionString).AddInterceptors(new SqliteWalInterceptor());
    //here...
    });

var app = builder.Build();

// Создание базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Создает базу данных, если она не существует
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();