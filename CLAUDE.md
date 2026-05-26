# Greenshot — Developer Guide

This is a trimmed fork of [Greenshot](https://github.com/greenshot/greenshot) containing only the core screenshot capture and annotation features. All external-service plugins (Imgur, Jira, Confluence, Office, Dropbox, Box, ExternalCommand), the Windows installer, and Win10-specific extras (Share/OCR) have been removed.

## Build

```bash
dotnet build src/Greenshot.sln
```

- **Target**: .NET Framework 4.8 (`net480`), Windows only
- Post-build copies `log4net-debug.xml` or `log4net-release.xml` as `log4net.xml` depending on configuration
- Output lands in `src/Greenshot/bin/<Configuration>/net480/`

## Solution Structure

| Project | Role |
|---|---|
| `Greenshot` | WinExe entry point — capture hotkeys, tray icon, UI forms, built-in destinations |
| `Greenshot.Base` | Core library — interfaces, DI container, image helpers, configuration |

## Architecture

### Entry Point
`src/Greenshot/GreenshotMain.cs` → `MainForm` (`src/Greenshot/Forms/MainForm.cs`)

Startup sequence: parse CLI args → initialize log4net → load plugins → register destinations/processors → start hotkeys.

### Built-in Destinations
Located in `src/Greenshot/Destinations/`. Registered in `MainForm.RegisterInternalDestinations()` (~line 440):
- `ClipboardDestination`, `FileDestination`, `FileWithDialogDestination`
- `PrinterDestination`, `EmailDestination`, `PickerDestination`, `EditorDestination`

To add a destination: subclass `AbstractDestination` (`src/Greenshot.Base/Core/AbstractDestination.cs`) and add an instance to the list in `RegisterInternalDestinations()`.

### Plugin System
`PluginHelper.LoadPlugins()` (`src/Greenshot/Helpers/PluginHelper.cs`) scans for `Greenshot.Plugin.*.dll` files at runtime (next to the executable). Plugins implement `IGreenshotPlugin` with a three-phase init:
1. `RegisterConfiguration()` — register INI config sections
2. `RegisterServices()` — register DI services
3. `Start()` — begin execution

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

### Capture Pipeline
`CaptureHelper` (`src/Greenshot/Helpers/CaptureHelper.cs`) coordinates capture modes (region, window, fullscreen). `WindowsGraphicsCaptureInterop` (`src/Greenshot/Native/WindowsGraphicsCaptureInterop.cs`) and `src/Greenshot/Native/DirectX/` provide the DirectX-based capture backend — do not remove these.

## Key Extension Points

| Interface | Purpose | Location |
|---|---|---|
| `IDestination` | Where to send a captured screenshot | `src/Greenshot.Base/Interfaces/` |
| `IGreenshotPlugin` | Full plugin lifecycle | `src/Greenshot.Base/Interfaces/` |
| `IProcessor` | Post-capture processing pipeline | `src/Greenshot.Base/Interfaces/` |
| `IFileFormatHandler` | Add/override image file format I/O | `src/Greenshot.Base/Interfaces/` |

## No Tests

There are no test projects in this repository.
