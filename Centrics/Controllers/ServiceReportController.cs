using Centrics.Models;
using Geocoding.Google;
using Hangfire;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Centrics.Controllers
{
    public class ServiceReportController : Controller
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
        public IActionResult AddNewReport()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            //this is for pulling out data from contract to service report
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ServiceReport model = new ServiceReport();
            ViewData["Companies"] = context.GetClientByContract();
            //TempData["address"] = "false";
            //context.Selfcaller();
            


            List<SelectListItem> PurposeList = new List<SelectListItem>();
            PurposeList.Add(new SelectListItem { Value = "Project", Text = "Project" });
            PurposeList.Add(new SelectListItem { Value = "Installation", Text = "Installation" });
            PurposeList.Add(new SelectListItem { Value = "M.S.A", Text = "M.S.A" });
            PurposeList.Add(new SelectListItem { Value = "Chargable", Text = "Chargable" });
            PurposeList.Add(new SelectListItem { Value = "P.O.C", Text = "P.O.C" });
            PurposeList.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
            PurposeList.Add(new SelectListItem { Value = "Return", Text = "Return" });
            PurposeList.Add(new SelectListItem { Value = "Others", Text = "Others" });
            ViewData["Purpose"] = PurposeList;

            List<SelectListItem> JobStatusList = new List<SelectListItem>();
            JobStatusList.Add(new SelectListItem { Value = "Completed", Text = "Completed" });
            JobStatusList.Add(new SelectListItem { Value = "Followup Required", Text = "Followup Required" });
            JobStatusList.Add(new SelectListItem { Value = "Recommendation Requied", Text = "Recommendation Required" });
            JobStatusList.Add(new SelectListItem { Value = "Escalated to Ext. Support", Text = "Escalated to Ext. Support" });
            ViewData["JobStatusList"] = JobStatusList;

            #region Client Ratings? Questionable needed on this form 
            //Client side?
            //List<SelectListItem> RatingList = new List<SelectListItem>();
            //RatingList.Add(new SelectListItem { Value = "Excellent", Text = "Excellent" });
            //RatingList.Add(new SelectListItem { Value = "Good", Text = "Good" });
            //RatingList.Add(new SelectListItem { Value = "Average", Text = "Average" });
            //RatingList.Add(new SelectListItem { Value = "Poor", Text = "Poor" });
            //ViewData["RatingList"] = RatingList;
            #endregion
            model.SerialNumber = context.getReportCounts();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AddNewReport(ServiceReport model)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
           

            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["Companies"] = context.GetClientByContract();

            #region prepare for failure codes
            List<SelectListItem> PurposeList = new List<SelectListItem>();
            PurposeList.Add(new SelectListItem { Value = "Project", Text = "Project" });
            PurposeList.Add(new SelectListItem { Value = "Installation", Text = "Installation" });
            PurposeList.Add(new SelectListItem { Value = "M.S.A", Text = "M.S.A" });
            PurposeList.Add(new SelectListItem { Value = "Chargable", Text = "Chargable" });
            PurposeList.Add(new SelectListItem { Value = "P.O.C", Text = "P.O.C" });
            PurposeList.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
            PurposeList.Add(new SelectListItem { Value = "Return", Text = "Return" });
            PurposeList.Add(new SelectListItem { Value = "Others", Text = "Others" });
            ViewData["Purpose"] = PurposeList;

            List<SelectListItem> JobStatusList = new List<SelectListItem>();
            JobStatusList.Add(new SelectListItem { Value = "Completed", Text = "Completed" });
            JobStatusList.Add(new SelectListItem { Value = "Followup Required", Text = "Followup Required" });
            JobStatusList.Add(new SelectListItem { Value = "Recommendation Requied", Text = "Recommendation Required" });
            JobStatusList.Add(new SelectListItem { Value = "Escalated to Ext. Support", Text = "Escalated to Ext. Support" });
            ViewData["JobStatusList"] = JobStatusList;

            model.SerialNumber = context.getReportCounts() + 1;
            string name = "";
            if (((TempData.Peek("dataishere") != null).ToString().ToLower()) == "true")
            {
                name = TempData.Peek("dataishere").ToString();
            }
            
            if (name != "" )
            {
                ClientAddress cA = context.GetClientAddressList(name);
                List<string> aList = cA.Addresslist;
                List<SelectListItem> AddressList = new List<SelectListItem>();
                if (aList != null)
                {
                    for (int i = 0; i < aList.Count(); i++)
                    {
                        AddressList.Add(new SelectListItem { Value = aList[i], Text = aList[i] });
                    }
                }
                ViewData["AddressList"] = AddressList;
                
            }
            else
            {
                ModelState.AddModelError("", "A error occured. Important");
                return View(model);
            }
            #endregion
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ModelState.IsValid)
            {
                double totalmshremain = context.GetRemainingMSHByCompany(model);
                double calculatedhours = context.CalculateMSH(model.TimeStart, model.TimeEnd);
                //ModelState.AddModelError("", "The calculated MSH:" + calculatedhours);
                //return View(model);
                if (totalmshremain < calculatedhours)
                {
                    ModelState.AddModelError("", "The company you have selected does not have enough remaining MSH, Please contact your Boss immediately regarding this issue.");
                    return View(model);

                }
                if (context.CheckExisitingReportID(model.SerialNumber))
                {
                    ModelState.AddModelError("", "Contact Application Developer");
                    return View(model);
                }

                if (model.TimeStart > DateTime.Now || model.TimeEnd > DateTime.Now)
                {
                    ModelState.AddModelError("", "Please enter a report after the service is rendered");
                    return View(model);
                }
                //questionable
                if (!(model.TimeStart.CompareTo(model.TimeEnd) <= 0))
                {
                    ModelState.AddModelError("", "your start time should be before your end time");
                    return View(model);
                }
                User user = context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
                model.ReportFrom = user.FirstName + user.LastName ;
                context.AddServiceReport(model);
                context.LogAction("Service Report", "Service Report (SRN: " + model.SerialNumber + ") created for " + model.ClientCompanyName + " at " + model.ClientAddress + ".", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
                TempData.Remove("dataishere");
                return RedirectToAction("ViewReports", "ServiceReport");
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult ChangeAddressInput([FromBody]string name)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            Debug.WriteLine("debug is here");
            TempData["dataishere"] = name;
            return Json(new { Url = Url.Action("ChangeAddressInput", "ServiceReport") });
        }
        [HttpGet]
        public IActionResult ChangeAddressInput()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            string name = "";
            if (((TempData.Peek("dataishere") != null).ToString().ToLower()) == "true")
            {
                name = TempData.Peek("dataishere").ToString();
            }

            if (name == "" || name == null)
            {
                return View();
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ClientAddress cA = context.GetClientAddressList(name);
            List<string> aList = new List<string>();
            aList = cA.Addresslist;
            Debug.WriteLine(name + "SPARTA");

            if (aList == null || aList.Count() == 0)
            {

            }
            else
            {
                List<SelectListItem> AddressList = new List<SelectListItem>();
                for (int i = 0; i < aList.Count(); i++)
                {
                    AddressList.Add(new SelectListItem { Value = aList[i], Text = aList[i] });
                }
                ViewData["AddressList"] = AddressList;

            }
            Debug.WriteLine(Url.Action("ChangeAddressInput", "ServiceReport"));
            Debug.WriteLine("muahaahahah pelase?");
            return PartialView("ChangeAddressInput",new ServiceReport());
        }
       
        [HttpGet]
        public void Unloader()
        {
            if(((TempData.Peek("dataishere") != null).ToString().ToLower()) == "true")
            {
                TempData.Remove("dataishere");
            }
        }
        [HttpGet]
        public IActionResult ViewReports()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["Pending"] = context.getPendingReports();
            ViewData["Confirmed"] = context.getConfirmedReports();

            return View();
        }

        [HttpPost]
        public IActionResult ViewReports(int page)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            return RedirectToAction("Report",page);
        }

        [HttpGet]
        public IActionResult Report(int id)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == 0)
            {
                return RedirectToAction("ViewReports");
            }
           
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            if (HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin" || context.getServiceReport(id).ReportFrom == context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))).FirstName + context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))).LastName)
            {
                ViewData["allowdelete"] = "weee";
            }
            if ((HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                ViewData["rights"] = "Admin";
            }


            ServiceReport model = context.getServiceReport(id);
            if (model.SerialNumber == 0)
            {
                return RedirectToAction("ViewReports");
            }
            return View(model);
        }



        [HttpGet]
        public IActionResult EditReport(int id)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

            User user = context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));

            ServiceReport model = new ServiceReport();
            
            //ViewData["Companies"] = context.GetClientByContract();

            #region prepare for failure codes
            List<SelectListItem> PurposeList = new List<SelectListItem>();
            PurposeList.Add(new SelectListItem { Value = "Project", Text = "Project" });
            PurposeList.Add(new SelectListItem { Value = "Installation", Text = "Installation" });
            PurposeList.Add(new SelectListItem { Value = "M.S.A", Text = "M.S.A" });
            PurposeList.Add(new SelectListItem { Value = "Chargable", Text = "Chargable" });
            PurposeList.Add(new SelectListItem { Value = "P.O.C", Text = "P.O.C" });
            PurposeList.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
            PurposeList.Add(new SelectListItem { Value = "Return", Text = "Return" });
            PurposeList.Add(new SelectListItem { Value = "Others", Text = "Others" });
            ViewData["Purpose"] = PurposeList;

            List<SelectListItem> JobStatusList = new List<SelectListItem>();
            JobStatusList.Add(new SelectListItem { Value = "Completed", Text = "Completed" });
            JobStatusList.Add(new SelectListItem { Value = "Followup Required", Text = "Followup Required" });
            JobStatusList.Add(new SelectListItem { Value = "Recommendation Requied", Text = "Recommendation Required" });
            JobStatusList.Add(new SelectListItem { Value = "Escalated to Ext. Support", Text = "Escalated to Ext. Support" });
            ViewData["JobStatusList"] = JobStatusList;

            //model.SerialNumber = context.getReportCounts() + 1;
            #endregion
            //pull db
            model = context.getServiceReport(id);
            if (model.SerialNumber == 0)
            {
                return RedirectToAction("Report");
            }
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin" || (user.FirstName + user.LastName) == context.getServiceReport(id).ReportFrom))
            {
                return RedirectToAction("Error", "Admin");
            }

            


            Debug.WriteLine("edit report" + model.TimeEnd  + model.TimeStart) ;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReport(ServiceReport report)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            List<SelectListItem> PurposeList = new List<SelectListItem>();
            PurposeList.Add(new SelectListItem { Value = "Project", Text = "Project" });
            PurposeList.Add(new SelectListItem { Value = "Installation", Text = "Installation" });
            PurposeList.Add(new SelectListItem { Value = "M.S.A", Text = "M.S.A" });
            PurposeList.Add(new SelectListItem { Value = "Chargable", Text = "Chargable" });
            PurposeList.Add(new SelectListItem { Value = "P.O.C", Text = "P.O.C" });
            PurposeList.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
            PurposeList.Add(new SelectListItem { Value = "Return", Text = "Return" });
            PurposeList.Add(new SelectListItem { Value = "Others", Text = "Others" });
            ViewData["Purpose"] = PurposeList;

            List<SelectListItem> JobStatusList = new List<SelectListItem>();
            JobStatusList.Add(new SelectListItem { Value = "Completed", Text = "Completed" });
            JobStatusList.Add(new SelectListItem { Value = "Followup Required", Text = "Followup Required" });
            JobStatusList.Add(new SelectListItem { Value = "Recommendation Requied", Text = "Recommendation Required" });
            JobStatusList.Add(new SelectListItem { Value = "Escalated to Ext. Support", Text = "Escalated to Ext. Support" });
            ViewData["JobStatusList"] = JobStatusList;
            //update database
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            User user = context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin" || (user.FirstName + user.LastName) == context.getServiceReport(report.SerialNumber).ReportFrom))
            {
                return RedirectToAction("Login", "Users");
            }
            if (!ModelState.IsValid)
            {
                return View(report);
            }

            if (ModelState.IsValid)
            {
                double totalmshremain = context.GetRemainingMSHByCompany(report);
                Debug.WriteLine("Debug from post editreport: Total MSH Remaining : " + totalmshremain);
                double calculatedhours = context.CalculateMSH(report.TimeStart, report.TimeEnd);
                //ModelState.AddModelError("", "The calculated MSH:" + calculatedhours);
                //return View(model);
                if (totalmshremain < calculatedhours)
                {
                    ModelState.AddModelError("", "The company you have selected does not have enough remaining MSH, Please contact your Boss immediately regarding this issue.");
                    return View(report);

                }
                

                if (report.TimeStart > DateTime.Now || report.TimeEnd > DateTime.Now)
                { 
                    ModelState.AddModelError("", "Please enter a report after the service is rendered");
                    return View(report);
                }
                //questionable
                if (!(report.TimeStart.CompareTo(report.TimeEnd) <= 0))
                {
                    ModelState.AddModelError("", "your start time should be before your end time");
                    return View(report);
                }
            Debug.WriteLine("This is submitting" + report.TimeEnd);
               
            context.ReportEdit(report);
            context.LogAction("Service Report", "Service Report (SRN: " + report.SerialNumber + ") has been edited.", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            return RedirectToAction("Report", new { id = report.SerialNumber });
            }
            
            return View(report);
        }

        
        public IActionResult ReportConfirm(int id)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            User user = context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin"))
            {
                return RedirectToAction("Error", "Admin");
            }
            ServiceReport meh = context.getServiceReport(id);
            double totalmshremain = context.GetRemainingMSHByCompany(meh);
            if (totalmshremain < meh.MSHUsed)
            {
                TempData["error"] = "The company you are trying to confirm currently does not have enough remaining MSH.";

                return RedirectToAction("Report",new { id });

            }

            context.LogAction("Service Report", "Service Report (SRN: " + id + ") has been confirmed.", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            context.ReportConfirm(id);
            //if (tf == false)
            //{
            //    ViewBag.Error = "Error with Confirming Report. Please try again.";
            //    return View();
            //}
            
            ServiceReport remains = context.SubtractMSHUsingSR(context.getServiceReport(id));

            while (remains.MSHUsed != 0)
            {
                remains = context.SubtractMSHUsingSR(remains);
            }
            
            Debug.WriteLine("hi id = " + id);
            return  RedirectToAction("ViewReports");
        }

        public IActionResult ReportDelete(int id)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            //checkowner
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            //enter user
             User user = context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID")));
            if (!(HttpContext.Session.GetString("AdminValidity") == "Admin" || HttpContext.Session.GetString("AdminValidity") == "Super Admin" || (user.FirstName + user.LastName) == context.getServiceReport(id).ReportFrom))
            {
                return RedirectToAction("Login", "Users");
            }
            if(context.getServiceReport(id).ReportStatus != "Confirmed")
            {
                context.DeleteReport(id);
            }
            
            context.LogAction("Service Report", "Service Report (SRN: " + id + ") has been deleted.", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            return RedirectToAction("ViewReports");
        }


        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult PrintReport(int id)
        {

            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ServiceReport model = context.getServiceReport(id);
            if (model.SerialNumber == 0 )
            {
                return RedirectToAction("ViewReports");
            }
            string[] m = model.JobStatus;

            string jobcombined = "";
            if (model.JobStatus.Length > 1)
            {       
                for (int i = 0; i < model.JobStatus.Length; i++)
                {
                    if (model.JobStatus[i] != model.JobStatus.Last())
                    {
                        jobcombined += model.JobStatus[i] + ",";
                    }
                    else
                    {
                        jobcombined += model.JobStatus[i];
                    }

                }

            }
            else if (model.JobStatus.Length == 1)
            {
                jobcombined = model.JobStatus[0];
            }
            model.JobStat = jobcombined;
            HttpContext.JsReportFeature()
                .Recipe(Recipe.PhantomPdf);

            return View(model);
        }

        
        [HttpPost]
        public async System.Threading.Tasks.Task<string> ReturnPostalAsync (string postal, GoogleAddressType type){
            Debug.WriteLine("postal" + postal);
            GoogleGeocoder geocoder = new GoogleGeocoder();
            geocoder.ApiKey = "AIzaSyCY8FWydp6zky0N4TVk44x5xao2JjBFios";
            IEnumerable<GoogleAddress> addresses = await geocoder.GeocodeAsync("Singapore" + postal);
            Debug.WriteLine(addresses.First().Coordinates.Latitude + ": :" + addresses.First().Coordinates.Longitude); 
            IEnumerable<GoogleAddress> reverse = await geocoder.ReverseGeocodeAsync(addresses.First().Coordinates.Latitude,addresses.First().Coordinates.Longitude);
            Debug.WriteLine(addresses.First().FormattedAddress);
            Debug.WriteLine(reverse.First().FormattedAddress);
            return reverse.First().FormattedAddress;

        }

        [HttpGet]
        public IActionResult AddBilling(int id)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            
            ServiceReport model = context.getServiceReport(id);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBilling(ServiceReport model)
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                if (model.Labour < 0 || model.Others < 0 || model.Parts < 0 || model.Transport <0)
                {
                    ModelState.AddModelError("", "Costs cannot be negative");
                    return View(model);
                }
                if(model.InvoiceNo < 0)
                {
                    ModelState.AddModelError("", "Invoice number cannot be negative");
                    return View(model);
                }
                
                
                CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                context.AddBilling(model);

                return RedirectToAction("Report", model.SerialNumber);
            }
            ModelState.AddModelError("","Error have occured");
            return View(model);
        }
    }
}