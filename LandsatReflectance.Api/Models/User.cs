namespace LandsatReflectance.Backend.Models;

public class User
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailEnabled { get; set; } = true;


    public static User AdminUser = new User
    {
        Guid = Guid.Parse("08dce7fb-6758-4b4c-8ae2-107dbd84aea1"),
        Email = "admin@admin.com",
        PasswordHash = "AQAAAAIAAYagAAAAEGRUBTmpS7OZKImMrjy2TR5SIupf7Ouz88cFK93SLaaIEDIDiAWIIZZ9cwelKUpEBw==",
        IsEmailEnabled = true
    };
    

    public string ToLogString() => $"User: ({Guid}, {Email}, {PasswordHash}, {IsEmailEnabled})";
}