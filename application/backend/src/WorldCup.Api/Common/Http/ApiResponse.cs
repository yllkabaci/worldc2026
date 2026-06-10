namespace WorldCup.Api.Common.Http;

/// <summary>Uniform success envelope for API payloads. Failures use RFC 7807 ProblemDetails, never this envelope.</summary>
public sealed record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public required T Data { get; init; }
}

public static class ApiResponseExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this T data) => new() { Data = data };

    public static ApiResponse<IReadOnlyCollection<T>> ToApiListResponse<T>(this IEnumerable<T> data) =>
        new() { Data = data.ToList() };
}
