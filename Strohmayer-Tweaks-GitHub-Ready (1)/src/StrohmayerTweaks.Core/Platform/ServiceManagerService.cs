using System.ServiceProcess;
using StrohmayerTweaks.Core.Logging;
using StrohmayerTweaks.Core.Models;

namespace StrohmayerTweaks.Core.Platform;

public sealed record ServiceRow(string Name, string DisplayName, string Status, string StartType, bool IsProtected);

/// <summary>Services that Windows (or its security/networking/audio stack) depends on. They can never be stopped from this app.</summary>
public static class ProtectedServices
{
    private static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        "RpcSs","RpcEptMapper","DcomLaunch","LSM","BFE","mpssvc","WinDefend","SecurityHealthService","wscsvc",
        "EventLog","EventSystem","Dhcp","Dnscache","NlaSvc","netprofm","nsi","Wcmsvc","Winmgmt","CryptSvc","KeyIso",
        "PlugPlay","Power","SamSs","lanmanserver","lanmanworkstation","Schedule","ProfSvc","UserManager",
        "StateRepository","TrustedInstaller","wuauserv","Audiosrv","AudioEndpointBuilder","BrokerInfrastructure",
        "SystemEventsBroker","gpsvc","Themes","ShellHWDetection","TimeBrokerSvc","CoreMessagingRegistrar","sppsvc",
        "SENS","UsoSvc","DeviceInstall","DoSvc","TermService","LanmanWorkstation","Appinfo","StorSvc","VaultSvc"
    };

    public static bool IsProtected(string serviceName) => Names.Contains(serviceName);
}

public sealed class ServiceManagerService
{
    private readonly ILog _log;
    private readonly Func<bool> _isAdmin;

    public ServiceManagerService(ILog log, Func<bool> isAdmin) { _log = log; _isAdmin = isAdmin; }

    public IReadOnlyList<ServiceRow> List()
    {
        var rows = new List<ServiceRow>();
        foreach (var sc in ServiceController.GetServices())
        {
            using (sc)
            {
                try
                {
                    rows.Add(new ServiceRow(sc.ServiceName, sc.DisplayName, sc.Status.ToString(),
                        sc.StartType.ToString(), ProtectedServices.IsProtected(sc.ServiceName)));
                }
                catch (Exception ex) { _log.Warn($"Cannot read service {sc.ServiceName}: {ex.Message}"); }
            }
        }
        return rows.OrderBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public OperationResult Start(string name)
    {
        if (!_isAdmin()) return OperationResult.Fail("Administrator rights are required.");
        try
        {
            using var sc = new ServiceController(name);
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            _log.Info($"Started service {name}");
            return OperationResult.Ok($"Service '{name}' started.");
        }
        catch (Exception ex) { _log.Error($"Start service {name} failed", ex); return OperationResult.Fail(ex.Message); }
    }

    public OperationResult Stop(string name)
    {
        if (ProtectedServices.IsProtected(name)) return OperationResult.Fail("This is an essential Windows service and cannot be stopped here.");
        if (!_isAdmin()) return OperationResult.Fail("Administrator rights are required.");
        try
        {
            using var sc = new ServiceController(name);
            if (!sc.CanStop) return OperationResult.Fail("Windows reports that this service cannot be stopped.");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            _log.Info($"Stopped service {name}");
            return OperationResult.Ok($"Service '{name}' stopped. It restarts according to its startup type or when you start it again.");
        }
        catch (Exception ex) { _log.Error($"Stop service {name} failed", ex); return OperationResult.Fail(ex.Message); }
    }
}
