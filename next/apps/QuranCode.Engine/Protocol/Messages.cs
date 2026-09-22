using System.Text.Json;

namespace QuranCode.Engine.Protocol;

/// <summary>One request line: <c>{"id": 7, "method": "verse.value", "params": {...}}</c>.</summary>
/// <remarks><c>params</c> may be omitted for methods that take none.</remarks>
internal sealed record Request(long Id, string Method, JsonElement? Params = null);

/// <summary>The closed set of error codes the UI can branch on.</summary>
internal static class ErrorCodes
{
    /// <summary>The line was not a valid request.</summary>
    public const string ParseError = "parse_error";

    /// <summary>The method exists but its parameters are missing or out of range.</summary>
    public const string InvalidParams = "invalid_params";

    /// <summary>A named thing (value system, chapter) does not exist.</summary>
    public const string NotFound = "not_found";

    public const string UnknownMethod = "unknown_method";

    /// <summary>A bug. Details go to stderr, never to the UI.</summary>
    public const string Internal = "internal";
}

internal sealed record RpcError(string Code, string Message);

/// <summary>A failure a handler reports on purpose, with a message fit for the UI.</summary>
internal sealed class RpcException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;

    public static RpcException InvalidParams(string message) => new(ErrorCodes.InvalidParams, message);

    public static RpcException NotFound(string message) => new(ErrorCodes.NotFound, message);
}
