namespace TaskManager.Web.Services.Abstractions;

public interface ICurrentUser
{
    string UserId { get; }
    bool IsAuthenticated { get; }
}
