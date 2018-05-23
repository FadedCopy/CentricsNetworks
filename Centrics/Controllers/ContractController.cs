using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Centrics.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Centrics.Controllers
{
    public class ContractController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        [HttpGet]
        public IActionResult AddContract()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            List<ClientAddress> clientAddresses = context.getAllClientAddress();
            List<SelectListItem> companynames = new List<SelectListItem>();
            for (int i = 0; i < clientAddresses.Count; i++)
            {
                companynames.Add(new SelectListItem { Value = clientAddresses[i].ClientCompany, Text = clientAddresses[i].ClientCompany });
            }

            ViewData["Company"] = companynames;

            ViewData["Company"] = companynames;
            List<SelectListItem> ContractTypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Bundled hours with product", Text = "Bundled hours with product" },
                new SelectListItem { Value = "Maintenance Contract", Text = "Maintenance Contract" }
            };

            ViewData["ContractType"] = ContractTypeList;

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContract(Contract model)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }
            List<SelectListItem> ContractTypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Bundled hours with product", Text = "Bundled hours with product" },
                new SelectListItem { Value = "Maintenance Contract", Text = "Maintenance Contract" }
            };

            ViewData["ContractType"] = ContractTypeList;

            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            List<ClientAddress> clientAddresses = context.getAllClientAddress();
            List<SelectListItem> companynames = new List<SelectListItem>();
            for (int i = 0; i < clientAddresses.Count; i++)
            {
                companynames.Add(new SelectListItem { Value = clientAddresses[i].ClientCompany, Text = clientAddresses[i].ClientCompany });
            }

            ViewData["Company"] = companynames;

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
                
                context.LogAction("Contract", "New " + model.ContractType + " contract has been created for " + model.ClientCompany + " lasting from " + model.StartValid + " to " + model.EndValid + ".", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
                context.AddContract(model);
                return RedirectToAction("ViewContract");
            }
            return View(model);
        }
        
        //[HttpGet]
        //public IActionResult ModifyContract(int contractid)
        //{
        //    if (contractid == 0)
        //    {
        //        return RedirectToAction("ViewContract");
        //    }

        //    CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
        //    Contract model = context.getContract(contractid);
        //    context.Emailsender(30, model);

        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ModifyContract(Contract meh)
        //{
        //    CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
        //    context.ModifyContract(meh);
        //    return RedirectToAction("ViewContract");
        //}

        [HttpGet]
        public IActionResult ViewContract()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            
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
                if (contractlist[i].EndValid.CompareTo(DateTime.Today) >= 0 && (contractlist[i].StartValid.CompareTo(DateTime.Today)) <= 0)
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
    }
}
