namespace Race_Day.Services;

/// <summary>Strongly typed configuration for JWT issuance and validation.</summary>
public class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Symmetric signing key. Must be at least 32 characters.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Token issuer.</summary>
    public string Issuer { get; set; } = "Race_Day";

    /// <summary>Token audience.</summary>
    public string Audience { get; set; } = "RaceDay.Client";

    /// <summary>Lifetime of an issued token in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 60;
}
