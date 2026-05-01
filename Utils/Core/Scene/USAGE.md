# Scene — Developer Reference

All types live in `MonoTools.Core.Scene`.

## Folder layout

```
Scene/
  SceneBase.cs      — base class for all scenes; owns the gameObjects list
  SceneUtility.cs   — static manager: auto-discovery, activation, update/draw dispatch
  SceneEnums.cs     — Status, Timing, DrawType
```

---

## SceneBase

Extend this to create a scene. Every concrete subclass is **auto-discovered** at startup by `SceneUtility.initialize()` via reflection — no manual registration needed.

```csharp
public class GameScene : SceneBase {
    public GameScene() {
        name = "GameScene";
    }

    public override void setup() {
        base.setup();
        gameObjects.Add(new Player());
        gameObjects.Add(new Enemy());
    }

    public override void draw(GameTime gt) {
        // your SpriteBatch calls here
    }
}
```

### Lifecycle methods

| Method | When it runs | Notes |
|---|---|---|
| Constructor | Once at startup (reflection) | Set `name` and optionally `isInitalScene` here |
| `setup()` | Each time the scene is activated (if `isSetup` is false) | Call `base.setup()` — sets `isSetup = true` |
| `update(gt)` | Every frame while active | Base implementation runs the three-pass update loop |
| `draw(gt)` | Every frame while active | Base is a no-op — implement your render passes here |
| `reset()` | When the scene is deactivated via `setInactive()` | Call `base.reset()` — sets `isSetup = false`, allowing `setup()` to run again on next activation. If you override `reset()` without calling `base.reset()`, `isSetup` stays `true` and `setup()` will be skipped on the next activation. |
| `onDestroy()` | Virtual hook, not called automatically | Call manually if needed for explicit teardown |

### Key properties

| Property | Description |
|---|---|
| `name` | String key used by `SceneUtility.setActive/setInactive` |
| `isInitalScene` | Set `true` on one scene to make it load automatically at startup — note: `isInitalScene` is the actual spelling (missing an 'i') |
| `gameObjects` | `ArrayList` of `GameObject` — add your objects in `setup()` |
| `updateStatus` | `Status.ACTIVE` or `Status.INACTIVE` — checked by SceneUtility |
| `rednerStatus` | `Status.ACTIVE` or `Status.INACTIVE` — controls whether `draw()` is called |

> Note: `rednerStatus` is the actual spelling in code (typo for "renderStatus").

### Update loop

`SceneBase.update()` makes three ordered passes over `gameObjects` each frame:

```
Pass 1 — Timing.BEFORE   (physics, input readers)
Pass 2 — Timing.DURRING  (main logic — default for new GameObjects)
Pass 3 — Timing.AFTER    (late update, cleanup)
```

Objects with `updateTiming = Timing.NEVER` are skipped.

### Draw loop

`SceneBase.draw()` is intentionally empty. Override it and sort your own draw calls by `DrawType`:

```csharp
public override void draw(GameTime gt) {
    // Camera-space pass
    Globals.spriteBatch.Begin(transformMatrix: _camera.getCameraMatrix());
    foreach (GameObject obj in gameObjects)
        if (obj.drawTiming == DrawType.WITH_CAMERA)
            obj.draw(gt);
    Globals.spriteBatch.End();

    // Screen-space pass (UI, HUD)
    Globals.spriteBatch.Begin();
    foreach (GameObject obj in gameObjects)
        if (obj.drawTiming == DrawType.WITHOUT_CAMERA)
            obj.draw(gt);
    Globals.spriteBatch.End();
}
```

---

## SceneUtility

Static manager. Drives scene discovery, activation, and the main update/draw dispatch.

### Initialization

Call once at game startup, after MonoGame content is ready:

```csharp
SceneUtility.intialize();
```

This reflects over all loaded assemblies, finds every concrete `SceneBase` subclass, instantiates each one, and registers it. If a scene has `isInitalScene = true` it is immediately activated.

> Note: `intialize` is the actual spelling in code (typo for "initialize").

### Switching scenes

```csharp
SceneUtility.setActive("GameScene");      // activates (calls setup() if first time)
SceneUtility.setInactive("MainMenu");     // deactivates (calls reset())
```

Changes are deferred — `setActive` and `setInactive` queue names and apply them at the start of the next `update()` call.

In practice, always call them as a pair — activate the destination and deactivate the current scene in the same callback:

```csharp
ctx.interactions.bind("onSettings", gt => {
    SceneUtility.setActive("SettingsScene");
    SceneUtility.setInactive(this.name);
});
```

Only one scene is designated `sceneBeingRendered` at a time — the last one passed to `setActive`. `draw()` only calls that scene's `draw()`.

Multiple scenes can be live simultaneously in `liveScenes` (all receive `update()`), but only `sceneBeingRendered` gets `draw()`.

### Game loop integration

```csharp
// In Game.Update:
SceneUtility.update(gameTime);   // flushes deferred add/remove queues only

foreach (var entry in SceneUtility.liveScenes)
    if (entry.Value.updateStatus == Status.ACTIVE)
        entry.Value.update(gameTime);

// In Game.Draw:
SceneUtility.draw(gameTime);
```

`SceneUtility.update()` only handles the deferred scene queue — it does **not** call `update()` on scenes. Your `Game.Update()` is responsible for iterating `liveScenes` and calling each scene's `update()`. `SceneUtility.draw()` calls `draw()` on `sceneBeingRendered` only, if its `rednerStatus` is `ACTIVE`.

### Scene lookup

```csharp
SceneUtility.scenes["GameScene"]      // all registered scenes (set at startup)
SceneUtility.liveScenes["GameScene"]  // only currently active scenes
SceneUtility.sceneBeingRendered       // the scene currently receiving draw()
```

---

## SceneEnums

Defined in `SceneEnums.cs`, used across `Scene`, `Objects`, and `UIElements`.

### Status

```csharp
Status.ACTIVE
Status.INACTIVE
```

Used by `SceneBase.updateStatus` and `rednerStatus` to pause update or draw without deactivating the scene.

### Timing

Controls which update pass a `GameObject` runs in.

```csharp
Timing.BEFORE    // pass 1
Timing.DURRING   // pass 2 — default for GameObject
Timing.AFTER     // pass 3
Timing.NEVER     // skipped
```

> Note: `DURRING` is the actual enum spelling (typo for "during").

### DrawType

Categorizes a `GameObject` for the scene's draw method.

```csharp
DrawType.WITH_CAMERA      // inside a camera-transformed SpriteBatch — default for GameObject
DrawType.WITHOUT_CAMERA   // screen-space / UI pass
DrawType.CUSTOM           // handled manually by the scene
DrawType.NEVER            // never drawn (e.g. Camera base class)
```
