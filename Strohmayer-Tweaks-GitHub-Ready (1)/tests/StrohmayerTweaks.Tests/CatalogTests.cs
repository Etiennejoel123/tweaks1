using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.Tweaks;
using Xunit;

namespace StrohmayerTweaks.Tests;

public sealed class CatalogTests
{
    [Fact]
    public void TweakIds_AreUnique() =>
        Assert.Equal(TweakCatalog.All.Count, TweakCatalog.All.Select(t => t.Id).Distinct().Count());

    [Fact]
    public void EveryTweak_HasDescription_AndValues()
    {
        foreach (var t in TweakCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Name), t.Id);
            Assert.True(t.Description.Length > 20, t.Id);
            Assert.NotEmpty(t.Values);
        }
    }

    [Fact]
    public void DWordValues_AreValidUnsignedIntegers()
    {
        foreach (var v in TweakCatalog.All.SelectMany(t => t.Values).Where(v => v.Kind == RegKind.DWord))
        {
            Assert.True(uint.TryParse(v.EnabledValue, out _), v.ValueName);
            if (v.DefaultValue is not null) Assert.True(uint.TryParse(v.DefaultValue, out _), v.ValueName);
        }
    }

    [Fact]
    public void EveryCategory_HasTweaks()
    {
        foreach (var c in new[] { TweakCategory.Gaming, TweakCategory.Performance, TweakCategory.Privacy,
                                  TweakCategory.Debloat, TweakCategory.Network, TweakCategory.Registry })
            Assert.NotEmpty(TweakCatalog.For(c));
    }

    [Theory]
    [InlineData("RpcSs")]
    [InlineData("WinDefend")]
    [InlineData("eventlog")]
    [InlineData("Dnscache")]
    public void EssentialServices_AreProtected(string name) => Assert.True(ProtectedServices.IsProtected(name));

    [Fact]
    public void ArbitraryService_IsNotProtected() => Assert.False(ProtectedServices.IsProtected("Fax"));
}
