using System.Text.Json;
using System.Text.Json.Serialization;
using StrohmayerTweaks.Core.Models;

namespace StrohmayerTweaks.Core.Backup;

public sealed class BackupValue
{
    public RegHive Hive { get; set; }
    public string KeyPath { get; set; } = "";
    public string ValueName { get; set; } = "";
    public bool Existed { get; set; }
    public RegKind Kind { get; set; }
    public string? Data { get; set; }
}

public sealed class BackupEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TweakId { get; set; } = "";
    public string TweakName { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RestoredUtc { get; set; }
    public List<BackupValue> Values { get; set; } = new();
}

/// <summary>Stores the original registry values of a tweak as JSON before the tweak is applied.</summary>
public sealed class BackupService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _dir;
    public string DirectoryPath => _dir;

    public BackupService(string directory) => _dir = directory;

    private string PathFor(string id) => Path.Combine(_dir, id + ".json");

    public void Save(BackupEntry entry)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(PathFor(entry.Id), JsonSerializer.Serialize(entry, Json));
    }

    public IReadOnlyList<BackupEntry> List()
    {
        var result = new List<BackupEntry>();
        if (!Directory.Exists(_dir)) return result;
        foreach (var f in Directory.GetFiles(_dir, "*.json"))
        {
            try
            {
                var e = JsonSerializer.Deserialize<BackupEntry>(File.ReadAllText(f), Json);
                if (e is not null) result.Add(e);
            }
            catch { /* skip unreadable file */ }
        }
        return result.OrderByDescending(e => e.CreatedUtc).ToList();
    }

    /// <summary>Most recent backup of this tweak that has not been restored yet.</summary>
    public BackupEntry? LatestFor(string tweakId) =>
        List().FirstOrDefault(e => e.TweakId == tweakId && e.RestoredUtc is null);

    public void MarkRestored(BackupEntry entry)
    {
        entry.RestoredUtc = DateTime.UtcNow;
        Save(entry);
    }
}
