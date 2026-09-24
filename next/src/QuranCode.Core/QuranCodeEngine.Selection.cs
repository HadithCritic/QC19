using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core;

/// <summary>Exact selections, from a chapter down to a letter (docs/specs/research-selection.md).</summary>
public sealed partial class QuranCodeEngine
{
    /// <summary>
    /// The counted text a selection covers under a text mode and options, or why
    /// it cannot be resolved.
    /// </summary>
    public SelectionResolution Resolve(
        QuranSelection selection, string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        CountingOptions effective = Effective(textMode, counting);
        var resolver = new SelectionResolver(
            Chapters, Verses, View(effective), Segmentation(textMode, effective),
            Display, CountingText(textMode, effective).NormalizeWord, textMode);
        return resolver.Resolve(selection);
    }
}
