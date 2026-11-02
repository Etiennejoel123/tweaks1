using StrohmayerTweaks.Core.Backup;
using StrohmayerTweaks.Core.Logging;
using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Platform;
using Xunit;

namespace StrohmayerTweaks.Tests;

public sealed class TweakEngineTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "st-tests-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();

    private static readonly Tweak CuTweak = new("t.cu", "CU tweak", "desc", TweakCategory.Registry, RiskLevel.Low,
        new[] { new RegistryValueSpec(RegHive.CurrentUser, @"Software\Test", "Flag", RegKind.DWord, "0", "1") });

    private static readonly Tweak LmTweak = new("t.lm", "LM tweak", "desc", TweakCategory.Registry, RiskLevel.Low,
        new[] { new RegistryValueSpec(RegHive.LocalMachine, @"SOFTWARE\Test", "Flag", RegKind.DWord, "5", null) });

    private TweakEngine Engine(bool admin = true, bool supported = true) =>
        new(_reg, new BackupService(_dir), new NullLog(), () => admin, () => supported);

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

    [Fact]
    public void Apply_WritesValue_AndCreatesBackup()
    {
        _reg.Write(RegHive.CurrentUser, @"Software\Test", "Flag", RegKind.DWord, "1");
        var engine = Engine();
        var result = engine.Apply(CuTweak);
        Assert.True(result.Success, result.Message);
        Assert.Equal(TweakState.Applied, engine.GetState(CuTweak));
        Assert.Single(new BackupService(_dir).List());
    }

    [Fact]
    public void Revert_RestoresOriginalValue_FromBackup()
    {
        _reg.Write(RegHive.CurrentUser, @"Software\Test", "Flag", RegKind.DWord, "7");
        var engine = Engine();
        engine.Apply(CuTweak);
        var result = engine.Revert(CuTweak);
        Assert.True(result.Success, result.Message);
        _reg.TryRead(RegHive.CurrentUser, @"Software\Test", "Flag", out _, out var value);
        Assert.Equal("7", value);
    }

    [Fact]
    public void Revert_DeletesValue_WhenItDidNotExistBefore()
    {
        var engine = Engine();
        engine.Apply(LmTweak);
        Assert.Equal(TweakState.Applied, engine.GetState(LmTweak));
        engine.Revert(LmTweak);
        Assert.False(_reg.TryRead(RegHive.LocalMachine, @"SOFTWARE\Test", "Flag", out _, out _));
    }

    [Fact]
    public void Apply_RequiresAdministrator_ForMachineWideTweaks()
    {
        var result = Engine(admin: false).Apply(LmTweak);
        Assert.False(result.Success);
        Assert.Empty(_reg.Values);
    }

    [Fact]
    public void Apply_IsRefused_OnUnsupportedOs()
    {
        var result = Engine(supported: false).Apply(CuTweak);
        Assert.False(result.Success);
        Assert.Empty(_reg.Values);
    }

    [Fact]
    public void Apply_RollsBack_WhenWriteFails()
    {
        _reg.Write(RegHive.CurrentUser, @"Software\Test", "Flag", RegKind.DWord, "1");
        _reg.FailOnWrite = true;
        var result = Engine().Apply(CuTweak);
        Assert.False(result.Success);
        _reg.FailOnWrite = false;
        _reg.TryRead(RegHive.CurrentUser, @"Software\Test", "Flag", out _, out var value);
        Assert.Equal("1", value);
    }

    [Fact]
    public void Apply_Twice_IsRejected()
    {
        var engine = Engine();
        Assert.True(engine.Apply(LmTweak).Success);
        Assert.False(engine.Apply(LmTweak).Success);
    }

    [Fact]
    public void Restore_FromBackupEntry_Works_AndCannotRunTwice()
    {
        _reg.Write(RegHive.CurrentUser, @"Software\Test", "Flag", RegKind.DWord, "1");
        var engine = Engine();
        engine.Apply(CuTweak);
        var backup = new BackupService(_dir).List().First();
        Assert.True(engine.Restore(backup).Success);
        _reg.TryRead(RegHive.CurrentUser, @"Software\Test", "Flag", out _, out var value);
        Assert.Equal("1", value);
        var again = new BackupService(_dir).List().First();
        Assert.False(engine.Restore(again).Success);
    }
}
