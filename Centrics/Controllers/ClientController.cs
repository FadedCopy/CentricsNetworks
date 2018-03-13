using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Centrics.Models;
using MySql.Data.MySqlClient;

namespace Centrics.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        

        //this is not creating client account
        [HttpGet]
        public IActionResult AddClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClient(Client model)
        {
            
            if (ModelState.IsValid)
            {
                    CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                    context.AddClient(model);
                    
                    return RedirectToAction("ViewClient","Client");
                

            }
            return View();
        }

        [HttpGet]
        public IActionResult ViewClient()
        {
            return View();
        }

    }
}
