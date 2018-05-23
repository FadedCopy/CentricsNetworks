using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Centrics.Models;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hangfire;
using Hangfire.Storage;

namespace Centrics.Controllers
{
    public class ClientAddressController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["viewer"] = context.getAllClientAddress();
            List < ClientAddress > listca = context.getAllClientAddress();
            
            return View();
        }

        [HttpPost]
        public IActionResult Index(searcher val)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["viewer"] = context.SearchClientAddress(val.searchvalue);
            
            return View();
        }
        public IActionResult SearchComapny()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Company(string name)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (name == "")
            {
                return RedirectToAction("index");
            }

            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ClientAddress boo = context.GetClientAddressList(name);

            if (boo.ClientCompany == "" || boo.ClientCompany == null)
            {
                //what happen when company do not have an address yet?
                Debug.WriteLine("This is not good");
                return RedirectToAction("index");
            }
            else
            {
                ViewData["CompanyName"] = boo.ClientCompany;
                if (boo.Addresslist != null )
                {
                    Debug.WriteLine("how the fk is this initialize?" + boo.Addresslist.Count());
                    ViewData["AddressList"] = boo.Addresslist;
                }
            }
            
            TempData["companyname"] = boo.ClientCompany;
            return View();
        }

        [HttpGet]
        public ActionResult AddAddress()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }

            string name = TempData.Peek("companyname").ToString();
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ClientAddress cA = context.GetClientAddressList(name);
            if (cA.Addresslist == null)
            {
                TempData["Message"] = " false";
            }else if (cA.Addresslist.Count() > 0 && cA.Addresslist.Count() <5 )
            {
                TempData["Message"] = "false";
            }
            else if (context.GetClientAddressList(name).Addresslist.Count() == 5)
            {
                TempData["Message"] = "true";
                //return RedirectToAction("company", new { name = name });
            }
            
            ClientAddress Weeeheee = new ClientAddress { ClientCompany= name};
            return PartialView("AddAddress",Weeeheee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAddress(ClientAddress clientAddress)
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
            context.AddAdresstoClientAddressList(clientAddress);
            context.LogAction("Client", "New address (" + clientAddress.Address + ") has been added to " + clientAddress.ClientCompany + ".", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            return RedirectToAction("Company", new { name = clientAddress.ClientCompany });
        }
        
        
        public IActionResult DeleteAddress(string name,string address)
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Company", new { name = name });
            }
                if (name == "" || name == null || address == "" || address == null)
            {
                return RedirectToAction("Index");
            }
            Debug.WriteLine("Kill" + address);
            context.RemoveAddressFromClientList(name, address);
            context.LogAction("Client", "Address (" + address + ") has been removed from client " + name + ".", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            return RedirectToAction("Company", new { name = name });
        }


        [HttpGet]
        public IActionResult AddNewCompany()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddNewCompany(ClientAddress clientAddress)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }
            if (!ModelState.IsValid)
            {
                if (clientAddress.ClientCompany == "")
                {
                    ModelState.AddModelError("", "Please enter a company name");
                } else if (clientAddress.Address == "")
                {
                    ModelState.AddModelError("", "Please enter a Address");
                }

                return View(clientAddress);
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

            ClientAddress exister = context.GetClientAddressList(clientAddress.ClientCompany);
            if (exister.ClientCompany != "")
            {
                ModelState.AddModelError("", "The company already exists.");
                return View(clientAddress);
            }

            context.LogAction("Client", clientAddress.ClientCompany + " has been added to the client list with this address (" + clientAddress.Address + ")", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            context.AddNewCompany(clientAddress);
            return RedirectToAction("Company", new { name = clientAddress.ClientCompany });
        }
    }
}
