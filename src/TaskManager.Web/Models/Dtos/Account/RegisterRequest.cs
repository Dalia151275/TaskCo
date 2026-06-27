namespace TaskManager.Web.Models.Dtos.Account;

public sealed record RegisterRequest(string Email, string Password, string ConfirmPassword);
