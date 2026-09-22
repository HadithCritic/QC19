using System.Threading;
using QuranCode.Core;

namespace QuranCode.Desktop.Services;

/// <summary>
/// Owns the one <see cref="QuranCodeEngine"/> the application uses.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately a small, explicit object rather than a static accessor. The
/// legacy application reached a static <c>Server</c> from anywhere, which is
/// what made its state impossible to reason about; here the engine is
/// constructed once, handed to the view models that need it, and disposed with
/// the application.
/// </para>
/// <para>
/// Opening is deferred until something asks. Constructing the service touches no
/// files, so the window can render before the database is opened and startup is
/// never blocked on data a screen may not use.
/// </para>
/// </remarks>
public sealed class EngineService : IDisposable
{
    private readonly Lock _gate = new();
    private QuranCodeEngine? _engine;
    private string? _failure;

    /// <summary>Where the content database was found, once opened.</summary>
    public string? DatabasePath { get; private set; }

    /// <summary>Why the engine could not be opened, if it could not.</summary>
    public string? Failure
    {
        get { lock (_gate) return _failure; }
    }

    /// <summary>
    /// The engine, opened on first use.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No content database could be located. The message says how to build one,
    /// because a missing generated file is a setup step, not a crash.
    /// </exception>
    public QuranCodeEngine Engine
    {
        get
        {
            lock (_gate)
            {
                if (_engine is not null) return _engine;
                if (_failure is not null) throw new InvalidOperationException(_failure);

                string? path = Locate();
                if (path is null)
                {
                    _failure =
                        "content.db was not found. Build it with:\n" +
                        "  python next/data/import/build_content.py <install-root> -o next/data/content.db";
                    throw new InvalidOperationException(_failure);
                }

                DatabasePath = path;
                _engine = new QuranCodeEngine(path);
                return _engine;
            }
        }
    }

    /// <summary>Whether the engine can be opened, without throwing if it cannot.</summary>
    public bool TryOpen()
    {
        try
        {
            _ = Engine;
            return true;
        }
        catch (Exception ex)
        {
            lock (_gate) _failure ??= ex.Message;
            return false;
        }
    }

    private static string? Locate()
    {
        foreach (string candidate in new[]
        {
            Path.Combine(Environment.CurrentDirectory, "content.db"),
            Path.Combine(AppContext.BaseDirectory, "content.db"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "content.db"),
        })
        {
            if (File.Exists(candidate)) return Path.GetFullPath(candidate);
        }
        return null;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}
