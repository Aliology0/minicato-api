using AlMostashar.Application.Common.Interfaces;
using System.Collections.Concurrent;
namespace AlMostashar.Infrastructure.Services
{
    public class InMemoryConnectionTracker : IConnectionTracker
    {
        private readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();

        public void AddConnection(int userId, string connectionId)
        {
            _userConnections.AddOrUpdate(userId,
                // Factory: creates a NEW set per call (safe for concurrent adds)
                _ => new HashSet<string> { connectionId },
                // If the user already exists, add the new connection ID to their set
                (_, existingSet) =>
                {
                    lock (existingSet)
                    {
                        existingSet.Add(connectionId);
                    }
                    return existingSet;
                });
        }

        public void RemoveConnection(int userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var existingSet))
            {
                lock (existingSet)
                {
                    existingSet.Remove(connectionId);

                    // If the user has no more active connections, remove them from memory
                    if (existingSet.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
            }
        }

        public bool IsUserActive(int userId)
        {
            // Lock the set to get a thread-safe read of Count
            if (!_userConnections.TryGetValue(userId, out var existingSet))
                return false;

            lock (existingSet)
            {
                return existingSet.Count > 0;
            }
        }
    }
}
