using System.Text.Json.Serialization;
using LandsatReflectance.Api.Models.UsgsApi.Endpoints;
using LandsatReflectance.Api.Models.UsgsApi.Types;

namespace LandsatReflectance.Backend.Utils.SourceGenerators;

[JsonSerializable(typeof(SceneSearchResponse))]
public partial class SceneSearchResponseJsonContext : JsonSerializerContext
{ }