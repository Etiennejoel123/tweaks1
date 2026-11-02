# CLAUDE.md - rules for future Claude sessions

Project: Strohmayer Tweaks, a Windows 11 tweaking app (C#, .NET 10, WPF, MVVM).

## Structure
- `src/StrohmayerTweaks.Core` - models, `TweakEngine`, `BackupService`, registry abstraction, logging, system/services/startup services. No UI code.
- `src/StrohmayerTweaks.Tweaks` - `TweakCatalog` (data only).
- `src/StrohmayerTweaks.App` - WPF views (`Views/`), view-models (`ViewModels/`), theme in `App.xaml`. Root namespace is `StrohmayerTweaks.UI`.
- `tests/StrohmayerTweaks.Tests` - xUnit; uses `FakeRegistry`, never the real registry.

## Hard rules
1. Never add fake buttons, placeholder features, or claims of functionality that does not exist.
2. Every system change goes through `TweakEngine` (backup first, rollback on failure) and must be reversible.
3. Every tweak needs an accurate description, the real Windows default (or `null` if the value is absent by default), and an entry that passes `CatalogTests`.
4. Confirm with the user (dialog) before any modification; check admin rights; log the operation.
5. Never disable or stop essential Windows services; extend `ProtectedServices` rather than shrink it.
6. Never download or execute scripts or binaries. No telemetry, no network calls.
7. Do not remove Windows apps, change Defender/Firewall/Update, or touch boot configuration.
8. Do not add dependencies without a clear reason; keep MVVM (no logic in code-behind except window chrome).
9. Do not claim a build or test passed unless it was actually run. WPF only builds on Windows.
10. Never commit secrets, `bin/`, `obj/`, or `.vs/`.

## Commands
`dotnet restore`, `dotnet build StrohmayerTweaks.sln -c Release`, `dotnet test StrohmayerTweaks.sln -c Release`
