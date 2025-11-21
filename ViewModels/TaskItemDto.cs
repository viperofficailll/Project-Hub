using System.Collections.Generic;

namespace ProjectHub.ViewModels
{
    public class TaskItemDto
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required string Status { get; set; }

        public required string Priority { get; set; }

        public string? DueDate { get; set; }

        public List<string> Assignees { get; set; } = new();
    }
}

