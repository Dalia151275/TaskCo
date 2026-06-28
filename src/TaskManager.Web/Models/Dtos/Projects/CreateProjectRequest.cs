namespace TaskManager.Web.Models.Dtos.Projects;

public sealed record CreateProjectRequest(string Name, string? Description = null, DateTimeOffset? DueDate = null);
