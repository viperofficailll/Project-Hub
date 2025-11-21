using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;
using ProjectHub.ViewModels;

namespace ProjectHub.Controllers
{
    public class ProjectController(AppDbContext context) : Controller
    {
        public IActionResult CreateOrJoin()
        {
            var email = HttpContext.Session.GetString("email");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Home");

            // Get projects where user is the owner
            var ownedProjects = context.Projects
                .Include(p => p.Members)
                .Where(p => p.Owner == email)
                .OrderByDescending(p => p.UpdatedAt)
                .ToList();

            // Get projects where user is a member (but not owner)
            var memberProjects = context.Projects
                .Include(p => p.Members)
                .Where(p => p.Members.Any(m => m.UserEmail == email) && p.Owner != email)
                .OrderByDescending(p => p.UpdatedAt)
                .ToList();

            var viewModel = new CreateOrJoinViewModel
            {
                OwnedProjects = ownedProjects,
                MemberProjects = memberProjects,
                UserEmail = email
            };

            return View(viewModel);
        }

        // New action to open a specific project
        public IActionResult OpenProject(int id)
        {
            var email = HttpContext.Session.GetString("email");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Home");

            var project = context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("CreateOrJoin");
            }

            // Check if user has access (owner or member)
            bool isOwner = project.Owner == email;
            bool isMember = project.Members.Any(m => m.UserEmail == email);

            if (!isOwner && !isMember)
            {
                TempData["Error"] = "You don't have access to this project.";
                return RedirectToAction("CreateOrJoin");
            }

            // Store ProjectId in session
            HttpContext.Session.SetInt32("ProjectId", project.Id);

            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("email");

            if (email == null)
                return RedirectToAction("Login", "Home");

            var user = context.Users.FirstOrDefault(u => u.UserEmail == email);

            if (user == null)
                return RedirectToAction("Login", "Home");

            // Get ProjectId from session
            var projectId = HttpContext.Session.GetInt32("ProjectId");

            Project? project = null;

            if (projectId != null)
            {
                project = context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefault(p => p.Id == projectId);
            }

            // If no project in session, try to find one
            if (project == null)
            {
                project = context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefault(p => p.Owner == email);
            }

            if (project == null)
            {
                project = context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefault(p => p.Members.Any(m => m.UserEmail == email));
            }

            if (project == null)
            {
                TempData["Error"] = "No project found. Create or join a project to continue.";
                return RedirectToAction("CreateOrJoin");
            }

            HttpContext.Session.SetInt32("ProjectId", project.Id);

            ViewBag.IsOwner = (project.Owner == email);
            ViewBag.CurrentUserEmail = email;
            ViewBag.CurrentUserName = user.UserName;

            return View(project);
        }

        [HttpPost]
        public IActionResult JoinProject(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Please enter a token.";
                return View("Join");
            }

            token = token.Trim();
            if (token.Length != 6 || !token.All(char.IsDigit))
            {
                TempData["Error"] = "Token must be exactly 6 digits.";
                return View("Join");
            }

            var email = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "You must be logged in to join a project.";
                return RedirectToAction("Login", "Home");
            }

            var user = context.Users.FirstOrDefault(u => u.UserEmail == email);
            if (user == null)
            {
                TempData["Error"] = "User not found. Please log in again.";
                return RedirectToAction("Login", "Home");
            }

            var project = context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.EntryToken == token);

            if (project == null)
            {
                TempData["Error"] = "Invalid token. No project found with this token.";
                return View("Join");
            }

            if (project.Owner == email)
            {
                TempData["Error"] = "You are already the owner of this project.";
                return View("Join");
            }

            if (project.Members.Any(m => m.UserEmail == email))
            {
                TempData["Error"] = "You are already a member of this project.";
                return View("Join");
            }

            project.Members.Add(user);
            project.UpdatedAt = DateTime.Now;

            try
            {
                context.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while joining the project. Please try again.";
                return View("Join");
            }

            HttpContext.Session.SetInt32("ProjectId", project.Id);
            TempData["SuccessMessage"] = $"Successfully joined '{project.ProjectName}'!";
            return RedirectToAction("Dashboard");
        }

        public IActionResult Join()
        {
            return View();
        }

        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("email");

            if (email == null)
                return RedirectToAction("Login", "Home");

            var user = context.Users.FirstOrDefault(u => u.UserEmail == email);

            if (user == null)
                return RedirectToAction("Login", "Home");

            return View(user);
        }

        [HttpPost]
        public IActionResult Create(Project project)
        {
            if (project == null)
            {
                ModelState.AddModelError("", "Invalid project data.");
                ViewBag.ShowCreateModal = true;
                return RedirectToAction("CreateOrJoin");
            }

            var email = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "You must be logged in to create a project.");
                ViewBag.ShowCreateModal = true;
                return RedirectToAction("CreateOrJoin");
            }

            project.Owner = email;

            if (!ModelState.IsValid)
            {
                ViewBag.ShowCreateModal = true;
                return RedirectToAction("CreateOrJoin");
            }

            context.Projects.Add(project);
            context.SaveChanges();

            HttpContext.Session.SetInt32("ProjectId", project.Id);
            TempData["SuccessMessage"] = "Project created successfully!";

            return RedirectToAction("Dashboard");
        }

        public IActionResult AddPeople()
        {
            var email = HttpContext.Session.GetString("email");
            var projectId = HttpContext.Session.GetInt32("ProjectId");
            if (projectId == null)
                return RedirectToAction("CreateOrJoin");

            var project = context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return RedirectToAction("CreateOrJoin");

            if (project.Owner != email)
            {
                TempData["Error"] = "Only project owners can invite new members.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.GeneratedToken = project.EntryToken;
            return View(project);
        }

        [HttpPost]
        public IActionResult GenerateToken()
        {
            var email = HttpContext.Session.GetString("email");
            var projectId = HttpContext.Session.GetInt32("ProjectId");

            if (projectId == null)
                return RedirectToAction("CreateOrJoin");

            var project = context.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return RedirectToAction("Dashboard");

            if (project.Owner != email)
            {
                TempData["Error"] = "Only project owners can generate invite codes.";
                return RedirectToAction("Dashboard");
            }

            string token = new Random().Next(100000, 999999).ToString();
            project.EntryToken = token;
            context.SaveChanges();

            ViewBag.GeneratedToken = token;
            return View("AddPeople", project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int id, string userName)
        {
            var email = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Home");
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                TempData["Error"] = "Username cannot be empty.";
                return RedirectToAction("Profile");
            }

            var user = context.Users.FirstOrDefault(u => u.Id == id && u.UserEmail == email);
            if (user == null)
            {
                TempData["Error"] = "Unable to load your profile.";
                return RedirectToAction("Profile");
            }

            user.UserName = userName.Trim();
            user.UpdatedAt = DateTime.Now;
            context.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }
    }
}