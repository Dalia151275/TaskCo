using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IProjectService _projects;
    private readonly ITaskItemService _tasks;

    // Assumption: spec says IList<Project> (entity) but CLAUDE.md prohibits entities in pages;
    // using ProjectSummary (wraps ProjectResponse) so all data comes through the service layer.
    public IReadOnlyList<ProjectSummary> Projects { get; private set; } = [];

    public int TotalTasks { get; private set; }
    public int OverdueTasks { get; private set; }
    public int CompletedTasks { get; private set; }
    public int InProgressTasks { get; private set; }

    public DashboardModel(IProjectService projects, ITaskItemService tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public async Task OnGetAsync()
    {
        // TODO: replace per-project ListAsync calls with a single bulk query, e.g.:
        // var stats = await _tasks.GetDashboardStatsAsync();
        var projectsResult = await _projects.ListAsync();
        if (!projectsResult.IsSuccess) return;

        var now = DateTimeOffset.UtcNow;
        var summaries = new List<ProjectSummary>();

        foreach (var project in projectsResult.Value!)
        {
            // TODO: replace with a bulk service method to avoid N+1, e.g.:
            // var taskStats = await _tasks.GetProjectStatsAsync(project.Id);
            var tasksResult = await _tasks.ListAsync(project.Id);
            var tasks = tasksResult.IsSuccess
                ? tasksResult.Value!
                : (IReadOnlyList<TaskItemResponse>)[];

            var done = tasks.Count(t => t.Status == TaskStatus.Done);
            var inProgress = tasks.Count(t => t.Status == TaskStatus.InProgress);
            var overdue = tasks.Count(t => t.DueDate < now && t.Status != TaskStatus.Done);

            TotalTasks += tasks.Count;
            CompletedTasks += done;
            InProgressTasks += inProgress;
            OverdueTasks += overdue;

            summaries.Add(new ProjectSummary(project, tasks.Count, done, overdue));
        }

        Projects = summaries.AsReadOnly();
    }

    public sealed record ProjectSummary(
        ProjectResponse Project,
        int TaskCount,
        int DoneCount,
        int OverdueCount)
    {
        public int ProgressPercent => TaskCount == 0
            ? 0
            : (int)Math.Round(DoneCount * 100.0 / TaskCount);
    }
}
