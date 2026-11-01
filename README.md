# Souvy - Material Patcher Fix

**Material Patcher Fix** is a Harmony utility patch for Timberborn that enhances **Bobingabout Script Pack's Material Patcher** (`Bobingabout.MaterialPatcher.BuildingPrefabMaterialPatcher`).

---

## 🌟 What This Mod Fixes

By default, `BuildingPrefabMaterialPatcher.InitializeEntity` calls `GetComponentsInChildren<Renderer>()` without passing `includeInactive: true`. 

In modular buildings and path-based structures (such as **Tubeways**, **Vertical Tubeways**, multi-connection blocks, and dynamic building models), inactive game objects represent connected mesh variants (e.g. straight segments, corner elbows, T-crossings, cross-junctions). Because inactive renderers were omitted by default:

* A single placed block would receive its material patch (e.g. Whitepaws gold and pink wood textures).
* As soon as other blocks connected, the game activated the multi-way connected sub-meshes, which reverted back to their original unpatched textures (e.g. dark green IronTeeth metal).

**Material Patcher Fix** intercepts `BuildingPrefabMaterialPatcher.InitializeEntity` via a Harmony Prefix patch and includes all inactive child renderers (`GetComponentsInChildren<Renderer>(true)`), ensuring every single mesh variant and connection state is properly and permanently patched.

---

## 🛠️ Requirements

* **Timberborn**: Version `1.0.0.0` / `1.1.0.0` or higher
* **Harmony**: `v2.3.3`+
* **Bobingabout Script Pack**: `v1.0.0`+

---

## 📥 Installation

1. Place `Souvy-MaterialPatcherFix` into your Timberborn Mods directory:
   `Documents/Timberborn/Mods/Souvy-MaterialPatcherFix/`
2. Enable **Material Patcher Fix** in the Timberborn Mod Manager.

---

## 📜 Credits & License

* **Author**: Souvy (`Souvy.MaterialPatcherFix`)
* **Original Material Patcher**: Bobingabout (`BobingaboutScriptPack`)
