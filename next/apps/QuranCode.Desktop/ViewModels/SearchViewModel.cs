using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>
/// Arabic text search.
/// </summary>
/// <remarks>
/// <para>
/// The search runs off the UI thread and reports progress through
/// <see cref="FeatureViewModel.IsBusy"/>, per brief §41. The legacy UI ran
/// searches inline and kept the window responsive with
/// <c>Application.DoEvents()</c>, which is not a substitute for getting off the
/// thread.
/// </para>
/// <para>
/// Results are capped for display. A query like "min" matches 3,092 verses, and
/// the brief (§19) asks for virtualized views rather than thousands of live
/// controls; until the list is virtualized, the cap keeps the page honest and
/// says how many were held back.
/// </para>
/// </remarks>
public sealed partial class SearchViewModel : FeatureViewModel
{
    private const int DisplayLimit = 200;

    public override string Title => "Search";
    public override string Description => "Find words in the Arabic text";

    public ObservableCollection<VerseRow> Results { get; } = [];

    [ObservableProperty]
    private string _term = string.Empty;

    [ObservableProperty]
    private int _wordnessIndex;

    [ObservableProperty]
    private string _summary = "Enter a term and press Search.";

    public string[] WordnessOptions { get; } =
        ["Anywhere", "Whole word", "Part of a word"];

    public SearchViewModel(EngineService engines) : base(engines) { }

    [RelayCommand]
    private async Task RunAsync()
    {
        if (string.IsNullOrWhiteSpace(Term))
        {
            Summary = "Enter a term.";
            return;
        }

        IsBusy = true;
        Error = null;
        Results.Clear();

        try
        {
            string term = Term;
            Wordness wordness = WordnessIndex switch
            {
                1 => Wordness.WholeWord,
                2 => Wordness.PartOfWord,
                _ => Wordness.Any,
            };

            // Off the UI thread: building the segmentation on first search is
            // the one genuinely slow step, and it must not freeze the window.
            (SearchResult result, List<VerseRow> rows) = await Task.Run(() =>
            {
                var engine = Engines.Engine;
                SearchResult found = engine.Search().Find(term, wordness);

                var list = new List<VerseRow>(Math.Min(found.VerseCount, DisplayLimit));
                foreach (int verseNumber in found.Verses.Take(DisplayLimit))
                {
                    Verse verse = engine.Verse(verseNumber);
                    list.Add(new VerseRow(
                        $"{verse.ChapterNumber}:{verse.NumberInChapter}",
                        verse.Text,
                        engine.ValueOfVerse(verse.Number)));
                }
                return (found, list);
            });

            foreach (VerseRow row in rows) Results.Add(row);

            Summary = result.VerseCount > DisplayLimit
                ? $"{result.WordCount:N0} words in {result.VerseCount:N0} verses · showing first {DisplayLimit}"
                : $"{result.WordCount:N0} words in {result.VerseCount:N0} verses";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Summary = "Search failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
