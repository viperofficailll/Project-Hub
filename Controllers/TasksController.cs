using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;
using ProjectHub.ViewModels;

namespace ProjectHub.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:int}/tasks")]
    public class TasksController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTasks(int projectId)
        {
            var authorizationResult = await EnsureProjectAccessAsync(projectId);
            if (authorizationResult.ErrorResult is IActionResult error)
            {
                return error;
            }

            var tasks = await context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return Ok(tasks.Select(MapToDto).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] TaskItemDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authorizationResult = await EnsureProjectAccessAsync(projectId);
            if (authorizationResult.ErrorResult is IActionResult error)
            {
                return error;
            }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                DueDate = ParseDate(request.DueDate),
                Assignees = request.Assignees?.ToList() ?? new List<string>(),
                ProjectId = projectId
            };

            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { projectId }, MapToDto(task));
        }

        [HttpPut("{taskId:int}")]
        public async Task<IActionResult> UpdateTask(int projectId, int taskId, [FromBody] TaskItemDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authorizationResult = await EnsureProjectAccessAsync(projectId);
            if (authorizationResult.ErrorResult is IActionResult error)
            {
                return error;
            }

            var task = await context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
            if (task == null)
            {
                return NotFound();
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = ParseDate(request.DueDate);
            task.Assignees = request.Assignees?.ToList() ?? new List<string>();

            await context.SaveChangesAsync();

            return Ok(MapToDto(task));
        }

        [HttpDelete("{taskId:int}")]
        public async Task<IActionResult> DeleteTask(int projectId, int taskId)
        {
            var authorizationResult = await EnsureProjectAccessAsync(projectId);
            if (authorizationResult.ErrorResult is IActionResult error)
            {
                return error;
            }

            var task = await context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
            if (task == null)
            {
                return NotFound();
            }

            context.TaskItems.Remove(task);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<(Project? Project, IActionResult? ErrorResult)> EnsureProjectAccessAsync(int projectId)
        {
            var email = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(email))
            {
                return (null, Unauthorized());
            }

            var project = await context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return (null, NotFound());
            }

            var hasAccess = project.Owner == email || project.Members.Any(m => m.UserEmail == email);
            if (!hasAccess)
            {
                return (null, Forbid());
            }

            return (project, null);
        }

        private static TaskItemDto MapToDto(TaskItem task)
        {
            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate?.ToString("yyyy-MM-dd"),
                Assignees = task.Assignees?.ToList() ?? new List<string>()
            };
        }

        private static DateOnly? ParseDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return null;
            }

            return DateOnly.TryParse(date, out var parsed) ? parsed : null;
        }
    }
}

