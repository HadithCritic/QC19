using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QuranCode.Core.Content;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>One verse as the reader displays it.</summary>
public sealed record VerseRow(string Reference, string Text, long Value);

/// <summary>
/// Browse the corpus a chapter at a time.
/// </summary>
/// <remarks>
/// Loads one chapter's verses, not the book. The legacy reader held the whole
/// corpus as a live object graph; here a chapter is at most 286 rows and the
/// rest stays in the database until asked for.
/// </remarks>
public sealed partial class ReaderViewModel : FeatureViewModel
{
    public override string Title => "Reader";
    public override string Description => "Browse chapters and verses";

    public ObservableCollection<Chapter> Chapters { get; } = [];
    public ObservableCollection<VerseRow> Verses { get; } = [];

    [ObservableProperty]
    private Chapter? _selectedChapter;

    [ObservableProperty]
    private string _summary = string.Empty;

    public ReaderViewModel(EngineService engines) : base(engines) { }

    protected override void OnActivated()
    {
        foreach (Chapter chapter in Engines.Engine.Chapters) Chapters.Add(chapter);
        SelectedChapter = Chapters.FirstOrDefault();
    }

    partial void OnSelectedChapterChanged(Chapter? value)
    {
        Verses.Clear();
        if (value is null) return;

        // Chapter is a record struct, so the bound property is Nullable<Chapter>.
        Chapter chapter = value.Value;

        try
        {
            IsBusy = true;
            var engine = Engines.Engine;

            for (int i = 0; i < chapter.VerseCount; i++)
            {
                Verse verse = engine.Verses[chapter.FirstVerse - 1 + i];
                Verses.Add(new VerseRow(
                    $"{chapter.Number}:{verse.NumberInChapter}",
                    verse.Text,
                    engine.ValueOfVerse(verse.Number)));
            }

            Summary = $"{chapter.VerseCount} verses · chapter value {engine.ValueOfChapter(chapter.Number):N0}";
            Error = null;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
