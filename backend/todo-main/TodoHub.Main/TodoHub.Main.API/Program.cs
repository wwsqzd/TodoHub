using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.Core.Mappings;
using TodoHub.Main.Core.Services;
using TodoHub.Main.Core.Validation;
using TodoHub.Main.DataAccess.Context;
using TodoHub.Main.DataAccess.Interfaces;
using TodoHub.Main.DataAccess.Repository;


// logger
var logFile = "Logs/myapp.txt";
foreach (var file in Directory.GetFiles("Logs"))
{
    File.Delete(file);
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() 
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("SourceContext") == false ||
            e.Properties["SourceContext"].ToString().Contains("TodoHub.Main")
        )
        .WriteTo.File(
            logFile,
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
    )
    .CreateLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    //logger
    builder.Services.AddSerilog();


    // link 
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );


    // auth
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    // services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<AbstractValidator<RegisterDTO>, RegisterDTOValidator>();
    builder.Services.AddScoped<AbstractValidator<LoginDTO>, LoginDTOValidator>();

    builder.Services.AddScoped<ITodoService, TodoService>();
    builder.Services.AddScoped<ITodoRepository, TodoRepository>();

    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<AbstractValidator<CreateTodoDTO>, CreateTodoDTOValidator>();
    builder.Services.AddScoped<AbstractValidator<UpdateTodoDTO>, UpdateTodoDTOValidator>();
    builder.Services.AddSingleton<ITodoCacheService, TodoCacheService>();


    // rebbitmq

    builder.Services.AddHostedService<TodosCleanerHostedService>();
    builder.Services.AddScoped<ITodosCleanerService, TodosCleanerService>();
    builder.Services.AddSingleton<IQueueProducer>(sp =>
    {
        return QueueProducer.CreateAsync().GetAwaiter().GetResult();
    });


    // every 12 hours
    builder.Services.AddHostedService<RefreshTokensHostedService>();
    builder.Services.AddScoped<IRefreshTokensCleanerService, RefreshTokensCleanerService>();


    //redis
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect("localhost:6379"));

    // mapping
    builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); });

    // allow frontend fetch
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            builder => builder
                .WithOrigins(
                "http://localhost:3000",
                "http://192.168.208.1:3000",
                "http://10.0.0.183:3000"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });



    // Swagger
    builder.Services.AddEndpointsApiExplorer();

    // Bearer token
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "TodoHub API",
            Version = "v1"
        });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Token: Bearer {token}",
            Name = "Authorization",
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

    //  rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("LoginPolicy", HttpContext =>
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "uknown";

            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.AddPolicy("RefreshPolicy", httpContext =>
        {
            var userId = httpContext.User.FindFirst("UserId")?.Value ?? "anon";
            return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.AddPolicy("TodosPolicy", HttpContext =>
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value ?? "anon";
            return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.AddPolicy("SignUpPolicy", HttpContext =>
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "uknown";

            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.RejectionStatusCode = 429;
    });



    var app = builder.Build();



    // rate limiting
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // alow frontend
    app.UseCors("AllowFrontend");

    app.UseHttpsRedirection();


    // swagger
    app.UseSwagger();
    app.UseSwaggerUI();

    // auth
    app.UseAuthentication();
    app.UseAuthorization();

   

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

