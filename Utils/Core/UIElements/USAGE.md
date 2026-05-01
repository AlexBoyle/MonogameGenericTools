# UIElements — Developer Reference

All types live in `MonoTools.Core.UIElements`.

## Folder layout

```
UIElements/
  UIElement.cs          — base element: layout, draw, input
  UIEnums.cs            — Unit, Orientation, Justify, Spacing
  SimpleButton.cs       — UIElement subclass with built-in colors and centred text
  ScrollBox.cs          — scrollable grid container (fixed columns)
  UIScrollPane.cs       — vertically scrollable container with auto scrollbar
  UITextBox.cs          — text input field; created by <input type="text">
  SetImageButton.cs     — sprite-sheet button with multiple states
  UIManager.cs          — scene-level update/draw dispatcher

  HtmlLoader/           — HTML template loading and data binding
    UIHtmlLoader.cs     — parses .html → UIElement tree; component system
    UIContext.cs        — entry point: wraps BindingContext + InteractionContext
    BindingContext.cs   — named getters polled every frame
    InteractionContext.cs — named Action<GameTime> handlers
    UILoadResult.cs     — load() return value: root + id map + bindings + list reconciler
    PathNavigator.cs    — walks dot/bracket paths via reflection

    ParserCore/
      StyleParser.cs    — CSS property parsing, shorthand expansion, live-key detection
      ColorParser.cs    — CSS color string → Color
      StyleSheet.cs     — per-load CSS rule store; resolves styles by tag + class
```

---

## UIElement

The base building block. Every visible or interactive element is a `UIElement` or a subclass.

### Dimensions

```csharp
el.setDimensions(400, 300);                    // 400×300 px
el.setDimensions(100, Unit.PER, 50, Unit.PX);  // 100% wide, 50 px tall
el.setWidth(100, Unit.PER);
el.setHeight(40, Unit.PX);
el.setPosition(0, 0);                          // relative to parent content area
```

Units: `Unit.PX` (pixels) or `Unit.PER` (percent of parent content area).

```csharp
el.setMinWidth(100f);   el.setMaxWidth(400f);
el.setMinHeight(32f);   el.setMaxHeight(200f);
```

### Visual

```csharp
el.color        = new Color(30, 30, 40);   // background; Transparent = no fill
el.texture      = someTexture2D;
el.visible      = false;
el.clipToBounds = true;                    // clips child rendering to bounds
```

### Text

Always use `setText()` — it triggers `sizeToText()` (or `updateWordWrap()` for wrapping elements) → layout update. Direct `el.text = "..."` bypasses this.

```csharp
el.setText("Hello");
el.setFontSize(0.5f);          // scale multiplier on the default SpriteFont
el.textColor  = Color.White;
el.centerText = true;
el.wordWrap   = true;          // wrap text; call updateWordWrap() after setting width
UIElement.MakeText("Label", 0.4f);   // static helper: creates + sizes in one call
```

`font-size: 24px` in CSS converts as `px / font.LineSpacing`.

### Word wrap

`<p>` tags have `wordWrap = true` by default. The element takes `width: 100%`, breaks text at word boundaries to fit, and sets its own height to the wrapped line count. Height propagates up auto-height ancestors automatically.

```html
<p class="description">Long body text that will wrap at the element boundary.</p>
```

`white-space: nowrap` opts out; `white-space: normal` opts a `<label>` or `<span>` in. A `<p>` with an explicit CSS `height` wraps but keeps that fixed height (clip to bounds if needed).

### Spacing

```csharp
el.setPadding(16);                  // all four sides
el.setPadding(8, 16, 8, 16);        // left, top, right, bottom
el.setMargin(0, 8, 0, 8);
el.setGap(8f);                      // px between consecutive flow children
```

### Interactivity

```csharp
el.interactive  = true;
el.hoverColor   = Color.LightGray;
el.pressedColor = Color.DarkGray;
el.onClick      = gt => DoSomething();
el.onHover      = gt => ShowTooltip();
el.onHoverExit  = gt => HideTooltip();
el.setCallback(gt => { DoSomething(); return true; });  // also sets interactive = true
```

