using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using Microsoft.Identity.Client;

namespace ProjectHub.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required string Status { get; set; }

        public required string Priority { get; set; }

        public DateOnly DueDate { get; set; }

        // Corrected the array declaration
        public string[]? Assignee { get; set; }
    }

}