using System.Globalization;
using QuranCode.Core;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using QuranCode.Core.User;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Bookmarks, history and text modes over the reader's <c>user.db</c>. Positions travel
/// as absolute verse numbers of the open edition and are stored as
/// chapter:verse.
/// </summary>
internal sealed class UserHandlers
{
    private readonly QuranCodeEngine _engine;
    private readonly UserStore _store;

    public UserHandlers(QuranCodeEngine engine, UserStore store)
    {
        _engine = engine;
        _store = store;

        // The reader's text modes are the engine's from the start. One that
        // is no longer sound (its base gone from this edition) is left in the
        // file, unused, rather than deleted.
        foreach (DerivedTextMode mode in _store.TextModes())
        {
            if (mode.Problem() is null && _engine.HasTextMode(mode.Base)) _engine.DefineTextMode(mode);
        }
    }

    public TextModeDto SaveTextMode(TextModeDto p)
    {
        var mode = new DerivedTextMode(
            p.Name ?? "", p.Base ?? "", [.. (p.Rules ?? []).Select(r => new TextRule(r.Find ?? "", r.Replace ?? ""))], p.Description ?? "");
        if (mode.Problem() is { } problem) throw RpcException.InvalidParams(problem);
        if (mode.Description.Length > UserStore.MaxNoteLength)
        {
            throw RpcException.InvalidParams($"A description is at most {UserStore.MaxNoteLength:N0} characters.");
        }

        _engine.DefineTextMode(mode);
        _store.SaveTextMode(mode);
        return Handlers.ToDto(mode);
    }

    public bool DeleteTextMode(NameParams p)
    {
        bool removed = _store.DeleteTextMode(p.Name);
        return _engine.RemoveTextMode(p.Name) || removed;
    }

    public IReadOnlyList<BookmarkDto> Bookmarks() => _store.Bookmarks().Select(ToDto).ToArray();

    public BookmarkDto SaveBookmark(BookmarkSaveParams p)
    {
        (VerseRef first, VerseRef last) = Refs(p.First, p.Last);
        if (p.Note.Length > UserStore.MaxNoteLength)
        {
            throw RpcException.InvalidParams($"A note is at most {UserStore.MaxNoteLength:N0} characters.");
        }
        return ToDto(_store.SaveBookmark(first, last, p.Note));
    }

    public bool DeleteBookmark(IdParams p) => _store.DeleteBookmark(p.Id);

    public IReadOnlyList<HistoryDto> History(HistoryListParams p) =>
        _store.History(ParseKind(p.Kind), p.Limit).Select(ToDto).ToArray();

    public bool AddHistory(HistoryAddParams p)
    {
        switch (ParseKind(p.Kind))
        {
            case HistoryKind.Browse:
                if (p.First is not int first || p.Last is not int last)
                {
                    throw RpcException.InvalidParams("A browse entry needs first and last.");
                }
                (VerseRef a, VerseRef b) = Refs(first, last);
                _store.AddBrowse(a, b);
                return true;

            default:
                if (string.IsNullOrWhiteSpace(p.Term)) throw RpcException.InvalidParams("A find entry needs a term.");
                if (p.Term.Length > UserStore.MaxTermLength)
                {
                    throw RpcException.InvalidParams($"A search is at most {UserStore.MaxTermLength} characters.");
                }
                _store.AddFind(p.Term, p.Wordness ?? "any");
                return true;
        }
    }

    public bool ClearHistory(HistoryClearParams p)
    {
        _store.ClearHistory(ParseKind(p.Kind));
        return true;
    }

    private static HistoryKind ParseKind(string kind) => kind switch
    {
        "browse" => HistoryKind.Browse,
        "find" => HistoryKind.Find,
        _ => throw RpcException.InvalidParams("kind must be browse or find."),
    };

    private (VerseRef First, VerseRef Last) Refs(int first, int last)
    {
        int count = _engine.Verses.Count;
        if (first < 1 || last > count || last < first)
        {
            throw RpcException.InvalidParams($"A range must satisfy 1 <= first <= last <= {count}.");
        }
        return (RefOf(first), RefOf(last));
    }

    private VerseRef RefOf(int absolute)
    {
        Verse verse = _engine.Verse(absolute);
        return new VerseRef(verse.ChapterNumber, verse.NumberInChapter);
    }

    /// <summary>The absolute number of a stored chapter:verse in this edition, or null if it has none.</summary>
    private int? AbsoluteOf(VerseRef reference)
    {
        if (reference.Chapter < 1 || reference.Chapter > _engine.Chapters.Count) return null;
        Chapter chapter = _engine.Chapters[reference.Chapter - 1];
        return reference.Verse >= chapter.FirstNumberInChapter && reference.Verse <= chapter.VerseCount
            ? chapter.AbsoluteOf(reference.Verse)
            : null;
    }

    private static string Text(VerseRef first, VerseRef last) =>
        first == last ? $"{first.Chapter}:{first.Verse}"
        : first.Chapter == last.Chapter ? $"{first.Chapter}:{first.Verse}-{last.Verse}"
        : $"{first.Chapter}:{first.Verse}-{last.Chapter}:{last.Verse}";

    private static string Time(DateTime utc) => utc.ToString("O", CultureInfo.InvariantCulture);

    private BookmarkDto ToDto(Bookmark b) =>
        new(b.Id, Text(b.First, b.Last), AbsoluteOf(b.First), AbsoluteOf(b.Last), b.Note, Time(b.CreatedUtc), Time(b.UpdatedUtc));

    private HistoryDto ToDto(HistoryEntry h) => new(
        h.Id,
        h.Kind == HistoryKind.Browse ? "browse" : "find",
        h.First is VerseRef f && h.Last is VerseRef l ? Text(f, l) : null,
        h.First is VerseRef first ? AbsoluteOf(first) : null,
        h.Last is VerseRef last ? AbsoluteOf(last) : null,
        h.Term, h.Wordness, Time(h.AtUtc));
}
