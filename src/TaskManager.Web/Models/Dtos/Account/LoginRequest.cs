namespace TaskManager.Web.Models.Dtos.Account;

public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);
