# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## About This Repository

A trimmed fork of [Greenshot](https://github.com/greenshot/greenshot) — a Windows screenshot tool. All external-service plugins (Imgur, Jira, Confluence, Office, Dropbox, Box, ExternalCommand) and Win10-specific extras (Share/OCR) have been removed. What remains: core screenshot capture, annotation, and built-in destinations (file, clipboard, printer, email).

## Build

**DO NOT use `dotnet build`** — it fails with `MSB4801 CodeTaskFactory not supported`. Always use `msbuild` from Visual Studio or MSBuild Tools for Windows.

```powershell
# Restore (run once before building, or when packages change)
msbuild src/Greenshot.sln /p:Configuration=Release /restore /t:PrepareForBuild

# Build Release
msbuild src/Greenshot.sln /p:Configuration=Release /t:Rebuild /v:normal

# Build Debug
msbuild src/Greenshot.sln /p:Configuration=Debug /t:Rebuild /v:normal
```

Output lands in `src/Greenshot/bin/<Configuration>/net480/`. Run the app with `src/Greenshot/bin/Debug/net480/Greenshot.exe`.

**There are no automated tests.** Testing is manual only.

### Build Gotchas

- Nerdbank.GitVersioning requires a full git clone with history. If the build fails with "Shallow clone lacks the objects required", run `git fetch --unshallow`.
- API credential environment variables (Box, Dropbox, Imgur, etc.) are optional for local dev — the build succeeds without them; plugins just have empty OAuth credentials.

## Solution Structure

| Project | Role |
|---|---|
| `src/Greenshot` | WinExe entry point — capture hotkeys, tray icon, UI forms, built-in destinations |
| `src/Greenshot.Base` | Core library — interfaces, DI container, image helpers, configuration |

## Architecture

### Startup Sequence

`GreenshotMain.cs` → parse CLI args → initialize log4net → load INI config → register destinations/processors → start hotkeys → `MainForm`.

### Capture Pipeline

`CaptureHelper` (`src/Greenshot/Helpers/CaptureHelper.cs`) coordinates capture modes (region, window, fullscreen). `WindowsGraphicsCaptureInterop` and `src/Greenshot/Native/DirectX/` implement the DirectX-based backend — do not remove these files.

### Destinations

Built-in destinations are in `src/Greenshot/Destinations/` and registered in `MainForm.RegisterInternalDestinations()`. To add a destination: subclass `AbstractDestination` (`src/Greenshot.Base/Core/AbstractDestination.cs`) and register an instance there.

### Plugin System

`PluginHelper.LoadPlugins()` scans for `Greenshot.Plugin.*.dll` files at runtime. Plugins implement `IGreenshotPlugin` with three-phase init:

1. `RegisterConfiguration(IniConfig)` — register INI sections before config load
2. `RegisterServices(IServiceLocator)` — register DI services after config load
3. `Start()` — begin execution after all services are available

Plugin entry class naming convention: `Greenshot.Plugin.<Name>.<Name>Plugin`.

### Configuration (Dapplo.Ini)

Define an interface extending `IIniSection`, annotate properties with `[DisplayKey]`. The Roslyn source generator produces the implementation in `obj/generated/`. Retrieve with:

```csharp
var conf = IniConfigRegistry.GetSection<IMyConfiguration>();
```

Core config: `ICoreConfiguration` in `src/Greenshot.Base/Interfaces/`.

### Dependency Injection

`SimpleServiceProvider` (`src/Greenshot.Base/Core/SimpleServiceProvider.cs`):

```csharp
SimpleServiceProvider.Current.AddService<IFoo>(new FooImpl());
var foo = SimpleServiceProvider.Current.GetInstance<IFoo>();
```

## Key Extension Points

| Interface | Purpose | Location |
|---|---|---|
| `IDestination` | Where to send a captured screenshot | `src/Greenshot.Base/Interfaces/` |
| `IGreenshotPlugin` | Full plugin lifecycle | `src/Greenshot.Base/Interfaces/` |
| `IProcessor` | Post-capture processing pipeline | `src/Greenshot.Base/Interfaces/` |
| `IFileFormatHandler` | Add/override image file format I/O | `src/Greenshot.Base/Interfaces/` |

## Coding Conventions

From `src/.editorconfig` and `CONTRIBUTING.md`:

- **Braces**: Allman style (opening brace on its own line)
- **Indentation**: 4 spaces, no tabs
- **Fields**: `_camelCase` (instance), `s_camelCase` (static), `t_camelCase` (thread-static)
- **Visibility**: Always explicit (`private string _foo`, not `string _foo`)
- **`using` directives**: System namespaces first, sorted alphabetically
- **`var`**: Only when the type is obvious from the right-hand side
- **`nameof()`**: Prefer over string literals
- **`this.`**: Avoid unless required for disambiguation

## Translation Tasks

Whenever UI messages are added, changed, or removed, delegate all translation work to the **translation-manager** custom agent. This agent handles all 39 supported language files (XML, UTF-8 BOM), the translation glossary, and validation. Do not attempt translation tasks directly — always use the translation-manager agent.

## CI/CD

`.github/workflows/release.yml` triggers on push to `main` or `release/1.*`. It restores, builds Release, packages a portable ZIP, and creates a GitHub release. Requires GitHub secrets for OAuth credentials (Box, Dropbox, Flickr, Imgur, Photobucket, Picasa).
