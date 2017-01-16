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
            var conns = dicConnection[key];
            return conns ?? Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            HashSet<string> connections = dicConnection[key];
            if (connections != null)
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
    }
}