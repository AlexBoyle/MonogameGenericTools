# GlobalUtilities — Developer Reference

All types live in `MonoTools.Core.GlobalUtilities`.

## Folder layout

```
GlobalUtilities/
  Globals.cs          — core MonoGame references: graphicsDevice, spriteBatch, stopwatch
  ContentUtility.cs   — global content loader (wraps ContentManager)
  ContentPool.cs      — per-instance typed asset cache (deprecated, will be removed)
  InputUtility.cs     — keyboard and mouse state, remapping, input consumption
  LoggingUtil.cs      — levelled debug logging via Debug.WriteLine
  ObjectUtilities.cs  — reflection helper: find all subclasses of a type
  Flags.cs            — global string → bool flag store
  WindowUtility.cs    — fullscreen switching, screen/window size queries
```

---

## Startup wiring

Call these in order in `Game.Initialize()`:

```csharp
protected override void Initialize() {
    WindowUtility.initialize(this, graphicsDeviceManager);   // 1 — window + graphics profile
    Globals.initialize(this);                                 // 2 — spriteBatch + stopwatch
    Window.TextInput += (_, e) => InputUtility.enqueueTextInput(e.Character); // 3 — text input
    ContentUtility.initialize(this);                          // 4 — content manager
    SceneUtility.intialize();                                 // 5 — auto-discover + load scenes
    SettingsUtility.intialize();                              // 6 — auto-discover + load YAML settings
    base.Initialize();
}
```

`InputUtility` has no init — call `InputUtility.update(gt)` at the top of `Game.Update()` each frame.

The game loop:

```csharp
protected override void Update(GameTime gameTime) {
    InputUtility.update(gameTime);       // must be first
    SceneUtility.update(gameTime);       // flushes deferred scene add/remove

    foreach (var entry in SceneUtility.liveScenes)
        if (entry.Value.updateStatus == Status.ACTIVE)
            entry.Value.update(gameTime);

    base.Update(gameTime);
}

protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);
    SceneUtility.draw(gameTime);
    base.Draw(gameTime);
}
```

---

## Globals

Holds the three references every other system needs. Call `initialize` once.

```csharp
Globals.initialize(this);

Globals.graphicsDevice   // GraphicsDevice
Globals.spriteBatch      // SpriteBatch (shared)
Globals.gameRef          // Game instance
Globals.globalStopwatch  // running since initialize(); used by LoggingUtil for timestamps
```

### Creating solid-color textures

```csharp
Texture2D white = Globals.GetNewTexture2D(1, 1);                    // 1×1 white
Texture2D red   = Globals.GetNewTexture2D(64, 64, Color.Red);       // 64×64 red
```

Useful for drawing rectangles and UI backgrounds without loading an asset file.

---

## ContentUtility

Thin wrapper around MonoGame's `ContentManager`. Loads assets from the `Content/` directory and returns `null` on failure instead of throwing.

```csharp
ContentUtility.initialize(this);   // sets RootDirectory = "Content"

Texture2D sprite = ContentUtility.get<Texture2D>("textures/player");
SpriteFont font  = ContentUtility.get<SpriteFont>("fonts/Vipnagorgialla");
Effect shader    = ContentUtility.get<Effect>("shaders/blur");
```

Logs an error and returns `null` if the asset can't be loaded. MonoGame's `ContentManager` caches internally — repeated calls for the same name are cheap.

---

## ContentPool\<T\>

> **Deprecated** — `ContentPool` is scheduled for removal. Use `ContentUtility.get<T>()` instead.

---

## InputUtility

Snapshot-based input. Call `update()` once per frame before anything reads input — it captures current and last frame state, and resets all consumption flags.

```csharp
InputUtility.update(gameTime);   // top of Game.Update()
```

### Keyboard

```csharp
InputUtility.isKeyDown(Keys.W)          // held this frame
InputUtility.isKeyUp(Keys.W)            // not held this frame
InputUtility.wasKeyPressed(Keys.Space)  // down this frame, up last frame
InputUtility.wasKeyReleased(Keys.Space) // up this frame, down last frame
```

All methods accept `useInputMap: false` to bypass remapping, and `ignoreKeyDisable: true` to bypass consumption.

#### Key remapping

```csharp
InputUtility.keyboardInputMap[Keys.W] = Keys.Up;   // W behaves as Up
```

Remapping is applied transparently to all `isKeyDown` / `wasKeyPressed` calls unless `useInputMap: false`.

#### Key consumption

Marks a key as "used" for the rest of the frame. Other systems checking that key will see `false`.

```csharp
InputUtility.setKeyInputConsumption(Keys.Enter);
InputUtility.wasKeyDisabled(Keys.Enter)   // true until next frame
```

### Mouse

