using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using LandsatReflectance.Backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace LandsatReflectance.Api.Services;

public static class AuthenticationService
{
    public static string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnvironmentVariablesService.AuthSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: "FlatEarthers",
            audience: "FlatEarthers",
            claims: claims, 
            expires: DateTime.UtcNow.AddHours(1), 
            signingCredentials: credentials
            );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new AuthenticationException("Failed to generate authentication token. Generated an null or empty string.");
        }

        return jwtToken;
    }
    
    public static async Task<(User? user, string errorMsg)> AuthenticateToken(UsersService usersService, ClaimsIdentity? identity)
    {
        if (identity is null)
        {
#if DEBUG
            return (null, "No claim was provided");
#else
            return "";
#endif
        }

        // asp.net core being a bitch and won't let me disable mapping for some jwt registered claim names
        var userGuidClaim = identity.FindFirst("sub")?.Value
            ?? identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        var userEmailClaim = identity.FindFirst("email")?.Value
            ?? identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        
        if (userGuidClaim is null || userEmailClaim is null)
        {
#if DEBUG
            return (null, "One of the claims \"Sub\", \"Email\", \"HashedPassword\" is missing");
#else
            return (null, String.Empty);
#endif
        }

        var user = await usersService.TryGetUser(userEmailClaim);
        if (user is null)
        {
#if DEBUG
            return (null, $"Could not find user with email \"{userEmailClaim}\"");
#else
            return (null, String.Empty);
#endif
        }

        if (user.Guid != Guid.Parse(userGuidClaim))
        {
#if DEBUG
            return (null, "The guid claim at \"Sub\" was not valid.");
#else
            return (null, String.Empty);
#endif
        }
        
        return (user, String.Empty);
    }
}