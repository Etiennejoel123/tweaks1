# Safety model

1. Backup before modification; abort if the backup cannot be written.
2. Automatic rollback if a write fails midway.
3. Confirmation dialog for every modification.
4. Administrator check for machine-wide changes; Windows 11 check for all tweaks.
5. Protected-service list prevents stopping essential services.
6. Startup entries are toggled, never deleted.
7. No script execution, no downloads, no network access.
8. All operations are logged to `%LocalAppData%\StrohmayerTweaks\Logs`.

Limitations: restoring works from the values captured when a tweak was applied. If you change the same value by other means in between, a restore overwrites it. Group Policy or other management software may override policy-based tweaks.
