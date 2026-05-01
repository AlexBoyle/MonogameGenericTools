# YamlSettingsReader — Developer Reference

All types live in `MonoTools.Core.YamlSettingsReader`.

**Dependency:** `YamlDotNet` — must be present in the project.

## Folder layout

```
YamlSettingsReader/
  SettingItem.cs          — base class for a single parsed entry
  YamlSettingsReader.cs   — generic reader: parses a YAML file into a typed dictionary
  SettingsUtility.cs      — static manager: auto-discovery and global lookup
  FileUtility.cs          — low-level file I/O (YAML read, text read/write)
```

---

## Overview

This system loads YAML data files from `Content/` at startup and makes them available globally by type. The pattern follows the same convention as `SceneUtility` — subclass `YamlSettingsReader<T>`, add a constructor that specifies the file and top-level key, and `SettingsUtility.intialize()` auto-discovers and loads your reader at startup.

---

## YAML file format

Files must live in `Content/` (relative to the assembly). Each file has one top-level key whose value is a list of maps. Every map must include a `key` field — this becomes the lookup key in the dictionary.

> **Content pipeline:** YAML files must be registered in `Content.mgcb` with a `/copy` entry so they are copied to the output directory — the same as HTML and CSS files. See the UIElements USAGE.md project setup section for the exact format.

```
#begin mySettings.yaml
/copy:mySettings.yaml
```

```yaml
weapons:
  - key: sword
    damage: 10
    speed: 1.5
  - key: bow
    damage: 7
    speed: 2.0
```

All values are read as strings and converted by your `SettingItem` subclass.

---

## SettingItem

Base class for one parsed entry. Override `setData()` to map the raw string dictionary to your fields.

```csharp
public class WeaponItem : SettingItem {
    public int damage;
    public float speed;

    public override void setData(Dictionary<string, string> properties) {
        base.setData(properties);   // reads properties["key"] → this.key
        damage = int.Parse(properties["damage"]);
        speed  = float.Parse(properties["speed"]);
    }
}
```

`getKey()` returns `this.key` as lowercase. By default `base.setData()` reads `properties["key"]` — so the YAML field must be named `key`.

**Custom key field:** If your YAML uses a different field name for the key (e.g. `elementId`), skip `base.setData()` and set `this.key` directly. Wrap each field in try/catch to make missing fields non-fatal:

```csharp
public override void setData(Dictionary<string, string> properties) {
    string temp = "";
    try { properties.TryGetValue("elementId", out temp); key = temp; } catch { }
    try { properties.TryGetValue("damage", out temp); damage = int.Parse(temp); } catch { }
    try { properties.TryGetValue("speed", out temp); speed = float.Parse(temp); } catch { }
}
```

---

## YamlSettingsReader\<T\>

Parses a YAML file into a `Dictionary<string, T>`. Subclass it and call `base(filename, topLevelKey)` in the constructor. Auto-discovery requires a public default constructor, but the two-argument constructor is where loading happens.

```csharp
public class WeaponSettings : YamlSettingsReader<WeaponItem> {
    public WeaponSettings() : base("weapons.yaml", "weapons") { }
}
```

That is all that is needed — `SettingsUtility.intialize()` will find and load this automatically.

### Direct access

If you hold a reference to the reader, you can query it directly:

```csharp
WeaponItem sword = reader.get("sword");
List<WeaponItem>  all  = reader.getAllValues();
List<string>      keys = reader.getAllKeys();
Dictionary<string, WeaponItem> raw = reader.getSettings();
```

`get(key)` is case-insensitive (lowercases the key before lookup). Returns `null` if not found.

---

## SettingsUtility

Static manager. Call `intialize()` once at startup — it reflects over all loaded assemblies, finds every concrete `YamlSettingsReader<T>` subclass, instantiates it (which triggers file loading), and registers it.

```csharp
SettingsUtility.intialize();
```

> Note: `intialize` is the actual spelling in code (typo for "initialize").

### Global lookup

```csharp
// Get one entry by key
WeaponItem sword = SettingsUtility.get<WeaponItem>("sword");

// Get all entries
List<WeaponItem>  all  = SettingsUtility.getAllValues<WeaponItem>();
List<string>      keys = SettingsUtility.getAllKeys<WeaponItem>();
```

### Getting the reader itself

```csharp
// Look up by item type — returns YamlSettingsReader<WeaponItem>
var reader = SettingsUtility.getSettingsReader<WeaponItem>();

// Look up by reader type — returns WeaponSettings (the concrete subclass)
var reader = SettingsUtility.getSettingReader<WeaponSettings>();
```

Use `getSettingReader<T>` when you need access to extra methods defined on your concrete reader subclass.

---

## FileUtility

Low-level I/O used internally by `YamlSettingsReader`. Can be used directly if needed.

### YAML files — read from `Content/`

```csharp
Dictionary<object, object> raw = FileUtility.readYamlFile("weapons.yaml");
```

Resolves the path as `<assembly location>\Content\<fileName>`. Returns the full deserialized YAML document.

### Text files — read/write to `My Documents`

```csharp
bool ok = FileUtility.writeTextFile("savelog", new[] { "line1", "line2" });
string[] lines = FileUtility.readTextFile("savelog");
```

Files are stored as `<My Documents>\<fileName>.txt`. Useful for logs or simple save data outside the Content pipeline.

> `writeYamlFile()` is a stub — it always returns `false` and does nothing.
