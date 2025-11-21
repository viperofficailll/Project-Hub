using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProjectHub.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        public required string ProjectName { get; set; }
        // Removed [Required] here - it's set server-side
        [StringLength(50, ErrorMessage = "Owner name cannot exceed 50 characters")]
        public string? Owner { get; set; }
        [Required(ErrorMessage = "Project type is required")]
        [StringLength(50, ErrorMessage = "Project type cannot exceed 50 characters")]
        public required string ProjectType { get; set; }
        // Optional fields
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        // Timestamps (similar to User class)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<User> Members { get; set; } = new List<User>();
        public string? EntryToken { get; set; }
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}