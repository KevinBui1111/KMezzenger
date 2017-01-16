using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KMezzenger.Models
{
    public class ConnectionMapping<T>
    {
        private readonly ConcurrentDictionary<T, HashSet<string>> dicConnection =
            new ConcurrentDictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return dicConnection.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            HashSet<string> connections = dicConnection.GetOrAdd(key, new HashSet<string>());

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            dicConnection.TryGetValue(key, out connections);

            return connections ?? Enumerable.Empty<string>();
        }
        public IEnumerable<string> GetConnections(T[] key)
        {
            return dicConnection.Where(kp => key.Contains(kp.Key)).SelectMany(kp => kp.Value);
        }

        public void Remove(T key, string connectionId)
        {
            HashSet<string> connections;
            if (dicConnection.TryGetValue(key, out connections))
                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        HashSet<string> removedUser;
                        dicConnection.TryRemove(key, out removedUser);
                    }
                }
        }
        public bool HasConnection(T key)
        {
            HashSet<string> conns;
            dicConnection.TryGetValue(key, out conns);
            return conns != null && conns.Count > 0;
        }
    }
}