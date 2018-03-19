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

        [HttpPost]
        public IActionResult EditUser(EditUserViewModel model, int UserID)
        {
            if (ModelState.IsValid)
            {
                Boolean validEmail = _context.CheckEditExistingEmail(model);
                if (validEmail)
                {
                    _context.EditUser(model, Convert.ToInt32(TempData["UserID"]));
                }
            }
            return RedirectToAction("ViewUsers");
        }

        [HttpGet]   
        public IActionResult EditUserDetails(int UserID)
        {
            User retrieveUserEdited = _context.getUser(UserID);
            User userRoles = new User();
            TempData["UserID"] = UserID;
            ViewData["Roles"] = userRoles.Roles;
            EditUserViewModel editedUser = new EditUserViewModel
            {
                FirstName = retrieveUserEdited.FirstName,
                LastName = retrieveUserEdited.LastName,
                UserEmail = retrieveUserEdited.UserEmail,
                UserRole = retrieveUserEdited.UserRole
            };
            return PartialView("EditUserDetails", editedUser);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel passwords)
        {
            string CurrentPassword = passwords.CurrentPassword;
            string NewPassword = passwords.NewPassword;

            User userChanged = _context.getUser(passwords.UserID);

            _context.ChangePassword(NewPassword, userChanged);
        }
    }
}