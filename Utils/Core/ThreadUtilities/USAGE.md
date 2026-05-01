# ThreadUtilities — Developer Reference

All types live in `MonoTools.Core.ThreadUtilities`.

## Folder layout

```
ThreadUtilities/
  TimerThread.cs    — fixed-rate background thread with pause, resume, and step mode
```

---

## TimerThread

Runs a callback on a background thread at a target ticks-per-second rate. Useful for game simulation loops, AI ticks, network polling, or any work that needs to run independently of the main MonoGame update/draw loop.

The thread is a daemon (`IsBackground = true`) — it will not prevent the process from exiting.

### Construction

```csharp
var sim = new TimerThread(() => {
    // your work here
    return true;   // return value is currently unused
}, initialTps: 20);
```

The callback is a `Func<bool>`. The returned bool is not currently acted on — return `true` by convention.

### Lifecycle

```csharp
sim.start();    // spawns the background thread; returns false if func is null
sim.pause();    // suspends the loop (blocks inside the thread until resumed)
sim.resume();   // unblocks a paused thread
sim.stop();     // signals the thread to exit and blocks until it joins
```

`stop()` unblocks both the pause signal and the step signal, so it is safe to call while the thread is paused or waiting for a step.

**Starting paused:** You can call `pause()` before `start()` to launch the thread in a suspended state. The thread will block immediately on its first tick and wait for `resume()`.

```csharp
sim = new TimerThread(updateFunc, tps: 20);
sim.pause();    // pre-pause before the thread exists
sim.start();    // thread spawns but blocks immediately
// later:
sim.resume();   // begin ticking
```

### Changing the tick rate

```csharp
sim.updateTPS(60);   // change target ticks-per-second at any time
```

Can be called while the thread is running. Resets the tick-time history to zero.

### Step mode

Useful for deterministic simulation debugging — the thread runs exactly one tick per `step()` call instead of free-running.

```csharp
sim.enableStepMode(true);
sim.step();   // trigger one tick
sim.step();   // trigger another
sim.enableStepMode(false);   // return to free-running
```

`step()` is a no-op when step mode is off.

### Performance metrics

```csharp
double avgMs  = sim.getAverageTickTime();           // average tick execution time in ms
double actual = sim.getApproximateTicksPerSecond();  // capped at targetTPS
```

The history buffer holds one entry per target TPS (e.g. 20 entries at 20 TPS) and is updated each tick. Use these to detect when your callback is running over budget.

### State inspection

```csharp
sim.isRunning()      // true after start(), false after stop()
sim.IsPaused         // true while paused
sim.isInStepMode     // true while step mode is active
```

### Thread safety

The callback runs on the background thread. If it reads or writes shared game state, you are responsible for synchronization. `GlobalECS` component writes use locks internally, but MonoGame's `SpriteBatch` and most scene state is not thread-safe — prefer passing results back to the main thread via a concurrent queue or double-buffer.

### Error handling

If the callback throws an unhandled exception, the thread logs the error via `LoggingUtil` and exits silently — the main thread continues running. There is a placeholder comment for a crash-save callback; hook into this if you need to persist state on simulation failure.
