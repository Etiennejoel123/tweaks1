using System.Collections.ObjectModel;
using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Tweaks;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class TweakItemViewModel : ViewModelBase
{
    private readonly Tweak _tweak;
    private readonly AppServices _s;
    private TweakState _state;

    public TweakItemViewModel(Tweak tweak, AppServices s)
    {
        _tweak = tweak; _s = s;
        ToggleCommand = new RelayCommand(Toggle, () => _state != TweakState.Unknown);
        Refresh();
    }

    public string Name => _tweak.Name;
    public string Description => _tweak.Description;
    public string RiskText => _tweak.Risk == RiskLevel.Low ? "Low risk" : "Medium risk";
    public string AdminText => _tweak.RequiresAdmin ? "Needs admin" : "No admin needed";
    public string Details => string.Join(Environment.NewLine, _tweak.Values.Select(v => v.Display));
    public string StateText => _state switch { TweakState.Applied => "Applied", TweakState.NotApplied => "Not applied", _ => "State unknown" };
    public string ActionText => _state == TweakState.Applied ? "Revert" : "Apply";
    public RelayCommand ToggleCommand { get; }

    public void Refresh()
    {
        _state = _s.Engine.GetState(_tweak);
        Raise(nameof(StateText)); Raise(nameof(ActionText));
    }

    private void Toggle()
    {
        var apply = _state != TweakState.Applied;
        var prompt = apply
            ? $"Apply \"{_tweak.Name}\"?\n\n{_tweak.Description}\n\nChanges:\n{Details}\n\nThe current values are backed up first and can be restored."
            : $"Revert \"{_tweak.Name}\"?\n\nThe values saved before it was applied will be restored.";
        if (!_s.Dialogs.Confirm(apply ? "Confirm apply" : "Confirm revert", prompt)) return;

        var result = apply ? _s.Engine.Apply(_tweak) : _s.Engine.Revert(_tweak);
        if (result.Success) _s.Dialogs.Info(result.Message); else _s.Dialogs.Error(result.Message);
        Refresh();
    }
}

public sealed class TweakPageViewModel : ViewModelBase, IRefreshable
{
    public string Title { get; }
    public string Subtitle { get; }
    public ObservableCollection<TweakItemViewModel> Items { get; } = new();

    public TweakPageViewModel(string title, string subtitle, TweakCategory category, AppServices s)
    {
        Title = title; Subtitle = subtitle;
        foreach (var t in TweakCatalog.For(category)) Items.Add(new TweakItemViewModel(t, s));
    }

    public void Refresh() { foreach (var i in Items) i.Refresh(); }
}
