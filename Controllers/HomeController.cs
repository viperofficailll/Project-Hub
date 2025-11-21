
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectHub.Data;
using ProjectHub.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;


namespace ProjectHub.Controllers
{
    public class HomeController(AppDbContext context) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            ModelState.Remove(nameof(user.UserName));

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var userToCheck = context.Users.FirstOrDefault(u => u.UserEmail == user.UserEmail);
            if (userToCheck == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(user);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(userToCheck, userToCheck.UserPassword, user.UserPassword);

            if (result == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("email", user.UserEmail);
                return RedirectToAction("CreateOrJoin", "Project");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(user);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            ///1. check the data annotations required ,email,minlength etc
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            ///2. check if the user already exists 
            if(context.Users.Any(u=> u.UserEmail == user.UserEmail))
            {
                ModelState.AddModelError( "UserEmail","Email already exists");
                return View(user);
            }



            var passwordHasher = new PasswordHasher<User>();

            // Hash the password
            user.UserPassword = passwordHasher.HashPassword(user, user.UserPassword);

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login"); // Optional but recommended
        }


        public IActionResult Verify()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }

}