### Tree management

```csharp
parent.addElement(child);     // triggers layout recalculation
parent.removeElement(child);
el.children   // List<UIElement>
el.parent     // null for root elements
```

When a child changes its dimensions (e.g. text grows), the parent chain is walked automatically: auto-height ancestors recompute their height, and the first fixed-height ancestor repositions all of its children.

### Scene loop

For HTML-loaded UI, drive `_ui.root` (the `UIElement` returned inside `UILoadResult`):

```csharp
// update():
_ui.update(gameTime);        // push binding values first
_ui.root.update(gameTime);   // then input + layout

// draw() — inside SpriteBatch.Begin/End:
Globals.spriteBatch.Begin(
    sortMode: SpriteSortMode.BackToFront,
    blendState: BlendState.NonPremultiplied,
    samplerState: SamplerState.LinearClamp);
_ui.root.draw(gameTime);
Globals.spriteBatch.End();

// after SpriteBatch.End:
_ui.root.customDraw(gameTime);   // clipToBounds / ScrollBox viewport pass
```

For manually constructed trees, call the same three methods on your own top-level `UIElement`.

Use `SamplerState.LinearClamp` in `SpriteBatch.Begin`. `PointClamp` produces pixelated text.

---

## Layout system

### Orientation

```csharp
el.setOrientation(Orientation.COLUMN);    // top → bottom (default)
el.setOrientation(Orientation.ROW);       // left → right
el.setOrientation(Orientation.RELATIVE);  // explicit setPosition() per child; siblings don't stack
el.setOrientation(Orientation.ABSOLUTE);  // like RELATIVE but positions are ignored in parent flow
```

### Cross-axis alignment

`setJustify` aligns on the axis perpendicular to flow. In `COLUMN`: horizontal. In `ROW`: vertical.

```csharp
el.setJustify(Justify.NONE);     // no alignment correction applied
el.setJustify(Justify.START);    // flush to start edge (default)
el.setJustify(Justify.CENTER);
el.setJustify(Justify.END);
```

### Auto-height containers

