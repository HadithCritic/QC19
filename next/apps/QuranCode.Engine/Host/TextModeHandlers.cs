using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

internal sealed partial class Handlers
{
    /// <summary>The reader's text modes, and the stock modes this edition has value systems for.</summary>
    public TextModesDto TextModes()
    {
        var installed = _engine.ValueSystemSummaries().Select(s => s.TextMode).ToHashSet(StringComparer.Ordinal);
        return new TextModesDto(
            [.. DerivedTextMode.StockModes.Where(installed.Contains)],
            [.. _engine.DerivedTextModes.OrderBy(m => m.Name, StringComparer.Ordinal).Select(ToDto)]);
    }

    internal static TextModeDto ToDto(DerivedTextMode mode) =>
        new(mode.Name, mode.Base, [.. mode.Rules.Select(r => new TextRuleDto(r.Find, r.Replace))], mode.Description);
}
