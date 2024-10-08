using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;
using LandsatReflectance.Backend.Utils.SourceGenerators;

EnvironmentVariablesService.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Insert(0, new CustomDateTimeConverter());
    options.SerializerOptions.Converters.Insert(0, new MetadataConverter());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<LoginTokenResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListAddResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListGetResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneMetadataListResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneSearchResponse>());
    
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SceneSearchResponseJsonContext.Default);
    
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();