Omit `height` in CSS (or don't call `setHeight`) and the container sizes to fit its children. If children are added dynamically after load, the parent chain reflows automatically when rows are added or removed.

---

## SimpleButton

`UIElement` subclass with `interactive = true` and sensible defaults out of the box:
- Background: `Color(224, 224, 224)` light gray
- Text color: `Color(30, 30, 30)` near-black
- Hover: `Color(196, 196, 196)`; Pressed: `Color(160, 160, 160)`
- `centerText = true`, `justify = CENTER`

Override any of these after construction.

```csharp
var btn = new SimpleButton("Start Game");
btn.setDimensions(160, 40);
btn.onClick = gt => SceneUtility.setActive("GameScene");
```

---

## SetImageButton

A button that reads its visual states from a horizontal spritesheet. Automatically switches between normal (0), hovered (1), and pressed (2) source rectangles each frame.

```csharp
// Spritesheet has 3 frames side-by-side, each 32×32
var btn = new SetImageButton(spriteSheet, new Rectangle(0, 0, 32, 32), numberOfStates: 3);
btn.setDimensions(32, 32);
btn.onClick = gt => DoSomething();
```

`initialImage` is the source rect for state 0. States 1 and 2 are generated by stepping `initialImage.Width` to the right for each state. `interactive` is set to `true` automatically.

---

## ScrollBox

A fixed-column scrollable grid. Items are sized and positioned automatically in a grid layout; the viewport clips to bounds using a `Viewport` scissor pass. Useful for item grids, inventories, and icon pickers.

```csharp
var box = new ScrollBox();
box.setDimensions(300, 400);
box.setColumns(3);   // 3 items per row (default is 2)
box.addElement(itemA);
box.addElement(itemB);
```

Items are sized to fill their grid cell (cell size = container width ÷ columns, with 4 px padding). All layout and sizing is managed automatically when items are added or removed.

`ScrollBox` renders entirely in `customDraw` — call `customDraw(gt)` after your `SpriteBatch.End`. The scroll bar is drawn as a 5 px strip on the right edge.

State available each frame:

```csharp
box.isMouseOnElement     // true when cursor is inside the box
box.didScrollThisFrame   // true when scroll wheel moved this frame
```

---

## UIScrollPane

A vertically scrollable container. Add children normally — `UIScrollPane` internally wraps them in an auto-height inner element and clips to its own bounds. A proportional scrollbar appears automatically when content overflows.

```csharp
var pane = new UIScrollPane();
pane.setDimensions(300, 400);           // outer (visible) size

pane.addElement(someRow);               // children go into the inner container
pane.addElement(anotherRow);
```

The scrollbar is 6 px wide and reserves space automatically — children lay out at `width - 6` when the bar is visible. Mouse wheel scrolling is consumed so it doesn't propagate to the game when the cursor is over the pane.

In HTML, add `scroll="true"` to any `<div>` to make it a `UIScrollPane`:

```html
<div class="content-panel" scroll="true" style="height: 400px;">
  <!-- children here -->
</div>
```

Or instantiate directly in C#. Unlike `ScrollBox` (which is a fixed-column grid), `UIScrollPane` is a single-column flow container.

---

## UIManager

A static scene-level dispatcher that owns a single root `UIElement` sized to the monitor. Use it when you want to add UI elements directly in C# without HTML loading and don't want to manage a root element yourself.

```csharp
// In scene setup():
var panel = new UIElement();
panel.setDimensions(400, 300);
UIManager.getRootElement().addElement(panel);

// In scene update():
UIManager.update(gameTime);

// In scene draw() — inside SpriteBatch.Begin/End:
UIManager.draw(gameTime);

// After SpriteBatch.End:
UIManager.customDraw(gameTime);
```

`UIManager.update()` checks the monitor size each frame and resizes the root element automatically if the resolution changes.

> **UIManager has a single global root.** Elements added from one scene persist if not removed. Call `UIManager.getRootElement().removeElement(panel)` in your scene's `reset()` — otherwise elements from multiple scenes will accumulate.

For HTML-loaded UI use `UILoadResult.root` directly instead — `UIManager` is for purely code-built trees.

---

## HTML loading — UIHtmlLoader

Loads a `.html` file from `Content/UI/` and builds a `UIElement` tree.

### Project setup

HTML and CSS files are plain files — they are **not** processed by the MonoGame content pipeline, just copied. Each file needs a `/copy` entry in `Content.mgcb`:

```
#begin UI/myScreen.html
/copy:UI/myScreen.html

#begin UI/styles/panels.css
/copy:UI/styles/panels.css
```

The easiest way to add these is via the MonoGame Content Builder (MGCB) editor: add the file, then in the properties panel change **Build Action** from `Build` to `Copy`. The editor writes the correct `/copy:` entry automatically.

Files are loaded at runtime from `Content/UI/` relative to the game's output directory, which is where the content pipeline copies them.

```csharp
UILoadResult ui = UIHtmlLoader.load("myScreen.html", ctx);
ui.root.setDimensions(screenWidth, screenHeight);
```

**Root dimensions must be set after load** — `%`-based children can't resolve until you call `root.setDimensions()`.

### Supported tags

| Tag | Maps to |
|---|---|
| `<div>` | `UIElement` — layout container |
| `<div scroll="true">` | `UIScrollPane` — vertically scrollable container |
| `<label>`, `<p>`, `<span>` | `UIElement` with text, auto-sized to content |
| `<h1>`–`<h6>` | `UIElement` with text, auto-sized to content — no automatic font scaling; set `font-size` in CSS explicitly |
| `<button>` | `SimpleButton` |
| `<img src="...">` | `UIElement` with texture |
| `<hr>` | 100% wide, 1 px tall, mid-gray |
| `<style>` | Inline CSS block |
| `<link rel="stylesheet" href="...">` | External CSS — path relative to the HTML file |
| Any unknown tag | Component file lookup — see Component system |

### Supported CSS properties

| Property | Effect |
|---|---|
| `width`, `height` | `px` or `%` |
| `min-width`, `max-width`, `min-height`, `max-height` | size constraints |
| `background-color` | `element.color` — CSS color or `{key}` live binding |
| `color` | `element.textColor` — or `{key}` live binding |
| `font-size` | `element.fontSize` (`px / font.LineSpacing`) |
| `flex-direction: row / column` | `setOrientation` |
| `align-items`, `justify-content` | `setJustify` — `start`, `center`, `end` |
| `gap` | `setGap` |
| `padding`, `margin` | 1–4-value shorthand expanded per CSS rules |
| `overflow: hidden` | `clipToBounds = true` |
| `display: none` | `visible = false` |
| `text-align: center` | `centerText = true` |
| `white-space: nowrap` | disables word wrap on `<p>` |
| `white-space: normal` | enables word wrap on `<label>` / `<span>` |

### Event attributes

Setting any event attribute automatically sets `interactive = true`.

```html
<button onclick="{onSave}">Save</button>
<div onhover="{onShowTooltip}" onhoverexit="{onHideTooltip}" />
```

Supported: `onclick`, `onhover`, `onhoverexit`.

### ID attribute

```html
<div id="buildMenu" style="width: 200px; height: 400px;" />
```

```csharp
UIElement el = ui.getElementById("buildMenu");
```

### Visibility attribute

```html
<div visible="{isPaused}" style="..." />
```

Entire value must be a single live key.

### HTML constraints

Files must be valid XML: all tags closed or self-closing (`<hr />`), all attributes quoted, exactly one root element.

---

## Binding sigils

| Sigil | When resolved | Source |
|---|---|---|
| `{{key}}` | Once, before XML parse | `props` dict passed to `load()` |
| `{key}` | Every frame via `BindingContext` | Named getter in C# |

```html
<label>World: {{worldName}}</label>
<label>Day: {day}, Pop: {population}</label>
<label style="color: {alertColor};">{oxygen}%</label>
```

---

## UIContext

Entry point for both data and interaction bindings.

```csharp
var ctx = new UIContext();
ctx.bindText("fps",         () => $"{Game.fps:0} fps");
ctx.bindColor("alertBg",    () => oxygen < 20 ? Color.Red : Color.Transparent);
ctx.bindVisible("isPaused", () => GameState.isPaused);
ctx.interactions.bind("onResume", gt => GameState.resume());
UILoadResult ui = UIHtmlLoader.load("hud.html", ctx);
```


---

## BindingContext

Named getters polled each frame.

| Method | Targets |
|---|---|
| `bindText(name, Func<string>)` | `element.text` |
| `bindColor(name, Func<Color>)` | `element.color` or `element.textColor` |
| `bindVisible(name, Func<bool>)` | `element.visible` |
| `register(name, Func<object>)` | path navigation — see below |

A text node can mix static and live tokens:

```html
<label>{{playerName}} — Day {day}, Pop: {population}</label>
```

---

## Path navigation — `register`

```csharp
ctx.register("player", () => player);
```

```html
<label>{player.hp} / {player.maxHp}</label>
<label>{player.stats.stamina}</label>
<label>{inventory[0].name}</label>
<label>{playerStats['health']}</label>
```

| Syntax | What it accesses |
|---|---|
| `{name.prop}` | Public property or field |
| `{name.a.b}` | Chained navigation |
| `{name[0]}` | `IList` / `Array` by index |
| `{name['key']}` | `IDictionary` by string key |

For computed or formatted values, prefer `bindText`:

```csharp
ctx.bindText("hpBar", () => $"{player.hp}/{player.maxHp}");
```

---

## InteractionContext

```csharp
ctx.interactions.bind("onBack",  gt => SceneUtility.setActive("MainMenu"));
ctx.interactions.bind("onApply", gt => SaveSettings());
```

Resolving an unknown name logs a warning and returns a no-op.

---

## UILoadResult

```csharp
_ui.update(gameTime);           // push binding values first
_ui.root.update(gameTime);      // then input + layout
_ui.blockMouseOverUI();         // optional — disables M1/scroll when cursor is over any UI element
```

```csharp
UIElement el = _ui.getElementById("myId");
```

### Handling screen resize

Call `_ui.root.setDimensions()` whenever the window size changes to reflow all `%`-based children:

```csharp
Point screenSize = WindowUtility.getwindowScreenSize();
if (!_lastScreenSize.Equals(screenSize)) {
    _lastScreenSize = screenSize;
    _ui.root.setDimensions(screenSize.X, screenSize.Y);
}
```

---

## Static slot substitution

`props` runs as a string-replace before XML parse. Use for values known at load time.

```csharp
UIHtmlLoader.load("mainMenu.html", null, new Dictionary<string,string> {
    ["appName"] = "Colony", ["version"] = "0.4.2"
});
```

```html
<label>{{appName}} v{{version}}</label>
```

---

## External stylesheets

```html
<link rel="stylesheet" href="styles/panels.css" />
<div class="panel">...</div>
```

Priority: tag rules → class rules (declaration order) → inline `style=""`.

Containers that will be populated dynamically need an explicit `width` in CSS (or `width: 100%`) — the auto-sizer only runs at load time and skips empty containers.

---

## Component system

Unknown tags are resolved to `{tag}.html` files. Search order:
1. `{currentDir}/components/{tag}.html`
2. `{currentDir}/{tag}.html`
3. Extra paths registered via `UIHtmlLoader.addComponentPath(dir)`

```html
<section-heading title="Display" />
<setting-row label="Resolution" value="{resolution}" />
```

Attributes become bindings in the component's context. Static strings, single live keys (`{key}`), and composite templates (`{a} / {b}`) all forward correctly.

Components share the parent's stylesheet, so CSS classes defined in the main HTML's `<link>` are available inside component files.

When a component is loaded via `UIHtmlLoader.load()` directly in C# (e.g. in a `bindList` factory), it does **not** inherit the caller's stylesheet — include a `<link>` inside the component file for any CSS it needs.

See `TileGame/Content/UI/components/` for examples.

---

## Input elements — UITextBox

`<input type="text">` creates a `UITextBox`. Click to focus, Escape/Enter/click-outside to blur. While focused, keyboard input is consumed and routed through `Window.TextInput` (OS-correct character translation). Backspace and Delete support key-repeat.

```html
<input type="text" class="search-input" placeholder="Search..." onchange="{onSearch}" />
<input type="text" value="{playerName}" placeholder="Enter name..." onchange="{onSetName}" />
```

```csharp
ctx.bindInput("onSearch",  v => _filter = v);
ctx.bindInput("onSetName", v => player.name = v);
```

`value="{key}"` seeds the initial value from a text binding getter (read once at load). All standard CSS properties apply — the box has sensible defaults for background, border, and padding that CSS can override.

`UITextBox.clearFocus()` should be called in the scene's `reset()` to prevent a stale focused reference across scene loads.

**CSS properties that affect text boxes:**

| Property | Effect |
|---|---|
| `width`, `height` | Dimensions (default height: 36px) |
| `background-color` | Fill |
| `border`, `border-width`, `border-color` | Outline (blue tint when focused) |
| `padding` | Inner spacing around text |
| `color` | Typed-text color |
| `font-size` | Text scale |

---

## bindList — dynamic lists

Populates a container element from a live C# collection. Items are diffed by key each frame — new items are appended, removed items are detached. Per-item bindings refresh every frame.

```csharp
_ui.bindList(
    "resolutionList",           // id="..." of the container in HTML
    () => _resolutions,         // collection getter
    item => item.id,            // stable unique key
    item => {
        var ctx = new UIContext();
        ctx.bindText("label", () => item.label);
        ctx.bindText("value", () => _selected == item.id ? "✓" : "");
        ctx.interactions.bind("onSelect", _ => _selected = item.id);
        return UIHtmlLoader.load("components/myrow.html", ctx);
    }
);
```

The factory is called once per new item; the returned `UILoadResult` stays alive until the item is removed.

See `TileGame/Game/Scenes/SettingsScene.cs` for a full working example.
