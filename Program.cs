using System.Text;
using BillFlow.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Load JWT settings from configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"]!;

// ✅ Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Swagger + JWT Bearer configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BillFlow API",
        Version = "v1" // 👈 Important! This must be present and valid
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter 'Bearer' followed by your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


// ✅ Configure EF Core with SQL Server
builder.Services.AddDbContext<InvoiceProDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// ✅ Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,  // 🔑 Ensures token expiration is validated
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // Optional: to ensure that there's no "grace period" after expiration
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log or handle failed authentication (e.g., expired token)
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    // Handle expired token specifically if needed
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // This will trigger on 401 Unauthorized (e.g., when the token is expired)
                context.Response.Headers["Token-Expired"] = "true"; // Set header using the indexer
                // Or, if you want to append:
                // context.Response.Headers.Append("Token-Expired", "true");
                return Task.CompletedTask;
            }
        };
    });

// ✅ Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://jackson951.github.io",
                "https://my-invoicing-system-app.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication(); // 🔐 Authentication first
app.UseAuthorization();

app.MapControllers();

app.Run();
