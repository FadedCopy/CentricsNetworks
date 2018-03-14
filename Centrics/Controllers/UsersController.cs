using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Microsoft.AspNetCore.Mvc;

namespace Centrics.Controllers
{
    public class UsersController : Controller
    {
        private readonly CentricsContext _context;

        public UsersController(CentricsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RegisterUser()
        {
            User model = new User();
            ViewData["Roles"] = model.Roles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterUser(User model)
        {

            if (ModelState.IsValid)
            {
                Boolean validEmail = _context.CheckExistingEmail(model);
                if (validEmail)
                {
                    _context.RegisterUser(model);

                    return RedirectToAction("ViewUsers", "Users");
                }
                else
                {
                    ViewBag.Message = "Email already exists, please enter another email.";
                    ViewData["Roles"] = model.Roles;
                    return View();
                }
                
            }
            ViewData["Roles"] = model.Roles;
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            Boolean successfulLogin;
            if (ModelState.IsValid)
            {
                successfulLogin = _context.LoginUser(model);

                if (successfulLogin)
                {
                    ViewBag.Message = "Success.";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Failed.";
                    return View();
                }
            }

            return View();
        }

        public IActionResult DeleteUser(int UserID)
        {
            _context.DeleteUser(UserID);

            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public IActionResult ViewUsers()
        {
            ViewBag.UsersData = _context.getUsers();
            return View();
        }

        public IActionResult EditUser(int UserID)
        {
            User userEdited = new User();

            userEdited = _context.getUser(UserID);

            return RedirectToAction("ViewUsers");
        }

        public IActionResult EditUserDetails(int UserID)
        {
            User userEdited = _context.getUser(UserID);
            User userRoles = new User();
            ViewData["Roles"] = userRoles.Roles;
            return PartialView("EditUserDetails");
        }
    }
}