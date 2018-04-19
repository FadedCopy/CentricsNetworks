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
using System.Security.Cryptography;

namespace Centrics.Controllers
{
    public class UsersController : Controller
    {
        private readonly CentricsContext _context;
        private const string GoogleAuthKey = "CentricsNetworks123!@#"; //Can be changed

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
            model.UserRole = "User";
            //If user input for registration is valid
            if (ModelState.IsValid)
            {
                //Check whether email is already existing in the database
                Boolean validEmail = _context.CheckExistingEmail(model);
                if (validEmail)
                {
                    _context.RegisterUser(model);
                    User registeredUser = _context.GetUserByEmail(model.UserEmail);
                    _context.LogAction("Registration", "User " + registeredUser.FirstName + " " + registeredUser.LastName + " has successfully registered with " + registeredUser.UserEmail + " as ID "+ registeredUser.UserID + ".", registeredUser);
                    TempData["Status"] = "Registration successful. Please login here.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Message = "Email already exists, please enter another email.";
                    ViewData["Roles"] = model.Roles;
                    return View();
                }
                
            }
            ViewData["Roles"] = model.Roles;
            return View("Error");
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
                LoginViewModel UserLogin = _context.LoginUser(model);
                bool successfulLogin = UserLogin.SuccessfulLogin;
                if (successfulLogin)
                {
                    TempData["LoginID"] = model.UserID;
                    TempData["LoginEmail"] = model.UserEmail;
                    _context.LogAction("Login", "Login successful.", model);
                    return RedirectToAction("Send2FA");
                }
                else
                {
                    TempData["Status"] = "Invalid username or password entered. Please try again.";
                    _context.LogAction("Login", "Login failed. Invalid password entered for user.", model);
                    return View();
                }
            }

