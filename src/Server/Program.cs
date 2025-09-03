using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000", "http://0.0.0.0:5001");


// ------------------------------------------------------------
// 1. Configuration & DI
// ------------------------------------------------------------

// Swagger, MVC
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Services applicatifs
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<DatabaseService>();

// Tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantConnectionFactory, TenantConnectionFactory>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantCredentialsStore, TenantCredentialsStore>();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer      = jwt["Issuer"],
        ValidAudience    = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
});

// ------------------------------------------------------------
// 2. Build & middleware pipeline
// ------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();   // JWT
app.UseAuthorization();    // [Authorize]

// ------------------------------------------------------------
// 3. Endpoints
// ------------------------------------------------------------
app.MapControllers();

// Exemple d’endpoint minimal (optionnel)
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ));
    return forecast.ToArray();
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

// Petit record DTO (se trouve souvent ailleurs, mais gardé ici pour l’exemple)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
