using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>
/// The application shell.
/// </summary>
/// <remarks>
/// <para>
/// Holds a list of feature pages and which one is showing. That is all. The
/// legacy MainForm was 61,311 lines with 728 controls and 278 event handlers;
/// the equivalent here knows nothing about what any feature does.
/// </para>
/// <para>
/// Every page is constructed at startup, which is cheap because a feature does
/// no data work until <see cref="FeatureViewModel.Activate"/> runs on first
/// display.
/// </para>
/// </remarks>
public sealed partial class MainViewModel : ViewModelBase
{
    private readonly EngineService _engines;

    public ObservableCollection<FeatureViewModel> Features { get; }

    [ObservableProperty]
    private FeatureViewModel? _selectedFeature;

    [ObservableProperty]
    private string _status = string.Empty;

    /// <summary>Parameterless constructor for the XAML previewer.</summary>
    public MainViewModel() : this(new EngineService()) { }

    public MainViewModel(EngineService engines)
    {
        _engines = engines;

        Features =
        [
            new ReaderViewModel(engines),
            new SearchViewModel(engines),
            new NumerologyViewModel(engines),
            new StatisticsViewModel(engines),
        ];

        Status = engines.TryOpen()
            ? $"content: {Path.GetFileName(engines.DatabasePath)}"
            : engines.Failure ?? "content database unavailable";

        SelectedFeature = Features[0];
    }

    partial void OnSelectedFeatureChanged(FeatureViewModel? value) => value?.Activate();
}