            return View();
        }

        public IActionResult DeleteUser(int UserID)
        {
            User userDeleted = _context.GetUser(UserID);
            User userWhoDeleted = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            _context.LogAction("User Deleted", "User " + userDeleted.FirstName + " " + userDeleted.LastName + " (User ID: " + userDeleted.UserID + ") has been deleted by User " + userWhoDeleted.FirstName + " " + userWhoDeleted.LastName + ".", userWhoDeleted);
            _context.DeleteUser(UserID);
            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public IActionResult ViewUsers()
        {
            if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin")
            {
                ViewBag.UsersData = _context.GetUsers();
                return View();
            }
            else return RedirectToAction("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin")
                {
                    User userWhoEdited = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
                    _context.EditUser(model);
                    EditUserViewModel newDetails = model;
                    _context.LogAction("User Edited", "Name: " + newDetails.FirstName + " " + newDetails.LastName + " Email: " + newDetails.UserEmail + " Role: " + newDetails.UserRole, userWhoEdited);
                    TempData["Status"] = "User successfully edited.";
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
            ViewData["RolesNoSuperAdmin"] = userRoles.RolesNoSuperAdmin;
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
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return View("Login");
            }
            else return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel passwords)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return View("Login");
            }
            string CurrentPassword = passwords.CurrentPassword;
            string NewPassword = passwords.NewPassword;
            passwords.UserID = Convert.ToInt32(HttpContext.Session.GetString("LoginID"));
            if (ModelState.IsValid)
            {
                User userChanged = _context.GetUser(passwords.UserID);
                if (_context.ChangePassword(CurrentPassword, NewPassword, userChanged))
                {
                    TempData["PWMessage"] = "Password change successful!";
                    _context.LogAction("Password Change", "Password change successful, user has changed their own password.", userChanged);
                }

                else
                {
                    TempData["PWMessage"] = "Current password entered is incorrect, please try again.";
                    _context.LogAction("Password Change", "Password change failure, user entered an invalid password for their account.", userChanged);
                }
            }
            
            return View("ChangePassword");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            //Sends the Reset Link to the user's email
            if (_context.SendResetLink(model))
            {
                //Initializes Viewbag so it will display on the View.
                ViewBag.Message = "Email sent, check your email for future instructions.";
                TempData["ForgotEmail"] = model.UserEmail;
                _context.LogAction("Forgot Password", "Reset link sent to user's email.", _context.GetUserByEmail(model.UserEmail));
                return View();
            }
            else
            {
                ViewBag.Message = "Email does not exist. Please try again.";
                return View();
            }
            

        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            TempData["ResetID"] = HttpContext.Request.Query["ResetID"].ToString();
            TempData["UserID"] = HttpContext.Request.Query["UserID"].ToString();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            model.ResetID = TempData["ResetID"].ToString();
            model.UserID = Convert.ToInt32(TempData["UserID"]);
            if (_context.RetrieveResetIDFromDB(model.ResetID))
            {
                _context.ResetPassword(model);
                ViewBag.Message = "Reset password successful.";
                _context.LogAction("Forgot Password", "User successfully reset password.", _context.GetUser(model.UserID));
            }
            else
            {
                ViewBag.Message = "Password reset failed, please try again.";
            }
            return View();
        }

        [HttpGet]
        public IActionResult Send2FA()
        {
            string email = TempData["LoginEmail"].ToString();
            //Two Factor Authentication Setup
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            User loggingIn = _context.GetUserByEmail(email);
            string UserUniqueKey = (email + GoogleAuthKey);
            TempData["UserUniqueKey"] = UserUniqueKey; //Session
            var setupInfo = TwoFacAuth.GenerateSetupCode("Centrics Network", email, UserUniqueKey, 300, 300);
            ViewBag.Message = "Enter your code displayed in Google Authenticator.";
            if (loggingIn.Authenticated == false)
            {
                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupInfo.ManualEntryKey;
                TempData["LoginEmail"] = email;
            }
            else if (loggingIn.Authenticated == true)
            {
                ViewBag.BarcodeImageUrl = null;
                ViewBag.SetupCode = null;
                TempData["LoginEmail"] = email;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Send2FA(TwoFactorAuth model)
        {
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = TempData["UserUniqueKey"].ToString();
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, model.CodeDigit.ToString());
            if (isValid)
            {
                HttpContext.Session.SetString("IsValidTwoFactorAuthentication", "true");
                HttpContext.Session.SetString("LoginID", TempData["LoginID"].ToString());
                HttpContext.Session.SetString("LoginEmail", TempData["LoginEmail"].ToString());
                User userLoggedIn = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
                _context.LogAction("2 Factor Authentication", "Authentication successful.", userLoggedIn);
                if (userLoggedIn.UserRole == "Admin")
                {
                    HttpContext.Session.SetString("AdminValidity", "Admin");
                }
                else if (userLoggedIn.UserRole == "Super Admin")
                {
                    HttpContext.Session.SetString("AdminValidity", "Super Admin");
                }
                if (userLoggedIn.Authenticated == false)
                    _context.SetUserAsAuthenticated(HttpContext.Session.GetString("LoginEmail"));

                return RedirectToAction("Profile", "Users");
            }
            else {
                ViewBag.Message = "Invalid code entered, please try again.";
                string email = TempData["LoginEmail"].ToString();
                TempData["LoginEmail"] = email;
                UserUniqueKey = (email + GoogleAuthKey);
                TempData["UserUniqueKey"] = UserUniqueKey; //Session
                User userLoggingIn = _context.GetUserByEmail(email);
                _context.LogAction("2 Factor Authentication", "Authentication failure. Invalid code entered.", userLoggingIn);
                return View("Send2FA");
            }   
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("LoginID") != null)
            {
                User UserLoggedIn = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
                ViewBag.UserID = UserLoggedIn.UserID;
                ViewBag.Email = UserLoggedIn.UserEmail;
                ViewBag.FirstName = UserLoggedIn.FirstName;
                ViewBag.LastName = UserLoggedIn.LastName;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
        
        public IActionResult Logout()
        {
            _context.LogAction("Logout", "User logged out.", _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            HttpContext.Session.Clear();
            TempData["Status"] = "You have successfully logged out.";
            return View("Login");
        }

        public IActionResult Reset2FA()
        {
            string email = TempData["LoginEmail"].ToString();
            //Two Factor Authentication Setup
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            User loggingIn = _context.GetUserByEmail(email);
            string UserUniqueKey = (email + GoogleAuthKey);
            TempData["UserUniqueKey"] = UserUniqueKey; //Session
            var setupInfo = TwoFacAuth.GenerateSetupCode("Centrics Network", email, UserUniqueKey, 300, 300);
            ViewBag.Message = "Enter your code displayed in Google Authenticator.";
            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.SetupCode = setupInfo.ManualEntryKey;
            TempData["LoginEmail"] = email;
            _context.Reset2FactorAuth(email);
            _context.LogAction("2 Factor Authentication", "User reset 2 Factor Authentication.", _context.GetUserByEmail(email));
            return View("Send2FA");
        }
    }
}