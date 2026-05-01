# Objects — Developer Reference

All types live in `MonoTools.Core.Objects` and `MonoTools.Core.Objects.AssetPooling`.

## Folder layout

```
Objects/
  GameObject.cs               — base class for all scene objects
  TestSquare.cs               — minimal draw example (demo only)

  AssetPooling/
    Poolable.cs               — base class for poolable objects; manages per-type IDs
    Pool.cs                   — generic object pool Pool<T> (stack of inactive + dict of active)

  GenericGameObjects/
    Camera.cs                 — base camera class; override getCameraMatrix()
```

---

## Poolable

Base class for every object that can be recycled through a `Pool<T>`. Assigns a stable per-type integer ID on construction.

```csharp
public class Bullet : Poolable {
    // ID assigned automatically in Poolable()
}
```

| Member | Description |
|---|---|
| `getId()` | Stable integer ID, unique within this type |
| `isActive` | Set to `true` by `Pool.getForUse()`, `false` on return |
| `reset()` | Virtual — called when returned to pool; sets `isActive = false` |

Override `reset()` to clear state before an object is reused:

```csharp
public override void reset() {
    base.reset();       // sets isActive = false
    velocity = Vector2.Zero;
    damage = 0;
}
```

---

## Pool\<T\>

Generic object pool. `T` must extend `Poolable` and have a default constructor.

```csharp
var bulletPool = new Pool<Bullet>(32);   // pre-allocate 32 inactive instances
```

### Getting and returning objects

```csharp
Bullet b = bulletPool.getForUse();       // pops from inactive stack (or new T() if empty)
b.position = spawnPoint;
// ...
bulletPool.removeFromUse(b);             // calls b.reset(), pushes back to inactive stack
```

`getForUse` sets `isActive = true`. `removeFromUse` is a no-op if the object isn't in the active dictionary (safe to call defensively).

### Pre-warming

```csharp
pool.addToPool(64);    // push 64 new instances onto the inactive stack
```

Useful after an initial burst to avoid allocations mid-game.

### Subclassing

Both `getForUse` and `removeFromUse` are `virtual` — override them to hook into acquire/release (e.g. to add to a scene's `gameObjects` list automatically).

---

## GameObject

Extends `Poolable`. The base class for anything that participates in the scene update and draw loop.

```csharp
public class Enemy : GameObject {
    public override void update(GameTime gt) { /* move, AI, etc. */ }
    public override void draw(GameTime gt)   { /* render */ }
}
```

| Property | Default | Description |
|---|---|---|
| `position` | `Vector2.Zero` | World position |
| `updateTiming` | `Timing.DURRING` | When `update()` is called relative to other objects |
| `drawTiming` | `DrawType.WITH_CAMERA` | Which render pass this object belongs to |

### Update timing

`SceneBase.update()` makes three passes over `gameObjects` each frame, in order:

| Value | Pass |
|---|---|
| `Timing.BEFORE` | First — physics, input |
| `Timing.DURRING` | Second — main game logic (default) |
| `Timing.AFTER` | Third — late update, cleanup |
| `Timing.NEVER` | Skipped entirely |

> Note: `DURRING` is the actual enum spelling in code (typo for "during").

### Draw timing

`DrawType` categorizes objects for different render passes. `SceneBase.draw()` is a virtual no-op — your scene subclass is responsible for iterating `gameObjects` and calling `draw()` on the right subset at the right time (e.g. inside/outside the camera `SpriteBatch`).

| Value | Intended use |
|---|---|
| `DrawType.WITH_CAMERA` | Inside a camera-transformed `SpriteBatch` (default) |
| `DrawType.WITHOUT_CAMERA` | UI or screen-space elements, outside the camera transform |
| `DrawType.CUSTOM` | Handled manually by the scene |
| `DrawType.NEVER` | Never drawn |

`Timing` and `DrawType` are defined in `MonoTools.Core.Scene.SceneEnums`.

### Adding objects to a scene

`SceneBase` exposes a protected `ArrayList gameObjects` (non-generic). When iterating, cast each element to `GameObject`:

```csharp
foreach (GameObject obj in gameObjects)
    obj.draw(gt);
```

Add instances in your scene's `setup()`:

```csharp
public class GameScene : SceneBase {
    private Enemy _enemy;

    public override void setup() {
        base.setup();
        _enemy = new Enemy();
        gameObjects.Add(_enemy);
    }
}
```

---

## Camera

A `GameObject` subclass that represents a camera. Defaults to `DrawType.NEVER` and `Timing.NEVER` so the scene loop ignores it — it only produces a transform matrix.

```csharp
public class MyCamera : Camera {
    public override Matrix getCameraMatrix() {
        return Matrix.CreateTranslation(-position.X, -position.Y, 0);
    }
}
```

Because `Timing.NEVER` means the scene loop never calls `update()`, call it yourself at the top of your scene's `draw()`:

```csharp
public override void draw(GameTime gt) {
    _camera.update(gt);   // update first — lerps position, recalculates matrix

    Globals.spriteBatch.Begin(transformMatrix: _camera.getCameraMatrix());
    foreach (GameObject obj in gameObjects)
        if (obj.drawTiming == DrawType.WITH_CAMERA)
            obj.draw(gt);
    Globals.spriteBatch.End();
}
```

---

## TestSquare

A minimal `GameObject` that draws a colored square. Exists as a quick sanity-check and draw example — not intended for production use.

```csharp
var sq = new TestSquare(x: 100, y: 100, texture: whiteTex, tra: 0.5);
gameObjects.Add(sq);
```

`tra` (0.0–1.0) lerps the square's color between red and blue.
