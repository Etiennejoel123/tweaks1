using Microsoft.Win32;
using StrohmayerTweaks.Core.Models;

namespace StrohmayerTweaks.Core.Registry;

public interface IRegistryAccessor
{
    /// <summary>Returns false if the value (or key) does not exist. Throws for unsupported value kinds.</summary>
    bool TryRead(RegHive hive, string keyPath, string valueName, out RegKind kind, out string value);
    void Write(RegHive hive, string keyPath, string valueName, RegKind kind, string value);
    void Delete(RegHive hive, string keyPath, string valueName);
}

public sealed class WindowsRegistryAccessor : IRegistryAccessor
{
    private static RegistryKey Base(RegHive h) => RegistryKey.OpenBaseKey(
        h == RegHive.CurrentUser ? RegistryHive.CurrentUser : RegistryHive.LocalMachine, RegistryView.Registry64);

    public bool TryRead(RegHive hive, string keyPath, string valueName, out RegKind kind, out string value)
    {
        kind = RegKind.String; value = "";
        using var b = Base(hive);
        using var k = b.OpenSubKey(keyPath, false);
        if (k is null) return false;
        var v = k.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        if (v is null) return false;
        switch (k.GetValueKind(valueName))
        {
            case RegistryValueKind.DWord:
                kind = RegKind.DWord;
                value = unchecked((uint)(int)v).ToString();
                return true;
            case RegistryValueKind.String:
                kind = RegKind.String;
                value = (string)v;
                return true;
            default:
                throw new NotSupportedException($"Unsupported registry value kind at {keyPath}\\{valueName}.");
        }
    }

    public void Write(RegHive hive, string keyPath, string valueName, RegKind kind, string value)
    {
        using var b = Base(hive);
        using var k = b.CreateSubKey(keyPath, true) ?? throw new IOException($"Cannot open or create key {keyPath}.");
        if (kind == RegKind.DWord)
            k.SetValue(valueName, unchecked((int)uint.Parse(value)), RegistryValueKind.DWord);
        else
            k.SetValue(valueName, value, RegistryValueKind.String);
    }

    public void Delete(RegHive hive, string keyPath, string valueName)
    {
        using var b = Base(hive);
        using var k = b.OpenSubKey(keyPath, true);
        k?.DeleteValue(valueName, false);
    }
}
