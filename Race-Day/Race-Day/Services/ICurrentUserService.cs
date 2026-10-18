namespace Race_Day.Services;

/// <summary>Convenience accessor for the identity of the authenticated caller.</summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's id, or <c>null</c> when unauthenticated.</summary>
    int? UserId { get; }

    /// <summary>The authenticated user's role name, or <c>null</c> when unauthenticated.</summary>
    string? Role { get; }

    /// <summary>Whether the current request carries a valid identity.</summary>
    bool IsAuthenticated { get; }
}
