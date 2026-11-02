# Strohmayer Tweaks

A modern, gaming-styled Windows 11 tweaking utility built with **C# / WPF / .NET 10** using the MVVM pattern.

Every tweak is a documented, individually reversible set of registry values. The app creates a backup before every change, asks for confirmation, checks for administrator rights, logs everything, and never runs downloaded or untrusted scripts.

## Features

| Module | What it does |
|---|---|
| Dashboard | Overview of OS, CPU, RAM, GPU, applied tweaks, backups and admin status |
| System Information | Read-only OS, processor, memory, graphics and storage details |
| Gaming Tweaks | Game DVR, Game Mode, GPU scheduling, multimedia scheduler priority |
| Performance Optimization | Menu delay, transparency, animations, background apps, startup delay |
| Privacy | Advertising ID, tailored experiences, suggestions, activity history, diagnostic level |
| Windows Debloat | Consumer features, Widgets, web search suggestions, lock screen promos (no apps are uninstalled) |
| Network Optimization | Multimedia network throttling, peer-to-peer update sharing |
| Services Manager | List services, start/stop non-essential ones; essential services are protected |
| Registry Tweaks | File Explorer and file-system settings |
| Startup Manager | List Run-key startup entries, enable/disable them reversibly |
| Restore Center | Restore the original values saved before any tweak |
| Settings | Admin status, compatibility, log and backup locations |

See [docs/TWEAKS.md](docs/TWEAKS.md) for the exact registry values of every tweak.

## Screenshots

> Add your own screenshots to `docs/screenshots/` and reference them here.

| Dashboard | Gaming Tweaks |
|---|---|
| _screenshot placeholder_ | _screenshot placeholder_ |

| Services Manager | Restore Center |
|---|---|
| _screenshot placeholder_ | _screenshot placeholder_ |

## Technology stack

- .NET 10 (LTS), C#, WPF, MVVM (hand-written, no UI framework dependency)
- `System.ServiceProcess.ServiceController` for services
- xUnit for tests
- GitHub Actions for CI and releases

## Installation

1. Download `Strohmayer-Tweaks-Windows-x64.zip` from the repository's **Releases** page (or from a workflow run's artifacts).
2. Extract it anywhere and run `StrohmayerTweaks.exe`. The build is self-contained; no .NET install is needed.
3. For machine-wide tweaks and service control, use **Settings > Restart as administrator**.

The app is unsigned, so Windows SmartScreen may warn on first launch.

## Build instructions

Requires Windows 10/11 and the .NET 10 SDK.

```powershell
dotnet restore StrohmayerTweaks.sln
dotnet build StrohmayerTweaks.sln -c Release
dotnet test StrohmayerTweaks.sln -c Release
dotnet publish src/StrohmayerTweaks.App/StrohmayerTweaks.App.csproj -c Release -r win-x64 --self-contained true -o publish
```

Or run `scripts/build-local.ps1`. WPF applications cannot be built on Linux or macOS.

## GitHub Actions

`.github/workflows/build.yml` runs on pushes to `main`, pull requests, version tags and manual dispatch:

1. restore, build and test on `windows-latest` (the job fails if anything fails),
2. publish a Windows x64 self-contained build,
3. upload `Strohmayer-Tweaks-Windows-x64.zip` as a workflow artifact,
4. when a tag like `v1.0.0` is pushed, create a GitHub Release with that ZIP attached.

Manual run: **Actions > Build > Run workflow**. Release: `git tag v1.0.0 && git push origin v1.0.0`.

## Development instructions

- Projects: `Core` (models, engine, backup, logging, system services), `Tweaks` (tweak catalog), `App` (WPF UI + view-models), `Tests`.
- To add a tweak, add one `T(...)` entry to `src/StrohmayerTweaks.Tweaks/TweakCatalog.cs` with a clear description and the real default value. It appears in its category page automatically; `CatalogTests` validates it.
- Read `CLAUDE.md` for the project rules.

## Safety information

- A JSON backup of the original values is written to `%LocalAppData%\StrohmayerTweaks\Backups` **before** any change; if writing fails, nothing is changed.
- Every apply/revert/restore/start/stop asks for confirmation.
- Machine-wide tweaks are blocked without administrator rights; everything is blocked on non-Windows-11 systems.
- Essential Windows services (RPC, Defender, Event Log, networking, audio, Windows Update, ...) cannot be stopped.
- No scripts are downloaded or executed; no network access at all.
- Logs: `%LocalAppData%\StrohmayerTweaks\Logs`.
- Some tweaks (for example GPU scheduling) depend on your hardware. Revert any tweak you do not like.
- Use at your own risk. Create a Windows restore point before experimenting.

## License

MIT - see [LICENSE](LICENSE).
