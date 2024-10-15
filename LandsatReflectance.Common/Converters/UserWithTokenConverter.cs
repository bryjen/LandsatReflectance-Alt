using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Common.Models;

namespace LandsatReflectance.Common.Converters;

public class UserWithTokenConverter : JsonConverter<UserWithToken>
{
    public override UserWithToken? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var userWithToken = new UserWithToken();

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        
        userWithToken.User = new User
        {
            Guid = root.GetProperty("guid").GetGuid(),
            Email = root.GetProperty("email").GetString() ?? string.Empty,
            PasswordHash = root.GetProperty("passwordHash").GetString() ?? string.Empty,
            IsEmailEnabled = root.GetProperty("isEmailEnabled").GetBoolean()
        };
        
        userWithToken.Token = root.GetProperty("token").GetString() ?? string.Empty;
        return userWithToken;
    }

    public override void Write(Utf8JsonWriter writer, UserWithToken userWithToken, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var userJson = JsonSerializer.Serialize(userWithToken.User, options);
        using var document = JsonDocument.Parse(userJson);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteString("token", userWithToken.Token);
        writer.WriteEndObject();
    }
}