```csharp
InputUtility.isMouse1Down()       // left held
InputUtility.isMouse2Down()       // right held
InputUtility.isMouse3Down()       // middle held

InputUtility.wasMouse1Pressed()   // left: up last frame → down this frame
InputUtility.wasMouse1Released()  // left: down last frame → up this frame
// wasMouse2Pressed/Released and wasMouse3Pressed/Released follow the same pattern

InputUtility.getMousePosition()       // Point — current cursor position
InputUtility.getMousePositionDiff()   // Point — delta since last frame
InputUtility.getScrollDiff()          // float — scroll wheel delta
```

#### Mouse button consumption

Disables a button for the rest of the current frame. Used by `UIElement` click handling to prevent clicks from falling through to game objects below UI.

```csharp
InputUtility.disableM1();     // suppress left button
InputUtility.disableM2();     // suppress right button
InputUtility.disableM3();     // suppress middle button
InputUtility.disableScroll(); // suppress scroll wheel
```

All `disable*` methods reset automatically at the start of the next `update()`.

#### Text input buffer

Used by `UITextBox` to receive OS-translated character input. Wire it up once in `Game.Initialize()`:

```csharp
Window.TextInput += (_, e) => InputUtility.enqueueTextInput(e.Character);
```

```csharp
InputUtility.dequeueTextInput(out char c)  // consume next buffered character
InputUtility.clearTextInput()              // discard all buffered input
```

---

## LoggingUtil

Writes timestamped messages to `Debug.WriteLine`. The default level is `ERRO` — only errors are printed unless you lower it.

```csharp
LoggingUtil.logLevel = LogLevel.INFO;   // show info, warn, and error
LoggingUtil.logLevel = LogLevel.DEBG;   // show everything
```

| Method | Level | Shown when logLevel ≥ |
|---|---|---|
| `LoggingUtil.debg(s)` | DEBG (0) | DEBG |
| `LoggingUtil.info(s)` | INFO (1) | INFO |
| `LoggingUtil.warn(s)` | WARN (2) | WARN |
| `LoggingUtil.err(s)`  | ERRO (3) | INFO |

Output format: `hh:mm:ss.ff [LEVEL] - message`

> Note: `err()` checks `logLevel >= INFO` rather than `>= ERRO`. At the default `ERRO` level, errors are always shown. However, setting `logLevel = DEBG` will suppress error output — if you use `DEBG`, set `logLevel` to `INFO` or higher to keep errors visible.

---

## DebugUtility

> `DebugUtility` is currently located outside the `Core` folder and is not part of `GlobalUtilities`. It will likely be moved in a future reorganization. It is used internally by `UIElement` to create the 1×1 white texture used for background rendering (`DebugUtility.getSolidTexture`).

---

## ObjectUtilities

Reflects over all loaded assemblies to find every concrete non-abstract subclass of a given type. Used internally by `SceneUtility`, `SettingsUtility`, and anywhere auto-discovery is needed.

```csharp
List<Type> sceneTypes = ObjectUtilities.getAllTypesOfBaseClass<SceneBase>();
```

Returns only concrete (non-abstract) types that are assignable to `T`, excluding `T` itself. Scans `AppDomain.CurrentDomain.GetAssemblies()` — all assemblies loaded at the time of the call.

---

## Flags

A global `string → bool` dictionary. Useful for game state that needs to be readable across systems without passing references.

```csharp
Flags.set("tutorialComplete", true);
Flags.set("bossDefeated", false);

bool done = Flags.get("tutorialComplete");   // false if key not set

Flags.clear("bossDefeated");    // remove one flag
Flags.clearAll();               // wipe everything (e.g. on new game)

var keys = Flags.getAllKeys();   // Dictionary<string,bool>.KeyCollection
```

`get` returns `false` for unknown keys — no exception.

---

## WindowUtility

Screen and window management. Call `initialize` once before using any other method.

```csharp
WindowUtility.initialize(this, _graphicsDeviceManager);
```

Sets `GraphicsProfile.HiDef` and reads the current monitor resolution.

### Queries

```csharp
Vector2 monitor = WindowUtility.getMoniterScreenSize();  // physical display resolution
Point   window  = WindowUtility.getwindowScreenSize();   // current back buffer size
```

> Note: `getMoniterScreenSize` has a typo ("Moniter") — that is the actual method name.

### Fullscreen

```csharp
// Borderless fullscreen (recommended for most games):
WindowUtility.switchToFullScreen(isborderless: true, shouldHardwareSwitch: false);

// Exclusive fullscreen:
WindowUtility.switchToFullScreen(isborderless: false, shouldHardwareSwitch: true);
```

Moves the window to `(0,0)`, resizes the back buffer to the monitor resolution, and calls `ApplyChanges()`.

> Window resize handling is currently a no-op — the resize callback exists but the resize logic inside it is commented out. Manually call `switchToFullScreen` or set back buffer dimensions directly if you need dynamic resizing.
