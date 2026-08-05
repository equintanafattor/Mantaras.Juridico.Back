namespace Mantaras.Juridico.Api.Contracts;

public sealed class ApiErrorResponse
{
    public required IReadOnlyCollection<ApiErrorItem> Errors { get; init; }
}

public sealed class ApiErrorItem
{
    public required string Code { get; init; }

    public required string Message { get; init; }
}
