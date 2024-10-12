using LandsatReflectance.Api.Models;
using LandsatReflectance.Common.Models;

namespace LandsatReflectance.Api.Exceptions;

public class UserRegistrationException : Exception
{
    public User User { get; init; }

    public UserRegistrationException(string message, User user) : base(message)
    {
        User = user;
    }
}