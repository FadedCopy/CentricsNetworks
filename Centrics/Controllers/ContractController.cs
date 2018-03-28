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
                if (!DateComparer(model.StartValid, model.EndValid))
                {
                    ModelState.AddModelError("","End Date should be set to a date after the start date. ");
                    return View(model);
                }
                if (!(model.EndValid.CompareTo(DateTime.Today) > 0))
                {
                    ModelState.AddModelError("", "The Contract you are trying to add has an End Date that expired already");
                    return View(model);
                }

                CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                
                context.AddContract(model);
                return RedirectToAction("ViewContract");
            }
            return View(model);
        }
        
        [HttpGet]
        public IActionResult ModifyContract(int contractid)
        {
            if (contractid == 0)
            {
                return RedirectToAction("ViewContract");
            }

            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            Contract model = context.getContract(contractid);
            
            Debug.WriteLine(model.ClientCompany + "Que");

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifyContract(Contract meh)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            context.ModifyContract(meh);
            return RedirectToAction("ViewContract");
        }

        [HttpGet]
        public IActionResult ViewContract()
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            List<Contract> contractlist = context.getContracts();
            List<Contract> ValidContract = new List<Contract>();
            List<Contract> ExpiredContract = new List<Contract>();
            List<Contract> FutureContract = new List<Contract>();
            List<Contract> FinishedContract = new List<Contract>();
            int counter = contractlist.Count();
            int i = 0;
            while(counter != 0)
            {
                if (contractlist[i].EndValid.CompareTo(DateTime.Today) > 0 && (contractlist[i].StartValid.CompareTo(DateTime.Today)) < 0)
                {
                    if (contractlist[i].MSH != 0)
                    {
                        ValidContract.Add(contractlist[i]);
                    }
                    else
                    {
                        FinishedContract.Add(contractlist[i]);
                    }
                }else if(contractlist[i].EndValid.CompareTo(DateTime.Today) < 0)
                {
                    ExpiredContract.Add(contractlist[i]);
                }else if(contractlist[i].StartValid.CompareTo(DateTime.Today) > 0)
                {
                    FutureContract.Add(contractlist[i]);
                }
                i++;
                counter--;
            }
            if(FinishedContract.Count != 0)
            {
                ViewData["Finished"] = FinishedContract;
            }
            if (ExpiredContract.Count != 0)
            {
                ViewData["Expired"] = ExpiredContract;
            }
            if (ValidContract.Count != 0)
            {
                ViewData["Valid"] = ValidContract;
            }
            if (FutureContract.Count != 0)
            {
                ViewData["Future"] = FutureContract;
                Debug.WriteLine("This is the future:" + ViewData["Future"]);
            }
            return View();
        }

        public Boolean DateComparer(DateTime t1, DateTime t2)
        {
            int diff = DateTime.Compare(t1, t2);
            if(diff < 1)
            {
                return true;
            }else if(diff == 0 || diff > 1)
            {
                return false;
            }
            return false;
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
