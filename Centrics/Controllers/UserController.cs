using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Microsoft.AspNetCore.Mvc;

namespace Centrics.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterUser(User model)
        {

            if (ModelState.IsValid)
            {
                CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                context.RegisterUser(model);

                return RedirectToAction("ViewClient", "Client");

            }
            return View();
        }

        [HttpGet]
        public IActionResult DeleteUsers()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewUsers()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditUsers()
        {
            return View();
        }
    }
}