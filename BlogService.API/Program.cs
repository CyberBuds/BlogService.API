using BlogService.API.Middleware;
using BlogService.Core.Interfaces;
using BlogService.Data;
using BlogService.Repository;
using BlogService.Repository.Auth;
using BlogService.Service;
using BlogService.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories and Services
builder.Services.AddScoped<ITenantService, TenantService>();
// ✅ Required for IHttpContextAccessor (used by CurrentUserService)
builder.Services.AddHttpContextAccessor();
// ✅ Register CurrentUserService
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITokenRepository, AuthRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
// ✅ ADD THIS LINE — was missing
builder.Services.AddScoped<IMediaService, MediaService>();

// ✅ CHANGED: Exposes real inner exception instead of generic 500
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



// JWT Authentication 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];
if (string.IsNullOrEmpty(secretKey)) secretKey = "SuperSecretKeyThatMustBeAtLeast32BytesLong!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role  // ✅ Maps to the full schema URI in your token
    };
});
builder.Services.AddAuthorization();
// Swagger/OpenAPI with JWT Auth & TenantId Header support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog Service Admin API", Version = "v1" });

    // ✅ ADDED — forces GET → POST → PUT → DELETE order in Swagger
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod switch
    {
        "GET" => "1",
        "POST" => "2",
        "PUT" => "3",
        "DELETE" => "4",
        _ => "5"
    }}");

    // JWT Security
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // API Key security definition
    c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
    {
        Description = "API Key for public endpoints. Example: \"X-API-Key: {your_key}\"",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKeyAuth"
                }
            },
            new List<string>()
        }
    });

    c.OperationFilter<TenantHeaderOperationFilter>();
});

//// Configure CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowPortal", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000","https://blogops-platform.vercel.app","https://blog-admin-panel-alpha.vercel.app", "http://localhost:3001")
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPortal", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                // Allow all localhost ports
                if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    return true;

                // Allow all *.vercel.app domains
                if (uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();


// ✅ CHANGED: Shows real inner exception message in response
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Server Error",
                status = 500,
                detail = error.Error.Message,
                innerException = error.Error.InnerException?.Message,           // 👈 real cause
                innerInnerException = error.Error.InnerException?.InnerException?.Message  // 👈 SQL error
            });
        }
    });
});




// Configure the HTTP request pipeline.
app.UseCors("AllowPortal");
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Enable CORS explicitly


// Use Tenant Middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();