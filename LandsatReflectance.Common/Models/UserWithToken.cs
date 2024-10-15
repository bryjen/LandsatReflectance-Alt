using System.Text.Json.Serialization;
using LandsatReflectance.Common.Converters;

namespace LandsatReflectance.Common.Models;

[JsonConverter(typeof(UserWithTokenConverter))]
public class UserWithToken
{
    public User User { get; set; } = new();
    public string Token = string.Empty;
}