namespace ServicoPalavra.Application.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message, IReadOnlyList<string>? Errors)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message, null);
    public static ApiResponse<T> Fail(string error) => new(false, default, null, [error]);
}

public sealed record ApiResponse(bool Success, string? Message, IReadOnlyList<string>? Errors)
{
    public static ApiResponse Ok(string? message = null) => new(true, message, null);
    public static ApiResponse Fail(string error) => new(false, null, [error]);
}
