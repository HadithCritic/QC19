using CommunityToolkit.Mvvm.ComponentModel;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>
/// Base for a feature page.
/// </summary>
/// <remarks>
/// <para>
/// Brief §18: no giant form. Each feature owns its own view model and its own
/// state, and the shell knows only this base type. Adding a feature means adding
/// a pair of files, never editing a central switch.
/// </para>
/// <para>
/// A feature does its first data work in <see cref="Activate"/> rather than in
/// its constructor, so the shell can create every page cheaply at startup and
/// only the page the user opens touches the database.
/// </para>
/// </remarks>
public abstract partial class FeatureViewModel : ViewModelBase
{
    protected EngineService Engines { get; }

    /// <summary>Name shown in the sidebar.</summary>
    public abstract string Title { get; }

    /// <summary>One line describing the page, shown under the title.</summary>
    public virtual string Description => string.Empty;

    /// <summary>True once <see cref="Activate"/> has run.</summary>
    [ObservableProperty]
    private bool _isActivated;

    /// <summary>Set while a long operation is running.</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>User-facing error text, or null.</summary>
    [ObservableProperty]
    private string? _error;

    protected FeatureViewModel(EngineService engines)
    {
        Engines = engines;
    }

    /// <summary>
    /// Called the first time the page is shown. Override to load data.
    /// </summary>
    public void Activate()
    {
        if (IsActivated) return;
        IsActivated = true;

        try
        {
            OnActivated();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    /// <summary>First-use loading. Runs once.</summary>
    protected virtual void OnActivated() { }
}
