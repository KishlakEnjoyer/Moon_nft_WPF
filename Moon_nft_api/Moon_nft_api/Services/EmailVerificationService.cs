// EmailVerificationService.cs
using System.Collections.Concurrent;

public static class EmailVerificationService
{
    private static readonly ConcurrentDictionary<string, (string Code, string Email, DateTime Expiry)> _codes = new();

    public static void StoreCode(string tempId, string code, string email, TimeSpan ttl)
    {
        _codes[tempId] = (code, email, DateTime.UtcNow + ttl);
    }

    public static bool TryGetEmailByCode(string tempId, string inputCode, out string email)
    {
        email = string.Empty;

        if (!_codes.TryGetValue(tempId, out var data))
            return false;

        if (DateTime.UtcNow > data.Expiry)
        {
            _codes.TryRemove(tempId, out _);
            return false;
        }

        if (data.Code == inputCode)
        {
            email = data.Email;
            _codes.TryRemove(tempId, out _);
            return true;
        }

        return false;
    }
}