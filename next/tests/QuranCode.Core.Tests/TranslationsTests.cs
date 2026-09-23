using Microsoft.Data.Sqlite;
using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Phase 6: translations, translation search, the Emlaaei fallback, word data and packs.</summary>
public sealed class TranslationsTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly QuranCodeEngine _classic = new(TestPaths.ContentDatabase);
    private readonly List<string> _files = [];

    public void Dispose()
    {
        _engine.Dispose();
        _classic.Dispose();
        SqliteConnection.ClearAllPools();
        foreach (string file in _files.Where(File.Exists)) File.Delete(file);
    }

    [Fact]
    public void TheSubmissionEditionCarriesItsTranslations()
    {
        // ADR 0004 narrows the edition to three texts: Khalifa's English, the
        // transliteration and the Emlaaei standard spelling.
        Assert.Equal(3, _engine.Translations.Count);
        TranslationInfo english = _engine.Translation("submission.en")!;
        Assert.Equal("Rashad Khalifa", english.Translator);
        Assert.StartsWith("In the name of GOD", _engine.TranslationText(english, 1, 1)[1]);
        Assert.Equal("transliteration", _engine.Translation("submission.translit")!.Kind);
        Assert.Equal("emlaaei", _engine.Translation("submission.emlaaei")!.Kind);

        // The standard spelling of verse 1 loses the Bismillah this edition keeps as verse 0.
        int verse = _engine.Verse(2, 1).Number;
        Assert.Equal("الم", _engine.TranslationText(_engine.Translation("submission.emlaaei")!, verse, verse)[verse]);
    }

    [Theory]
    [InlineData("الله", true)]
    [InlineData("ٱللَّهِ ۚ", true)]
    [InlineData("آمنوا", true)]
    [InlineData("God", false)]
    [InlineData("خدای", false)] // ی is not one of the 36 Quran letters
    [InlineData("الله 1", false)]
    public void TheLanguageRuleFollowsTheOriginal(string text, bool arabic) => Assert.Equal(arabic, TranslationSearch.IsArabic(text));

    [Fact]
    public void TranslationSearchIgnoresCaseAndHonorsWordness()
    {
        Assert.Equal([(0, 3)], TranslationSearch.Matches("God is great", "god", Wordness.WholeWord));
        Assert.Empty(TranslationSearch.Matches("Godly", "god", Wordness.WholeWord));
        Assert.Equal([(0, 3)], TranslationSearch.Matches("Godly", "god", Wordness.PartOfWord));

        TranslationInfo english = _engine.Translation("submission.en")!;
        IReadOnlyList<TranslationHit> hits = TranslationSearch.Find("most  merciful", Wordness.Any, [(english.Key, _engine.AllTranslationText(english))]);
        Assert.Contains(hits, h => h.VerseNumber == 1);
    }

    [Fact]
    public void TheStandardSpellingFindsWhatTheUthmaniMisses()
    {
        Assert.Equal(0, _engine.Search().Find("الكتاب", Wordness.WholeWord).VerseCount);
        Assert.NotEmpty(_engine.EmlaaeiSearch("الكتاب", Wordness.WholeWord));
        Assert.NotEmpty(_classic.EmlaaeiSearch("الكتاب", Wordness.WholeWord));
    }

    [Fact]
    public void WordsHaveMeaningsTransliterationAndGrammar()
    {
        WordData bism = _engine.WordDataOf(1, 0)!;
        Assert.Equal("In (the) name", bism.Meaning);
        Assert.Equal("Bismi", bism.Transliteration);
        Assert.Equal(["PP", "N"], bism.Parts.Select(p => p.Tag));
        Assert.Equal("Noun", _engine.GrammarLabel("N", "en"));

        // Verse 0 of chapter 2 is the Bismillah, and has 1:1's grammar.
        int zero = _engine.Verse(2, 0).Number;
        Assert.Equal(bism.Parts.Select(p => p.Form), _engine.WordDataOf(zero, 0)!.Parts.Select(p => p.Form));
        Assert.Equal("INL", _engine.WordDataOf(_engine.Verse(2, 1).Number, 0)!.Parts[0].Tag);
    }

    [Fact]
    public void BuckwalterBecomesArabic()
    {
        Assert.Equal("بِ", Buckwalter.ToArabic("bi"));
        Assert.Equal("الٓمٓ", Buckwalter.ToArabic("Al^m^"));
    }
}
