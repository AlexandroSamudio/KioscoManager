using System.Security;

namespace API.Helpers;

public class CloudinarySettings
{
    public string? CloudName { get; set; }
    public string? ApiKey { get; set; }
    public SecureString? ApiSecret { get; set; }
}
