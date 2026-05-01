namespace MonoTools.Core.ECS
{
    interface IComponentArray
    {
        void RemoveEntity(int entityId);
        bool HasEntity(int entityId);
    }

    class ComponentArray<T> : IComponentArray where T : struct
    {
        private readonly object _lock = new();

        public T[] data;
        public int[] sparse;  // entityId -> dense index, -1 = absent
        public int[] dense;   // dense index -> entityId
        public int Count;

        public ComponentArray(int capacity)
        {
            data = new T[capacity];
            dense = new int[capacity];
            sparse = new int[capacity];
            Array.Fill(sparse, -1);
        }

        public bool HasEntity(int entityId)
        {
            return (uint)entityId < (uint)sparse.Length && sparse[entityId] != -1;
        }

        public void Add(int entityId, in T value)
        {
            lock (_lock)
            {
                EnsureSparseCapacity(entityId + 1);
                EnsureDenseCapacity(Count + 1);
                int index = Count++;
                dense[index] = entityId;
                sparse[entityId] = index;
                data[index] = value;
            }
        }

        public ref T GetRef(int entityId)
        {
            return ref data[sparse[entityId]];
        }

        public void RemoveEntity(int entityId)
        {
            lock (_lock)
            {
                int index = sparse[entityId];
                int last = Count - 1;
                data[index] = data[last];
                dense[index] = dense[last];
                sparse[dense[index]] = index;
                sparse[entityId] = -1;
                Count--;
            }
        }

        private void EnsureSparseCapacity(int needed)
        {
            if (needed <= sparse.Length) return;
            int newSize = Math.Max(needed, sparse.Length * 2);
            int old = sparse.Length;
            Array.Resize(ref sparse, newSize);
            Array.Fill(sparse, -1, old, newSize - old);
        }

        private void EnsureDenseCapacity(int needed)
        {
            if (needed <= data.Length) return;
            int newSize = Math.Max(needed, data.Length * 2);
            Array.Resize(ref data, newSize);
            Array.Resize(ref dense, newSize);
        }
    }
}
