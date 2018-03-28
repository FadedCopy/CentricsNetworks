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

        public IActionResult Index()
        {
            User user = _context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            if (_context.CheckUserPrivilege(user))
            {
                return View();
            }
            else return View("Error");
        }
    }
}