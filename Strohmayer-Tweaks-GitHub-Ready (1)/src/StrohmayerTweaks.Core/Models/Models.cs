namespace StrohmayerTweaks.Core.Models;

public enum RegHive { CurrentUser, LocalMachine }
public enum RegKind { DWord, String }
public enum RiskLevel { Low, Medium }
public enum TweakCategory { Gaming, Performance, Privacy, Debloat, Network, Registry }
public enum TweakState { NotApplied, Applied, Unknown }

/// <summary>One registry value a tweak changes. DWord values are expressed as unsigned decimal strings.
/// DefaultValue == null means "the value does not exist by default" and revert deletes it.</summary>
public sealed record RegistryValueSpec(
    RegHive Hive, string KeyPath, string ValueName, RegKind Kind, string EnabledValue, string? DefaultValue)
{
    public string Display =>
        $"{(Hive == RegHive.CurrentUser ? "HKCU" : "HKLM")}\\{KeyPath} -> {(ValueName.Length == 0 ? "(Default)" : ValueName)} = {EnabledValue}" +
        $"  (default: {DefaultValue ?? "not set"})";
}

public sealed record Tweak(
    string Id, string Name, string Description, TweakCategory Category, RiskLevel Risk,
    IReadOnlyList<RegistryValueSpec> Values)
{
    public bool RequiresAdmin => Values.Any(v => v.Hive == RegHive.LocalMachine);
}

public sealed record OperationResult(bool Success, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);
    public static OperationResult Fail(string message) => new(false, message);
}
