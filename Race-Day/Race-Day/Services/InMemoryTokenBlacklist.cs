using System.Collections.Concurrent;

namespace Race_Day.Services;

/// <summary>
/// Thread-safe, in-memory implementation of <see cref="ITokenBlacklist"/>. Revoked entries are
/// pruned once they pass their natural expiry so the set cannot grow without bound.
/// </summary>
public class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _revoked = new();

    /// <inheritdoc />
    public void Revoke(string jti, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        PruneExpired();
        _revoked[jti] = expiresAt;
    }

    /// <inheritdoc />
    public bool IsRevoked(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti) || !_revoked.TryGetValue(jti, out var expiresAt))
        {
            return false;
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            _revoked.TryRemove(jti, out _);
            return false;
        }

        return true;
    }

    private void PruneExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in _revoked)
        {
            if (entry.Value <= now)
            {
                _revoked.TryRemove(entry.Key, out _);
            }
        }
    }
}
