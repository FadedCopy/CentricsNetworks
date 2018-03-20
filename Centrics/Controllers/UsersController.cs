using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Net.Http;

namespace Centrics.Controllers
{
    public class UsersController : Controller
    {
        private readonly CentricsContext _context;
        private const string GoogleAuthKey = "CentricsNetworks123!@#"; //You can add your own Key

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
            if (ModelState.IsValid)
            {
                string message = "";
                bool status = false;

                LoginViewModel UserLogin = _context.LoginUser(model);
                bool successfulLogin = UserLogin.SuccessfulLogin;
                if (successfulLogin)
                {
                    status = true;
                    message = "Two Factor Authentication Verification";
                    TempData["Email"] = model.UserEmail;
                    //Two Factor Authentication Setup
                    TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                    string UserUniqueKey = (model.UserEmail + GoogleAuthKey);
                    TempData["UserUniqueKey"] = UserUniqueKey; //Session
                    var setupInfo = TwoFacAuth.GenerateSetupCode("Centrics Network", model.UserEmail, UserUniqueKey, 300, 300);
                    ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                    ViewBag.SetupCode = setupInfo.ManualEntryKey;
                    ViewBag.Message = message;  
                    ViewBag.Status = status;
                    TempData["LoginID"] = UserLogin.UserID;
                    return RedirectToAction("Send2FA");
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
            ViewBag.UsersData = _context.GetUsers();
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
            User retrieveUserEdited = _context.GetUser(UserID);
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
            passwords.UserID = Convert.ToInt32(TempData["LoginID"]);
            if (ModelState.IsValid)
            {
                User userChanged = _context.GetUser(passwords.UserID);
                if (_context.ChangePassword(CurrentPassword, NewPassword, userChanged))
                {
                    ViewBag.Message = "Success!";
                }
                else
                    ViewBag.Message = "Failure";
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendResetLink(ForgotPasswordViewModel model)
        {
            _context.SendResetLink(model);

            return RedirectToAction("ForgotPassword");
        }

       
        
        [HttpGet]
        public IActionResult Send2FA()
        {

            string email = TempData["Email"].ToString();
            //Two Factor Authentication Setup
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = (email + GoogleAuthKey);
            TempData["UserUniqueKey"] = UserUniqueKey; //Session
            var setupInfo = TwoFacAuth.GenerateSetupCode("Centrics Network",email , UserUniqueKey, 300, 300);
            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.SetupCode = setupInfo.ManualEntryKey;
            TempData["Email"] = email;
            return View();
        }
        [HttpPost]
        public IActionResult Send2FA(TwoFactorAuth model)
        {
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = TempData["UserUniqueKey"].ToString();
            Debug.WriteLine("Token = " + model.CodeDigit.ToString());
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, model.CodeDigit.ToString());
            if (isValid)
            {
                HttpContext.Session.SetString("IsValidTwoFactorAuthentication", "true");
                return RedirectToAction("ViewUsers", "Users");
            }
            else {
                ModelState.AddModelError("","Invalid Code Entered");
                string email = TempData["Email"].ToString();
                TempData["Email"] = email;
                UserUniqueKey = (email + GoogleAuthKey);
                TempData["UserUniqueKey"] = UserUniqueKey; //Session
                return View();
            }
            //return RedirectToAction("Login", "Users");

        }
    }
}