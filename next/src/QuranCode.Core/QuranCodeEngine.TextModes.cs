using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

namespace QuranCode.Core;

public sealed partial class QuranCodeEngine
{
    private readonly Dictionary<string, DerivedTextMode> _derived = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ValueSystem> _derivedSystems = new(StringComparer.Ordinal);

    /// <summary>The derived text modes the reader has defined.</summary>
    public IReadOnlyList<DerivedTextMode> DerivedTextModes => [.. _derived.Values];

    /// <summary>Changes whenever a derived text mode is defined or removed, so a cache of the value systems can tell it is stale.</summary>
    public int TextModesVersion { get; private set; }

    /// <summary>
    /// Adds a derived text mode, or replaces one the reader defined earlier
    /// under the same name.
    /// </summary>
    /// <exception cref="ArgumentException">The definition is not sound.</exception>
    public void DefineTextMode(DerivedTextMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        if (mode.Problem() is { } problem) throw new ArgumentException(problem, nameof(mode));

        _derived[mode.Name] = mode;
        Forget(mode.Name);
        TextModesVersion++;
    }

    /// <summary>Removes a derived text mode.</summary>
    public bool RemoveTextMode(string name)
    {
        if (!_derived.Remove(name)) return false;
        Forget(name);
        TextModesVersion++;
        return true;
    }

    /// <summary>The stock mode a text mode stands on: its base when it is derived, else itself.</summary>
    public string BaseOf(string textMode) =>
        _derived.TryGetValue(textMode, out DerivedTextMode? mode) ? mode.Base : textMode;

    /// <summary>Whether the name is a stock or a derived text mode.</summary>
    public bool HasTextMode(string textMode) =>
        _derived.ContainsKey(textMode) || DerivedTextMode.StockModes.Contains(textMode, StringComparer.Ordinal);

    /// <summary>
    /// A derived mode has every value system its base has, under its own
    /// name: <c>TaaAsHaa_Alphabet_Primes1</c> values letters as
    /// <c>Simplified30_Alphabet_Primes1</c> does. Its letter stage is the
    /// base's, so the letters it leaves are the base's letters.
    /// </summary>
    private IEnumerable<ValueSystemSummary> DerivedSystemSummaries() =>
        _derived.Count == 0
            ? []
            : _content.ValueSystemSummaries()
                .SelectMany(s => _derived.Values
                    .Where(m => m.Base == s.TextMode)
                    .Select(m => s with { Name = m.Name + s.Name[s.TextMode.Length..], TextMode = m.Name }))
                .OrderBy(s => s.Name, StringComparer.Ordinal);

    private ValueSystem? DerivedSystem(string name)
    {
        if (_derivedSystems.TryGetValue(name, out ValueSystem? cached)) return cached;
        int split = name.IndexOf('_', StringComparison.Ordinal);
        if (split < 0 || !_derived.TryGetValue(name[..split], out DerivedTextMode? mode)) return null;

        ValueSystem stock = _content.GetValueSystem(mode.Base + name[split..]);
        var system = new ValueSystem(name, mode.Name, stock.LetterOrder, stock.LetterValue, stock.Values);
        _derivedSystems[name] = system;
        return system;
    }

    private TextPipeline PipelineOf(string textMode)
    {
        if (!_derived.TryGetValue(textMode, out DerivedTextMode? mode)) return new(_content.GetTextMode(textMode));

        // The base's rules first, so its word joins see the text as written;
        // then the mode's own; then the base's letter stage.
        TextMode stock = _content.GetTextMode(mode.Base);
        var rules = new List<(string Find, string Replace)>(stock.Rules);
        rules.AddRange(mode.Rules.Select(r => (r.Find, r.Replace)));
        return new TextPipeline(new TextMode(mode.Name, stock.WordCountMethod, rules), mode.Base);
    }

    /// <summary>Drops everything built for a text mode, after it is defined, changed or removed.</summary>
    private void Forget(string textMode)
    {
        foreach (var key in _segmentations.Keys.Where(k => k.Item1 == textMode).ToArray()) _segmentations.Remove(key);
        foreach (var key in _searches.Keys.Where(k => k.Item1 == textMode).ToArray()) _searches.Remove(key);
        foreach (var key in _countingTexts.Keys.Where(k => k.Item1 == textMode).ToArray()) _countingTexts.Remove(key);
        foreach (var key in _similarities.Keys.Where(k => k.Item1 == textMode).ToArray()) _similarities.Remove(key);

        // Value systems are named after their text mode.
        string systems = textMode + "_";
        foreach (var key in _derivedSystems.Keys.Where(k => k.StartsWith(systems, StringComparison.Ordinal)).ToArray())
            _derivedSystems.Remove(key);
        foreach (var key in _wordValues.Keys.Where(k => k.Item1.StartsWith(systems, StringComparison.Ordinal)).ToArray())
            _wordValues.Remove(key);
        foreach (var key in _numberSearches.Keys.Where(k => k.Item1.StartsWith(systems, StringComparison.Ordinal)).ToArray())
            _numberSearches.Remove(key);
    }
}
