using QuranCode.Core.Content;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>Translations and the other verse texts (Features.txt #27).</summary>
internal sealed partial class Handlers
{
    /// <summary>Texts asked for at once.</summary>
    public const int MaxTranslationKeys = 20;

    /// <summary>Verses per request: a long chapter and some to spare.</summary>
    public const int MaxTranslationVerses = 700;

    /// <summary>A word's gloss, transliteration, roots and grammar (Features.txt #62, #64).</summary>
    public WordInfoDto WordInfo(WordParams p)
    {
        if (p.Verse < 1 || p.Verse > _engine.Verses.Count) throw RpcException.InvalidParams($"Verses run from 1 to {_engine.Verses.Count}.");
        Verse verse = _engine.Verse(p.Verse);
        string[] words = DisplayWords.Split(verse.Text);
        if (p.Word < 0 || p.Word >= words.Length) throw RpcException.InvalidParams($"This verse has words 0 to {words.Length - 1}.");

        WordData? data = _engine.WordDataOf(p.Verse, p.Word);
        string[] roots = _engine.Roots.Of(p.Verse, p.Word).Select(_engine.Roots.Text).ToArray();
        WordPartDto[] parts = (data?.Parts ?? []).Select(part => new WordPartDto(
            part.Part, Buckwalter.ToArabic(part.Form), part.Form, part.Tag,
            _engine.GrammarLabel(part.Tag, "en"), _engine.GrammarLabel(part.Tag, "ar"),
            part.Features.Select(Feature).ToArray())).ToArray();

        return new WordInfoDto(p.Verse, p.Word, words[p.Word],
            string.IsNullOrEmpty(data?.Meaning) ? null : data.Meaning,
            string.IsNullOrEmpty(data?.Transliteration) ? null : data.Transliteration,
            roots, parts);
    }

    /// <summary>A feature with its names: POS:N is named for N; a lemma or root is given in Arabic too.</summary>
    private FeatureDto Feature(string feature)
    {
        int colon = feature.IndexOf(':');
        string key = colon < 0 ? feature : feature[..colon];
        string value = colon < 0 ? "" : feature[(colon + 1)..];
        return key switch
        {
            "POS" => new FeatureDto(feature, _engine.GrammarLabel(value, "en"), _engine.GrammarLabel(value, "ar"), null),
            "LEM" or "ROOT" or "SP" => new FeatureDto(
                feature, key == "ROOT" ? "Root" : key == "LEM" ? "Lemma" : "Special", null, Buckwalter.ToArabic(value)),
            _ => new FeatureDto(feature, _engine.GrammarLabel(feature, "en"), _engine.GrammarLabel(feature, "ar"), null),
        };
    }

    public IReadOnlyList<TranslationDto> TranslationList() => _engine.Translations
        .Select(t => new TranslationDto(t.Key, t.Language, t.Name, t.Translator, t.Kind, t.RightToLeft, t.Source != 0))
        .ToArray();

    public IReadOnlyList<TranslationTextDto> TranslationText(TranslationTextParams p)
    {
        if (p.Keys is null || p.Keys.Count == 0) throw RpcException.InvalidParams("keys is empty.");
        if (p.Keys.Count > MaxTranslationKeys) throw RpcException.InvalidParams($"At most {MaxTranslationKeys} texts at once.");
        VerseRange range = RequireRange(p.First, p.Last);
        if (range.Last - range.First + 1 > MaxTranslationVerses)
        {
            throw RpcException.InvalidParams($"At most {MaxTranslationVerses} verses at once.");
        }

        return p.Keys.Distinct(StringComparer.Ordinal).Select(key =>
        {
            TranslationInfo translation = _engine.Translation(key)
                ?? throw RpcException.NotFound($"There is no translation named \"{Truncate(key)}\".");
            IReadOnlyDictionary<int, string> text = _engine.TranslationText(translation, range.First, range.Last);
            return new TranslationTextDto(key, text.OrderBy(t => t.Key).Select(t => new VerseTextDto(t.Key, t.Value)).ToArray());
        }).ToArray();
    }
}
