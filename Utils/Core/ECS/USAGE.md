# ECS — Developer Reference

All types live in `MonoTools.Core.ECS`.

## Folder layout

```
ECS/
  GlobalECS.cs          — static manager: entities, components, queries, systems
  Entity.cs             — lightweight entity struct (wraps an int ID)
  ComponentArray.cs     — dense-sparse storage backing and IComponentArray interface; not used directly
  ISystem.cs            — system interface: Update + optional Draw
  IComponentData.cs     — optional marker interface for component structs
  IComponentProcesser.cs — deprecated alias for ISystem; kept for compatibility
  Position.cs           — example component (Position : IComponentData)
```

---

## Core concepts

- **Entity** — a plain integer ID. Has no data of its own.
- **Component** — a plain `struct` that holds data. Associated with an entity via `GlobalECS`.
- **System** — a class that queries for entities with specific components and runs logic on them each frame.

---

## Entity lifecycle

```csharp
Entity e = GlobalECS.CreateEntity();   // allocates or recycles an ID
GlobalECS.DestroyEntity(e);            // removes all components and recycles the ID
```

Destroyed IDs are pushed onto a recycle stack and reused by the next `CreateEntity` call.

`Entity.Invalid` (`id = -1`) is a sentinel for "no entity". Check `entity.IsValid` before use.

---

## Components

Any `struct` can be a component. Implementing `IComponentData` is optional — it's a marker interface only.

```csharp
public struct Health : IComponentData {
    public int current;
    public int max;
}

public struct Velocity {
    public float x;
    public float y;
}
```

### Add / Get / Remove

```csharp
GlobalECS.AddComponent(entity, new Health { current = 100, max = 100 });
GlobalECS.AddComponent(entity, new Velocity { x = 0, y = 0 });

// GetComponent returns a ref — write through it directly
ref Health hp = ref GlobalECS.GetComponent<Health>(entity);
hp.current -= 10;

GlobalECS.RemoveComponent<Velocity>(entity);
```

`GetComponent` also accepts a raw `int` entity ID (useful inside query loops):

```csharp
ref Health hp = ref GlobalECS.GetComponent<Health>(entityId);
```

### HasComponent

```csharp
if (GlobalECS.HasComponent<Health>(entity)) { ... }
if (GlobalECS.HasComponent<Health>(entityId)) { ... }
```

---

## Queries

Queries return entity IDs, not `Entity` structs, to avoid allocation on hot paths. Use `GetComponent<T>(entityId)` inside the loop.

### Single component — `ReadOnlySpan<int>`

```csharp
foreach (int id in GlobalECS.Query<Velocity>())
{
    ref Velocity v = ref GlobalECS.GetComponent<Velocity>(id);
    v.x += 1f;
}
```

### Two components — `IEnumerable<int>`

```csharp
foreach (int id in GlobalECS.Query<Velocity, Position>())
{
    ref Velocity v = ref GlobalECS.GetComponent<Velocity>(id);
    ref Position p = ref GlobalECS.GetComponent<Position>(id);
    p.x += (int)v.x;
    p.y += (int)v.y;
}
```

### Three components — `IEnumerable<int>`

```csharp
foreach (int id in GlobalECS.Query<Health, Velocity, Position>())
{
    // entity has all three
}
```

---

## Systems

Implement `ISystem` and register once (typically at scene load). `GlobalECS.Update` and `GlobalECS.Draw` call all registered systems in registration order.

```csharp
public class MovementSystem : ISystem
{
    public void Update(GameTime gt)
    {
        foreach (int id in GlobalECS.Query<Velocity, Position>())
        {
            ref Velocity v = ref GlobalECS.GetComponent<Velocity>(id);
            ref Position p = ref GlobalECS.GetComponent<Position>(id);
            p.x += (int)(v.x * (float)gt.ElapsedGameTime.TotalSeconds);
            p.y += (int)(v.y * (float)gt.ElapsedGameTime.TotalSeconds);
        }
    }
}
```

```csharp
// Scene load:
GlobalECS.RegisterSystem(new MovementSystem());
GlobalECS.RegisterSystem(new RenderSystem());

// Game loop:
GlobalECS.Update(gameTime);
GlobalECS.Draw(gameTime);
```

Only implement `Draw()` when the system renders something.

---

## Reset

Clears all entities, components, and systems. Call on scene teardown or new game.

```csharp
GlobalECS.Reset();
```

`GlobalECS` is static and shared across the whole process. `Reset()` is never called automatically — you must call it explicitly when you need a clean ECS state (e.g. starting a new game). If your ECS entities and systems are intentionally global across scenes, don't call it on scene transitions.

> `Reset()` also clears all registered systems. After calling `Reset()`, re-register your systems before the next `Update` call.

---

## IComponentProcesser

```csharp
// Kept for compatibility. Prefer ISystem directly.
public interface IComponentProcesser : ISystem { }
```

Any class already implementing `IComponentProcesser` still works — it extends `ISystem` so `GlobalECS.RegisterSystem` accepts it unchanged.
