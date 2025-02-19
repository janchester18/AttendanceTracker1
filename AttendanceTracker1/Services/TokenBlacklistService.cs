using System.Collections.Concurrent;

namespace AttendanceTracker1.Services
{
    public class TokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

        public void AddToBlacklist(string token, DateTime expiry)
        {
            _blacklistedTokens.TryAdd(token, expiry);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _blacklistedTokens.ContainsKey(token);
        }
    }
}
