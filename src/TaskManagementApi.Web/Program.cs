using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure.Persistence;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Infrastructure.Repositories;
using TaskManagementApi.Infrastructure.Services;
using TaskManagementApi.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        var origins = builder.Configuration
            .GetSection("App:Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
        var methods = builder.Configuration
            .GetSection("App:Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        policy.WithOrigins(origins)
              .WithMethods(methods)
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("TaskManagementApi.Infrastructure")));

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 2. Keycloak JWT Bearer validation
var keycloakUrl = builder.Configuration["Keycloak:AuthServerUrl"];
var realm = builder.Configuration["Keycloak:Realm"];
var clientId = builder.Configuration["Keycloak:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakUrl}/realms/{realm}";
        options.Audience = clientId;
        options.RequireHttpsMetadata = false; // set true in production
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

// 3. Keycloak role converter
builder.Services.AddSingleton<IClaimsTransformation, KeycloakJwtRoleConverter>();

// 4. Register KeycloakAuthService with HttpClient
builder.Services.AddHttpClient<IKeycloakAuthService, KeycloakAuthService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISubProjectService, SubProjectService>();

builder.Services.AddValidatorsFromAssemblyContaining<IProjectService>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- app pipeline ---
app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
