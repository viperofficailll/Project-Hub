
// Create this file in: ViewModels/CreateOrJoinViewModel.cs

using ProjectHub.Models;

namespace ProjectHub.ViewModels
{
    public class CreateOrJoinViewModel
    {
        public List<Project> OwnedProjects { get; set; } = new();
        public List<Project> MemberProjects { get; set; } = new();
        public string? UserEmail { get; set; }
    }
}