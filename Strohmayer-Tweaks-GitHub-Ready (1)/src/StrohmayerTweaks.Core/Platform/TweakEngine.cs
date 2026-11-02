using StrohmayerTweaks.Core.Backup;
using StrohmayerTweaks.Core.Logging;
using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Registry;

namespace StrohmayerTweaks.Core.Platform;

/// <summary>Applies and reverts registry tweaks. Always writes a backup first and rolls back on failure.</summary>
public sealed class TweakEngine
{
    private readonly IRegistryAccessor _reg;
    private readonly BackupService _backups;
    private readonly ILog _log;
    private readonly Func<bool> _isAdmin;
    private readonly Func<bool> _isSupportedOs;

    public TweakEngine(IRegistryAccessor reg, BackupService backups, ILog log, Func<bool> isAdmin, Func<bool>? isSupportedOs = null)
    {
        _reg = reg; _backups = backups; _log = log; _isAdmin = isAdmin;
        _isSupportedOs = isSupportedOs ?? (() => Compatibility.IsWindows11);
    }

    public TweakState GetState(Tweak tweak)
    {
        try
        {
            foreach (var v in tweak.Values)
            {
                if (!_reg.TryRead(v.Hive, v.KeyPath, v.ValueName, out _, out var current)) return TweakState.NotApplied;
                if (!string.Equals(current, v.EnabledValue, StringComparison.Ordinal)) return TweakState.NotApplied;
            }
            return TweakState.Applied;
        }
        catch (Exception ex)
        {
            _log.Warn($"Cannot read state of {tweak.Id}: {ex.Message}");
            return TweakState.Unknown;
        }
    }

    public OperationResult Apply(Tweak tweak)
    {
        var pre = Precheck(tweak.RequiresAdmin);
        if (pre is not null) return pre;
        if (GetState(tweak) == TweakState.Applied) return OperationResult.Fail("This tweak is already applied.");

        BackupEntry entry;
        try
        {
            entry = new BackupEntry { TweakId = tweak.Id, TweakName = tweak.Name };
            foreach (var v in tweak.Values)
            {
                var existed = _reg.TryRead(v.Hive, v.KeyPath, v.ValueName, out var kind, out var data);
                if (existed && kind != v.Kind)
                    return OperationResult.Fail($"Existing value type differs at {v.KeyPath}\\{v.ValueName}; refusing to modify.");
                entry.Values.Add(new BackupValue
                {
                    Hive = v.Hive, KeyPath = v.KeyPath, ValueName = v.ValueName,
                    Existed = existed, Kind = existed ? kind : v.Kind, Data = existed ? data : null
                });
            }
            _backups.Save(entry); // backup must succeed before any modification
        }
        catch (Exception ex)
        {
            _log.Error($"Backup failed for {tweak.Id}; nothing was changed", ex);
            return OperationResult.Fail("Backup failed, nothing was changed: " + ex.Message);
        }

        try
        {
            foreach (var v in tweak.Values) _reg.Write(v.Hive, v.KeyPath, v.ValueName, v.Kind, v.EnabledValue);
            _log.Info($"Applied tweak {tweak.Id} (backup {entry.Id})");
            return OperationResult.Ok($"'{tweak.Name}' applied. Some changes need a sign-out or restart to take effect.");
        }
        catch (Exception ex)
        {
            _log.Error($"Apply failed for {tweak.Id}; rolling back", ex);
            try { WriteBack(entry); _backups.MarkRestored(entry); }
            catch (Exception rb) { _log.Error($"Rollback failed for {tweak.Id}", rb); }
            return OperationResult.Fail("Applying failed and was rolled back: " + ex.Message);
        }
    }

    public OperationResult Revert(Tweak tweak)
    {
        var pre = Precheck(tweak.RequiresAdmin);
        if (pre is not null) return pre;
        try
        {
            var entry = _backups.LatestFor(tweak.Id);
            if (entry is not null)
            {
                WriteBack(entry);
                _backups.MarkRestored(entry);
                _log.Info($"Reverted tweak {tweak.Id} from backup {entry.Id}");
            }
            else
            {
                foreach (var v in tweak.Values)
                {
                    if (v.DefaultValue is null) _reg.Delete(v.Hive, v.KeyPath, v.ValueName);
                    else _reg.Write(v.Hive, v.KeyPath, v.ValueName, v.Kind, v.DefaultValue);
                }
                _log.Info($"Reverted tweak {tweak.Id} to documented defaults (no backup found)");
            }
            return OperationResult.Ok($"'{tweak.Name}' reverted.");
        }
        catch (Exception ex)
        {
            _log.Error($"Revert failed for {tweak.Id}", ex);
            return OperationResult.Fail("Revert failed: " + ex.Message);
        }
    }

    /// <summary>Restores the registry values captured in a specific backup (Restore Center).</summary>
    public OperationResult Restore(BackupEntry entry)
    {
        var pre = Precheck(entry.Values.Any(v => v.Hive == RegHive.LocalMachine));
        if (pre is not null) return pre;
        if (entry.RestoredUtc is not null) return OperationResult.Fail("This backup was already restored.");
        try
        {
            WriteBack(entry);
            _backups.MarkRestored(entry);
            _log.Info($"Restored backup {entry.Id} ({entry.TweakId})");
            return OperationResult.Ok($"Backup of '{entry.TweakName}' restored.");
        }
        catch (Exception ex)
        {
            _log.Error($"Restore failed for backup {entry.Id}", ex);
            return OperationResult.Fail("Restore failed: " + ex.Message);
        }
    }

    private void WriteBack(BackupEntry entry)
    {
        foreach (var v in entry.Values)
        {
            if (v.Existed && v.Data is not null) _reg.Write(v.Hive, v.KeyPath, v.ValueName, v.Kind, v.Data);
            else _reg.Delete(v.Hive, v.KeyPath, v.ValueName);
        }
    }

    private OperationResult? Precheck(bool needsAdmin)
    {
        if (!_isSupportedOs()) return OperationResult.Fail("This tweak can only be applied on Windows 11.");
        if (needsAdmin && !_isAdmin()) return OperationResult.Fail("Administrator rights are required. Restart the app as administrator (Settings page).");
        return null;
    }
}
