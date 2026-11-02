# Architecture

```
App (WPF, MVVM)  --->  Tweaks (catalog)  --->  Core (engine, backup, registry, logging)
Tests            --->  Tweaks, Core
```

- **MainViewModel** owns the navigation list. Each entry holds a view-model; `App.xaml` maps view-model types to views with `DataTemplate`s.
- **TweakEngine** applies/reverts a `Tweak` (a list of `RegistryValueSpec`). Flow: precheck (OS, admin) -> read current values -> save backup JSON -> write values -> rollback on error.
- **IRegistryAccessor** abstracts the registry so the engine is unit-tested with an in-memory fake.
- **BackupService** stores one JSON file per applied tweak in `%LocalAppData%\StrohmayerTweaks\Backups`.
- **ServiceManagerService / StartupService** provide the Services and Startup modules.
