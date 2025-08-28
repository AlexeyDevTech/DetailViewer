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


var dbPath = builder.Configuration["DB_PATH"];
if (string.IsNullOrEmpty(dbPath))
{
    dbPath = "data.db";
    Console.WriteLine("DB_PATH environment variable not set. Using default 'data.db'");
}

var connectionString = $"Data Source={dbPath}";

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'RemoteDatabase' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlite(connectionString).AddInterceptors(new SqliteWalInterceptor());
    //here...
    });

var app = builder.Build();

// �������� ���� ������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // ������� ���� ������, ���� ��� �� ����������
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