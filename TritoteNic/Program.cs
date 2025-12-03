using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TritoteNic.Data;
using TritoteNic;
using TritoteNic.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada en appsettings.json");
}

builder.Services.AddDbContext<TritoteContext.TritoteConext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(MappingConfig));

// Services
builder.Services.AddHttpContextAccessor(); // Necesario para BitacoraService
builder.Services.AddScoped<TritoteNic.Services.IJwtService, TritoteNic.Services.JwtService>();
builder.Services.AddScoped<TritoteNic.Services.IPedidoService, TritoteNic.Services.PedidoService>();
builder.Services.AddScoped<TritoteNic.Services.IClienteService, TritoteNic.Services.ClienteService>();
builder.Services.AddScoped<TritoteNic.Services.IBitacoraService, TritoteNic.Services.BitacoraService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_ME_IN_PRODUCTION";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TritoteNic";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TritoteNic_Users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // Configurar para leer roles desde el token
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

// Configurar políticas de autorización por roles
builder.Services.AddAuthorization(options =>
{
    // Política para Administradores: acceso completo
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Administrador", "Admin"));
    
    // Política para Vendedores: acceso limitado
    options.AddPolicy("VendedorOnly", policy => 
        policy.RequireRole("Vendedor"));
    
    // Política para Admin o Vendedor (acceso común)
    options.AddPolicy("AdminOrVendedor", policy => 
        policy.RequireRole("Administrador", "Admin", "Vendedor"));
    
    // Política por defecto: requiere autenticación
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddControllers().AddNewtonsoftJson();

// CORS para WPF
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWPF", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWPF");

// Middleware de manejo de errores (debe ir antes de UseAuthentication)
app.UseExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
