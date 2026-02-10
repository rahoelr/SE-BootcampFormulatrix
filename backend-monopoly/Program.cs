using MonopolyBackend.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:3000",
            "http://localhost:5173"
        };

        var configOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
        if (configOrigins != null && configOrigins.Length > 0)
        {
            allowedOrigins.AddRange(configOrigins);
        }

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
