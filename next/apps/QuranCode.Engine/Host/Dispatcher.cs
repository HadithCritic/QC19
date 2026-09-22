using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Turns one request line into one response line. Pure with respect to I/O,
/// so the whole protocol is testable without a process.
/// </summary>
internal sealed class Dispatcher
{
    private delegate void Method(JsonElement? parameters, Utf8JsonWriter writer);

    private readonly Dictionary<string, Method> _methods;
    private readonly TextWriter _log;

    public Dispatcher(Handlers handlers, TextWriter log)
    {
        _log = log;
        WireJson json = WireJson.Default;

        _methods = new Dictionary<string, Method>(StringComparer.Ordinal)
        {
            ["engine.info"] = NoParams(handlers.Info, json.EngineInfo),
            ["chapters.list"] = NoParams(handlers.Chapters, json.IReadOnlyListChapterDto),
            ["systems.list"] = NoParams(handlers.ValueSystems, json.IReadOnlyListValueSystemDto),
            ["chapter.verses"] = With(json.ChapterParams, handlers.ChapterVerses, json.IReadOnlyListVerseDto),
            ["chapter.values"] = With(json.ChapterValuesParams, handlers.ChapterValues, json.IReadOnlyListVerseValueDto),
            ["selection.stats"] = With(json.RangeParams, handlers.Stats, json.StatsDto),
            ["reference.parse"] = With(json.ReferenceParams, handlers.ParseReference, json.RangeDto),
            ["number.analyze"] = With(json.NumberParams, handlers.AnalyzeNumber, json.NumberDto),
            ["text.values"] = With(json.TextValuesParams, handlers.TextValues, json.IReadOnlyListSystemValueDto),
            ["search.text"] = With(json.SearchParams, handlers.Search, json.SearchResultDto),
        };
    }

    public IReadOnlyCollection<string> MethodNames => _methods.Keys;

    public string Handle(string line)
    {
        Request? request;
        try
        {
            request = JsonSerializer.Deserialize(line, WireJson.Default.Request);
        }
        catch (JsonException ex)
        {
            return Error(null, ErrorCodes.ParseError, $"The request is not valid JSON: {ex.Message}");
        }

        if (request is null || string.IsNullOrEmpty(request.Method))
        {
            return Error(request?.Id, ErrorCodes.ParseError, "The request has no method.");
        }

        if (!_methods.TryGetValue(request.Method, out Method? method))
        {
            return Error(request.Id, ErrorCodes.UnknownMethod, $"There is no method named \"{request.Method}\".");
        }

        try
        {
            return Write(request.Id, writer =>
            {
                writer.WritePropertyName("result");
                method(request.Params, writer);
            });
        }
        catch (RpcException ex)
        {
            return Error(request.Id, ex.Code, ex.Message);
        }
        catch (JsonException ex)
        {
            return Error(request.Id, ErrorCodes.InvalidParams, $"The parameters are not valid: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Details stay on stderr. The UI gets a sentence, not a stack trace.
            _log.WriteLine($"[{request.Method} #{request.Id}] {ex}");
            return Error(request.Id, ErrorCodes.Internal, "The engine hit an unexpected error. Details are in the log.");
        }
    }

    private static Method NoParams<TResult>(Func<TResult> handler, JsonTypeInfo<TResult> result) =>
        (_, writer) => JsonSerializer.Serialize(writer, handler(), result);

    private static Method With<TParams, TResult>(
        JsonTypeInfo<TParams> parameters, Func<TParams, TResult> handler, JsonTypeInfo<TResult> result) =>
        (element, writer) =>
        {
            if (element is not { ValueKind: JsonValueKind.Object } value)
            {
                throw RpcException.InvalidParams("This method needs a params object.");
            }
            TParams typed = value.Deserialize(parameters)
                ?? throw RpcException.InvalidParams("This method needs a params object.");
            JsonSerializer.Serialize(writer, handler(typed), result);
        };

    private static string Error(long? id, string code, string message) => Write(id, writer =>
    {
        writer.WritePropertyName("error");
        JsonSerializer.Serialize(writer, new RpcError(code, message), WireJson.Default.RpcError);
    });

    /// <summary>
    /// Arabic is written as UTF-8 rather than as escape sequences, which would
    /// triple the size of every verse. Safe because the output goes to the UI
    /// over a pipe and is never inserted into HTML unescaped.
    /// </summary>
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static string Write(long? id, Action<Utf8JsonWriter> body)
    {
        // The body is written into a buffer before anything is returned, so a
        // handler that throws halfway leaves no partial line behind.
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            writer.WriteStartObject();
            if (id is long value) writer.WriteNumber("id", value);
            else writer.WriteNull("id");
            body(writer);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}
