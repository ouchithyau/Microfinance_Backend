using Microfinance.Application;
using Microfinance.Infrastructure;
using Microfinance.Persistence;
using Microfinance.Persistence.Context;
using Microfinance.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers & API Explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer Authentication & Unique Schema IDs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Microfinance System API",
        Version = "v1",
        Description = "Clean Architecture Microfinance Backend API"
    });

    // Nested Generics (e.g. Result<List<UserDto>>) ගැටලුව වැළැක්වීමට
    c.CustomSchemaIds(BuildUniqueSchemaId);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token like: Bearer eyJhbGciOi...",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);

// JWT Authentication Configuration
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "MicrofinanceSystemSuperSecretKey2026@LongAndSecureSecretKey!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "MicrofinanceApi",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "MicrofinanceClients",
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Seed initial Database records
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await ApplicationDbContextSeeder.SeedInitialDataAsync(context);
}

// Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure Swagger Middleware
if (app.Environment.IsDevelopment())
{
    // SerializeAsV2 = true මගින් Swagger 2.0 format එකට force කරයි (Version parser errors වැළැක්වීමට)
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microfinance System API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Unique Schema ID Generator for Nested Generic Types
static string BuildUniqueSchemaId(Type type)
{
    if (!type.IsGenericType)
    {
        return type.Name;
    }

    var genericName = type.Name.Split('`')[0];
    var genericArgs = string.Join("", type.GetGenericArguments().Select(BuildUniqueSchemaId));
    return $"{genericName}Of{genericArgs}";
}