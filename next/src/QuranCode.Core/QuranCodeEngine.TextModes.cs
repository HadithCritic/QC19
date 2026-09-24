using QuranCode.Core.Text;

namespace QuranCode.Core;

public sealed partial class QuranCodeEngine
{
    private readonly Dictionary<string, DerivedTextMode> _derived = new(StringComparer.Ordinal);

    /// <summary>The derived text modes the reader has defined.</summary>
    public IReadOnlyList<DerivedTextMode> DerivedTextModes => [.. _derived.Values];

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
    }

    /// <summary>Removes a derived text mode.</summary>
    public bool RemoveTextMode(string name)
    {
        if (!_derived.Remove(name)) return false;
        Forget(name);
        return true;
    }

    /// <summary>The stock mode a text mode stands on: its base when it is derived, else itself.</summary>
    public string BaseOf(string textMode) =>
        _derived.TryGetValue(textMode, out DerivedTextMode? mode) ? mode.Base : textMode;

    /// <summary>Whether the name is a stock or a derived text mode.</summary>
    public bool HasTextMode(string textMode) =>
        _derived.ContainsKey(textMode) || DerivedTextMode.StockModes.Contains(textMode, StringComparer.Ordinal);

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
    }
}
