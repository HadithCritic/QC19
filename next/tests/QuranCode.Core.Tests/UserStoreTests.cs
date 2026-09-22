using QuranCode.Core.User;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class UserStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"qurancode-user-{Guid.NewGuid():N}.db");
    private readonly UserStore _store;

    public UserStoreTests() => _store = new UserStore(_path);

    public void Dispose()
    {
        _store.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (string file in new[] { _path, _path + "-wal", _path + "-shm" })
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }

    [Fact]
    public void SavesAndUpdatesABookmarkByRange()
    {
        Bookmark first = _store.SaveBookmark(new(2, 255), new(2, 255), "Ayat al-Kursi");
        Bookmark again = _store.SaveBookmark(new(2, 255), new(2, 255), "11261");

        Assert.Equal(first.Id, again.Id);
        Bookmark stored = Assert.Single(_store.Bookmarks());
        Assert.Equal("11261", stored.Note);
        Assert.Equal(first.CreatedUtc, stored.CreatedUtc);
    }

    [Fact]
    public void DeletesABookmark()
    {
        Bookmark bookmark = _store.SaveBookmark(new(1, 1), new(1, 7), "");
        Assert.True(_store.DeleteBookmark(bookmark.Id));
        Assert.False(_store.DeleteBookmark(bookmark.Id));
        Assert.Empty(_store.Bookmarks());
    }

    [Fact]
    public void RejectsAnOverlongNote() =>
        Assert.Throws<ArgumentException>(() => _store.SaveBookmark(new(1, 1), new(1, 1), new string('x', UserStore.MaxNoteLength + 1)));

    [Fact]
    public void HistoryIsNewestFirstAndSkipsRepeats()
    {
        _store.AddBrowse(new(1, 1), new(1, 7));
        _store.AddBrowse(new(1, 1), new(1, 7));
        _store.AddBrowse(new(2, 255), new(2, 255));
        _store.AddFind("الله", "any");
        _store.AddFind("الله", "any");

        IReadOnlyList<HistoryEntry> browse = _store.History(HistoryKind.Browse, 10);
        Assert.Equal(2, browse.Count);
        Assert.Equal(new VerseRef(2, 255), browse[0].First);
        Assert.Single(_store.History(HistoryKind.Find, 10));
    }

    [Fact]
    public void HistoryIsTrimmedToItsLimit()
    {
        for (int i = 1; i <= UserStore.HistoryLimit + 20; i++) _store.AddFind($"term {i}", "any");
        IReadOnlyList<HistoryEntry> find = _store.History(HistoryKind.Find, UserStore.HistoryLimit);
        Assert.Equal(UserStore.HistoryLimit, find.Count);
        Assert.Equal($"term {UserStore.HistoryLimit + 20}", find[0].Term);
    }

    [Fact]
    public void ClearsOneKindOnly()
    {
        _store.AddBrowse(new(1, 1), new(1, 1));
        _store.AddFind("x", "any");
        _store.ClearHistory(HistoryKind.Find);
        Assert.Empty(_store.History(HistoryKind.Find, 10));
        Assert.Single(_store.History(HistoryKind.Browse, 10));
    }

    [Fact]
    public void DataSurvivesReopening()
    {
        _store.SaveBookmark(new(36, 1), new(36, 83), "Ya-Sin");
        using var reopened = new UserStore(_path);
        Assert.Equal("Ya-Sin", Assert.Single(reopened.Bookmarks()).Note);
    }
}
