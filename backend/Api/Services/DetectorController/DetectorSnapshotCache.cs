using System.Collections.Generic;
namespace Api.Services.DetectorController
{
    public class DetectorSnapshotCache
    {
        private readonly Dictionary<int, byte[]> _snapshots = new();

        public byte[] this[int index] => _snapshots[index];

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