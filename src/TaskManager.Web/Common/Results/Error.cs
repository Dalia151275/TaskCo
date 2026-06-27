namespace TaskManager.Web.Common.Results;

public sealed record Error(string Code, string Message, object? Details = null);

public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Validation = "VALIDATION";
    public const string Unauthenticated = "UNAUTHENTICATED";
    public const string Conflict = "CONFLICT";
}
