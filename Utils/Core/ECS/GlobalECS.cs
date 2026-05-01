namespace MonoTools.Core.ECS
{
    using Microsoft.Xna.Framework;

    public static class GlobalECS
    {
        private static int _nextEntityId = 0;
        private static readonly Stack<int> _recycled = new();
        private static readonly Dictionary<Type, IComponentArray> _components = new();
        private static readonly List<ISystem> _systems = new();
        private static readonly object _lock = new();

        // ── Entity lifecycle ──────────────────────────────────────────────────

        public static Entity CreateEntity()
        {
            lock (_lock)
            {
                int id = _recycled.Count > 0 ? _recycled.Pop() : _nextEntityId++;
                return new Entity(id);
            }
        }

        public static void DestroyEntity(Entity entity)
        {
            lock (_lock)
            {
                foreach (var array in _components.Values)
                    if (array.HasEntity(entity.id))
                        array.RemoveEntity(entity.id);
                _recycled.Push(entity.id);
            }
        }

        // ── Components ────────────────────────────────────────────────────────

        public static void AddComponent<T>(Entity entity, T value) where T : struct
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_components.TryGetValue(type, out var array))
                {
                    array = new ComponentArray<T>(256);
                    _components[type] = array;
                }
                ((ComponentArray<T>)array).Add(entity.id, value);
            }
        }

        public static ref T GetComponent<T>(Entity entity) where T : struct =>
            ref ((ComponentArray<T>)_components[typeof(T)]).GetRef(entity.id);

        public static ref T GetComponent<T>(int entityId) where T : struct =>
            ref ((ComponentArray<T>)_components[typeof(T)]).GetRef(entityId);

        public static bool HasComponent<T>(Entity entity) where T : struct =>
            HasComponent<T>(entity.id);

        public static bool HasComponent<T>(int entityId) where T : struct =>
            _components.TryGetValue(typeof(T), out var array) && array.HasEntity(entityId);

        public static void RemoveComponent<T>(Entity entity) where T : struct
        {
            if (_components.TryGetValue(typeof(T), out var array) && array.HasEntity(entity.id))
                array.RemoveEntity(entity.id);
        }

        // ── Queries ───────────────────────────────────────────────────────────
        // Returns entity IDs, not Entity structs, for zero-allocation hot paths.
        // Use GetComponent<T>(entityId) to read/write data.

        /// <summary>Returns a span over all entity IDs that have component T.</summary>
        public static ReadOnlySpan<int> Query<T>() where T : struct
        {
            if (!_components.TryGetValue(typeof(T), out var raw)) return ReadOnlySpan<int>.Empty;
            var arr = (ComponentArray<T>)raw;
            return new ReadOnlySpan<int>(arr.dense, 0, arr.Count);
        }

        /// <summary>Returns entity IDs that have both T1 and T2.</summary>
        public static IEnumerable<int> Query<T1, T2>()
            where T1 : struct where T2 : struct
        {
            if (!_components.TryGetValue(typeof(T1), out var raw1)) yield break;
            if (!_components.TryGetValue(typeof(T2), out var raw2)) yield break;
            var a1 = (ComponentArray<T1>)raw1;
            var a2 = (ComponentArray<T2>)raw2;
            // iterate the smaller set to minimise checks
            if (a1.Count <= a2.Count)
            {
                for (int i = 0; i < a1.Count; i++)
                    if (a2.HasEntity(a1.dense[i])) yield return a1.dense[i];
            }
            else
            {
                for (int i = 0; i < a2.Count; i++)
                    if (a1.HasEntity(a2.dense[i])) yield return a2.dense[i];
            }
        }

        /// <summary>Returns entity IDs that have all three components.</summary>
        public static IEnumerable<int> Query<T1, T2, T3>()
            where T1 : struct where T2 : struct where T3 : struct
        {
            if (!_components.TryGetValue(typeof(T1), out var raw1)) yield break;
            if (!_components.TryGetValue(typeof(T2), out var raw2)) yield break;
            if (!_components.TryGetValue(typeof(T3), out var raw3)) yield break;
            var a1 = (ComponentArray<T1>)raw1;
            var a2 = (ComponentArray<T2>)raw2;
            var a3 = (ComponentArray<T3>)raw3;
            for (int i = 0; i < a1.Count; i++)
            {
                int id = a1.dense[i];
                if (a2.HasEntity(id) && a3.HasEntity(id)) yield return id;
            }
        }

        // ── Systems ───────────────────────────────────────────────────────────

        public static void RegisterSystem(ISystem system)
        {
            lock (_lock) { _systems.Add(system); }
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var s in _systems) s.Update(gameTime);
        }

        public static void Draw(GameTime gameTime)
        {
            foreach (var s in _systems) s.Draw(gameTime);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        /// <summary>Clears all entities, components, and systems. Call on new game / scene teardown.</summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _nextEntityId = 0;
                _recycled.Clear();
                _components.Clear();
                _systems.Clear();
            }
        }
    }
}
