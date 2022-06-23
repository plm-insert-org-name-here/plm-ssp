using System.Collections.Generic;

namespace Api.Services
{
    public class LocationSnapshotCache
    {
        private readonly Dictionary<int, byte[]> _snapshots = new();

        public byte[]? Get(int id)
        {
            if (_snapshots.ContainsKey(id))
                return _snapshots[id];

            return null;
        }

        public void Set(int id, byte[] newSnapshot)
        {
            if (_snapshots.ContainsKey(id))
                _snapshots[id] = newSnapshot;
            else
                _snapshots.Add(id, newSnapshot);
        }

        public bool Reset(int id)
        {
            return _snapshots.Remove(id);
        }
    }
}