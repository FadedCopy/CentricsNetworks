using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Centrics.Models;
using FluentEmail.Core;
using FluentEmail.Razor;

namespace Centrics.Controllers
{
    public class ContractController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddContract()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContract(Contract model)
        {
            if (ModelState.IsValid)
            {
                CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                context.AddContract(model);
            }
            return View();
        }
        
        [HttpGet]
        public IActionResult ModifyContract()
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

            return View();
        }

        public void CheckContractExpiryWarning()
        {
            DateTime today = DateTime.Today;
            Debug.WriteLine(today);

            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            

            List<Contract> contracts = context.getContracts();

            for (int i = 0; i < contracts.Count; i++) {
                DateTime end = contracts[i].EndValid;
                if ((end - today).Days == 30)
                {
                    //send email
                }
                else if ((end - today).Days == 7)
                {
                    //send email dashboard?

                }
                else if ((end - today).Days == 0)
                {
                    //send email
                }
            }
        }
        //works but weirdly works? feel like u can send from any account to any account without validation which is wtf?
        public void ContractExpiryWarningEmail()
        {
            // Using Razor templating package
            Email.DefaultRenderer = new RazorRenderer();

            var template = "Dear @Model.Name, You are totally @Model.Compliment.";

            var email = Email
                .From("fadedcostt@gmail.com")
                .To("ai.permacostt@gmail.com")
                .Subject("burden")
                .UsingTemplate(template, new { Name = "Luke", Compliment = "Awesome" });
            email.Send();
        }
    }
}
