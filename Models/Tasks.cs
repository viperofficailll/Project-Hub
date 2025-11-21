using System;
using System.Collections.Generic;

namespace ProjectHub.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required string Status { get; set; }

        public required string Priority { get; set; }

        public DateOnly? DueDate { get; set; }

        public List<string> Assignees { get; set; } = new();

        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;
    }

}