using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Centrics.Controllers
{
    public class AdminController : Controller
    {
        private readonly CentricsContext _context;

        public AdminController(CentricsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            User user = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin")
            {
                return View();
            }
            else return View("Error");
        }

        [HttpGet]
        public IActionResult Logs()
        {
            User user = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin")
            {
                ViewBag.LogsData = _context.GetAllLogs();
                return View();
            }
            else return View("Error");
        }
        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
        public IActionResult DeleteAllLogs()
        {
            if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin")
            {
                _context.DeleteAllLogs();
            }
            else
            {
                ViewBag.Permissions = "You do not have sufficient permissions to delete all logs. Only Super Admins are able to delete all logs.";
                return View("Logs");
            }
    
            return View("Logs");
        }
    }
}