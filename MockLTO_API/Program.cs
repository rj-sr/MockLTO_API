using MockLTO_API.Configuration;
using MockLTO_API.Data;
using MockLTO_API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IApplicationConfig, ApplicationConfig>();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<ISqlStoredProcedureExecutor, SqlStoredProcedureExecutor>();
builder.Services.AddScoped<ISqlQueryExecutor, SqlQueryExecutor>();
builder.Services.AddScoped<ILtoVehicleRecordRepository, LtoVehicleRecordRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
