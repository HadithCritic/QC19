using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>A labelled figure.</summary>
public sealed record Statistic(string Name, string Value);

/// <summary>
/// Corpus totals and letter frequencies.
/// </summary>
public sealed partial class StatisticsViewModel : FeatureViewModel
{
    public override string Title => "Statistics";
    public override string Description => "Corpus totals and letter frequencies";

    public ObservableCollection<Statistic> Totals { get; } = [];
    public ObservableCollection<Statistic> LetterFrequencies { get; } = [];

    [ObservableProperty]
    private string _summary = string.Empty;

    public StatisticsViewModel(EngineService engines) : base(engines) { }

    protected override void OnActivated()
    {
        var engine = Engines.Engine;
        var segmentation = engine.Segmentation();

        Totals.Add(new Statistic("Chapters", $"{engine.Chapters.Count:N0}"));
        Totals.Add(new Statistic("Verses", $"{segmentation.VerseCount:N0}"));
        Totals.Add(new Statistic("Words", $"{segmentation.WordCount:N0}"));
        Totals.Add(new Statistic("Letters", $"{segmentation.LetterCount:N0}"));
        Totals.Add(new Statistic("Value systems", $"{engine.ValueSystems().Count:N0}"));
        Totals.Add(new Statistic("Book value", $"{engine.ValueOfBook():N0}"));

        // Letter frequencies straight off the segmentation: no separate index.
        var counts = new Dictionary<char, int>();
        foreach (char letter in segmentation.LetterChars)
        {
            counts[letter] = counts.GetValueOrDefault(letter) + 1;
        }

        foreach ((char letter, int count) in counts.OrderByDescending(p => p.Value))
        {
            LetterFrequencies.Add(new Statistic(letter.ToString(), $"{count:N0}"));
        }

        Summary = $"{counts.Count} distinct letters";
    }
}
