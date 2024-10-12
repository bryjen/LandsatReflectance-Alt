using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LandsatReflectance.Api.Middleware;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Api.Utils;
using LandsatReflectance.Api.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;
using LandsatReflectance.Backend.Utils.SourceGenerators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

EnvironmentVariablesService.Init();

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Insert(0, new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Insert(0, new MetadataConverter());
    options.JsonSerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<LoginTokenResponse>());
    options.JsonSerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListAddResponse>());
    options.JsonSerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListGetResponse>());
    options.JsonSerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneMetadataListResponse>());
    options.JsonSerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneSearchResponse>());
    
    options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, SceneSearchResponseJsonContext.Default);
    
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;  // To prevent escaping of '&'
});

builder.Services.AddResponseCaching();

builder.Services.AddControllers()
    .AddNewtonsoftJson();


var dbConnectionString = EnvironmentVariablesService.DbConnectionString;
builder.Services.AddDbContext<UsersDbContext>(options => options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));
builder.Services.AddDbContext<TargetsDbContext>(options => options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));

builder.Services.AddSingleton<UsgsAuthTokenStore>();

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<TargetsService>();
builder.Services.AddScoped<UsgsImageService>();

builder.Services.AddTransient<UsgsAuthHandler>();

builder.Services.AddHttpClient<UsgsImageService>(((_, httpClient) =>
    {
        httpClient.BaseAddress = new Uri("https://m2m.cr.usgs.gov/api/api/json/stable/");
    }))
    .AddHttpMessageHandler<UsgsAuthHandler>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Authentication
#if !DISABLE_AUTHENTICATION
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(EnvironmentVariablesService.AuthSecretKey);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            
            ValidIssuer = "FlatEarthers",
            ValidAudience = "FlatEarthers",
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = msgReceivedContext =>
            {
                msgReceivedContext.Token = msgReceivedContext.Request.Headers["X-Auth-Token"].FirstOrDefault();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
#endif
#endregion


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddCors();



var app = builder.Build();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCaching();

app.UseMiddleware<BaseErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();