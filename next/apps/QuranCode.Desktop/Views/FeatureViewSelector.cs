using Avalonia.Controls;
using Avalonia.Controls.Templates;
using QuranCode.Desktop.ViewModels;

namespace QuranCode.Desktop.Views;

/// <summary>
/// Picks the view for a feature view model.
/// </summary>
/// <remarks>
/// An explicit map rather than the template's name-convention ViewLocator.
/// Reflection over type names fails silently when a name drifts; this fails at
/// compile time, and the mapping is visible in one place.
/// </remarks>
public sealed class FeatureViewSelector : IDataTemplate
{
    public bool Match(object? data) => data is FeatureViewModel;

    public Control Build(object? data) => data switch
    {
        ReaderViewModel => new ReaderView(),
        SearchViewModel => new SearchView(),
        NumerologyViewModel => new NumerologyView(),
        StatisticsViewModel => new StatisticsView(),
        _ => new TextBlock { Text = $"No view for {data?.GetType().Name}" },
    };
}
