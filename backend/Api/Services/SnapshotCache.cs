using System.Collections.Generic;

namespace Api.Services
{
    using LocationId = System.Int32;

    public class SnapshotCache
    {
        private readonly Dictionary<LocationId, byte[]> _snapshots = new();

        public byte[]? Get(LocationId id)
        {
            if (_snapshots.ContainsKey(id))
                return _snapshots[id];

            return null;
        }

        public void Set(LocationId id, byte[] newSnapshot)
        {
            if (_snapshots.ContainsKey(id))
                _snapshots[id] = newSnapshot;
            else
                _snapshots.Add(id, newSnapshot);
        }

        public bool Reset(LocationId id)
        {
            return _snapshots.Remove(id);
        }
    }
}