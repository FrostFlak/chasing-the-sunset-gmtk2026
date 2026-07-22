# CMS — Content Management System

Load and access game content by filename ID. Supports JSON configs and Unity assets.

---

## Setup

### 1. File Structure
```
Assets/Resources/CMS/
├── Configs/
│   └── TreeData/
│       └── oak_common.json    ← JSON config
├── tree_log.prefab             ← Unity asset
└── ambient_music.mp3           ← Unity asset
```
**Filename = ID** (without extension).

### 2. Create Data Type
```csharp
using System;
using CMS;

[Serializable]
public class TreeData : IDataEntity {
    public string Id { get; set; }
    public int Hp;
    public float MinMass;
}
```

### 3. Load & Use
```csharp
CMS.LoadAll();  // call once at startup

var tree   = CMS.Get<TreeData>("oak_common");
var prefab = CMS.Get<GameObject>("tree_log");
var music  = CMS.Get<AudioClip>("ambient_music");
```

---

## CMS Editor (`CMS > Editor`)
- **🔄 Scan** — load all files from disk
- **✏️ Edit** — toggle read-only / editable mode for JSON fields
- **📂 Click icon** — ping file in Project window
- **🔑 Generate IDs** — create `CMSIdRegistry` const fields (run before build)
- **☑️ Checkboxes** — select multiple items for batch delete
- **Drag & Drop** — drop assets from Project window into the editor to move them to CMS folder

---

## API

| Method | Description |
|--------|-------------|
| `CMS.LoadAll()` | Load all content (lazy — auto-triggers on first `Get<T>()`) |
| `CMS.Reload()` | Force reload |
| `CMS.Get<T>(id)` | Get data or asset by ID |
| `CMS.GetAll<T>()` | Get all entries of type T |
| `CMS.ClearAll()` | Clear everything |

---

## Conventions
- **JSON files** go in `Resources/CMS/Configs/{ClassName}/`
- **Assets** go anywhere under `Resources/CMS/`
- **ID** = filename without extension
- Supported: `.json`, `.prefab`, `.mp3`, `.wav`, `.png`, `.jpg`, `.mat`, `.anim`, and more
