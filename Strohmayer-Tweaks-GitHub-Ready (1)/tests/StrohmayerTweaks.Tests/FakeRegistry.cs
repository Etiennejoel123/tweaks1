using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Registry;

namespace StrohmayerTweaks.Tests;

internal sealed class FakeRegistry : IRegistryAccessor
{
    public Dictionary<string, (RegKind Kind, string Value)> Values { get; } = new();
    public bool FailOnWrite { get; set; }

    private static string K(RegHive h, string p, string n) => $"{h}|{p}|{n}".ToLowerInvariant();

    public bool TryRead(RegHive hive, string keyPath, string valueName, out RegKind kind, out string value)
    {
        if (Values.TryGetValue(K(hive, keyPath, valueName), out var v)) { kind = v.Kind; value = v.Value; return true; }
        kind = RegKind.String; value = ""; return false;
    }

    public void Write(RegHive hive, string keyPath, string valueName, RegKind kind, string value)
    {
        if (FailOnWrite) throw new IOException("simulated write failure");
        Values[K(hive, keyPath, valueName)] = (kind, value);
    }

    public void Delete(RegHive hive, string keyPath, string valueName) => Values.Remove(K(hive, keyPath, valueName));
}
