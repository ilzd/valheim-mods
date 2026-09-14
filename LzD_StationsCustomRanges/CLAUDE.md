# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

BepInEx/Harmony mod for Valheim that lets players customize the build range (and per-level range bonus) of crafting stations — Workbench, Forge, Stonecutter, Blackforge, Artisan Table, Galdr Table — plus the max-distance range for station upgrade extensions (chimneys, etc.). All logic lives in a single file: `LzD_StationsCustomRanges/LzD_StationsCustomRanges.cs`.

## Build

Classic .NET Framework (v4.7.2) class library, built via MSBuild/Visual Studio — no dotnet CLI, no test project, no package manager.

```
msbuild LzD_StationsCustomRanges.sln /p:Configuration=Release
```

Or open `LzD_StationsCustomRanges.sln` in Visual Studio and build.

Game/framework assemblies (`assembly_valheim.dll`, `UnityEngine*.dll`, `BepInEx.dll`, `0Harmony.dll`) are vendored as DLLs under `LzD_StationsCustomRanges/Libs/` and referenced directly via `HintPath` — there's no NuGet restore step. The build output is not auto-copied to a Valheim `BepInEx/plugins` folder; deployment is manual.

There is no automated test suite; validate changes by deploying the built DLL to a Valheim install's `BepInEx/plugins` directory and testing in-game (enable `Logs enabled` in the config to see debug output).

## Architecture

- **Single plugin class** (`LzD_StationsCustomRanges`, in `LzD_StationsCustomRanges.cs`) extends `BaseUnityPlugin`. `Awake()` registers all BepInEx `ConfigEntry` settings (grouped into numbered sections: `1 - General`, `2 - Global`, `3 - Workbench`, ... `9 - Upgrades build distance` — section names control ordering in the BepInEx config UI) and then calls `harmony.PatchAll()`.
- **Two Harmony patches**, both nested static classes inside the plugin class:
  - `CraftingStation_UpdateKnownStationsInRange_Patch` postfixes `CraftingStation.UpdateKnownStationsInRange`, iterates all known stations, matches on `station.m_name` (localization keys like `$piece_workbench`), and applies the per-station configured `m_rangeBuild` / `m_extraRangePerLevel` via the `changeStationRange` / `changeStationRangePerLevel` helpers.
  - `StationExtension_Awake_Patch` postfixes `StationExtension.Awake` to override the private `m_maxStationDistance` field (accessed via Harmony's `___fieldName` ref-injection).
- **Global override**: when `2 - Global > a. Global settings enabled` is on, `globalRange`/`globalRangePerLevel` override all individual per-station settings inside `changeStationRange`/`changeStationRangePerLevel`.
- **Master toggles** gate behavior at multiple levels: `modEnabled` (skips `PatchAll` entirely and short-circuits both patches), `rangeEnabled`/`rangeLvlEnabled` (gate base range vs. per-level range changes globally), and per-station `*Enabled` flags. When adding a new station or setting, follow this same layered toggle pattern.
- Logging goes through the local `Log()` helper, gated by the `logsEnabled` config flag — always use it instead of calling `Debug.Log` directly, so users can silence output.
- `modVersion` (top of the class) is bumped independently per release; recent commit history shows this is done as its own commit when publishing a new version.
