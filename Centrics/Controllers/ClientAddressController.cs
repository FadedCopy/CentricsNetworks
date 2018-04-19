﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Centrics.Models;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Centrics.Controllers
{
    public class ClientAddressController : Controller
    {
        public IActionResult Index()
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["viewer"] = context.getAllClientAddress();
            List < ClientAddress > listca = context.getAllClientAddress();
            
            return View();
        }

        [HttpPost]
        public IActionResult Index(searcher val)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["viewer"] = context.SearchClientAddress(val.searchvalue);
            
            return View();
        }
        public IActionResult SearchComapny()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Company(string name)
        {
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
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            context.AddAdresstoClientAddressList(clientAddress);
            return RedirectToAction("Company", new { name = clientAddress.ClientCompany });
        }
        
        
        public IActionResult DeleteAddress(string name,string address)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            if (name == "" || name == null || address == "" || address == null)
            {
                return RedirectToAction("Index");
            }
            context.RemoveAddressFromClientList(name, address);
            return RedirectToAction("Company", new { name = name });
        }


        [HttpGet]
        public IActionResult AddNewCompany()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewCompany(ClientAddress clientAddress)
        {
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

            context.AddNewCompany(clientAddress);
            return RedirectToAction("Company", new { name = clientAddress.ClientCompany });
        }
    }
}
