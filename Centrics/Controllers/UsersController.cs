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
            //If user input for registration is valid
            if (ModelState.IsValid)
            {
                //Check whether email is already existing in the database
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
        public IActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                    _context.EditUser(model);
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
                UserID = UserID,
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
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            //Sends the Reset Link to the user's email
            if (_context.SendResetLink(model))
            {
                //Initializes Viewbag so it will display on the View.
                ViewBag.Message = "Email sent, check your email for future instructions.";
                TempData["ForgotEmail"] = model.UserEmail;
                return View();
            }
            
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            TempData["ResetID"] = HttpContext.Request.Query["ResetID"].ToString();
            TempData["UserID"] = HttpContext.Request.Query["UserID"].ToString();
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            model.ResetID = TempData["ResetID"].ToString();
            model.UserID = Convert.ToInt32(TempData["UserID"]);
            if (_context.RetrieveResetIDFromDB(model.ResetID))
            {
                _context.ResetPassword(model);
                ViewBag.Message = "Reset Password Successful";
            }
            else
            {
                ViewBag.Message = "Reset Password Failed";
            }
            return View();
        }

        [HttpGet]
        public IActionResult Send2FA()
        {
            string email = TempData["Email"].ToString();
            //Two Factor Authentication Setup
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = (email + GoogleAuthKey);
            TempData["UserUniqueKey"] = UserUniqueKey; //Session
            var setupInfo = TwoFacAuth.GenerateSetupCode("Centrics Network", email , UserUniqueKey, 300, 300);
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
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, model.CodeDigit.ToString());
            if (isValid)
            {
                HttpContext.Session.SetString("IsValidTwoFactorAuthentication", "true");
                HttpContext.Session.SetString("UserID", TempData["LoginID"].ToString());
                return RedirectToAction("Profile", "Users");
            }
            else {
                ModelState.AddModelError("","Invalid Code Entered");
                string email = TempData["Email"].ToString();
                TempData["Email"] = email;
                UserUniqueKey = (email + GoogleAuthKey);
                TempData["UserUniqueKey"] = UserUniqueKey; //Session
                return View();
            }   
        }

        [HttpGet]
        public IActionResult Profile()
        {
            User UserLoggedIn = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            ViewBag.UserID = UserLoggedIn.UserID;
            ViewBag.Email = UserLoggedIn.UserEmail;
            ViewBag.FirstName = UserLoggedIn.FirstName;
            ViewBag.LastName = UserLoggedIn.LastName;

            return View();
        }
    }
}