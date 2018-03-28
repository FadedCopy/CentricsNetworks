using Centrics.Models;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Centrics.Controllers
{
    public class ServiceReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddNewReport()
        {
            //this is for pulling out data from contract to service report
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ServiceReport model = new ServiceReport();
            ViewData["Companies"] = context.GetClientByContract();

            List <SelectListItem> PurposeList = new List<SelectListItem>();
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
            model.SerialNumber = context.getReportCounts() + 1;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AddNewReport(ServiceReport model)
        {
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
            #endregion
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ModelState.IsValid)
            {
                double totalmshremain = context.GetRemainingMSHByCompany(model);
                if (totalmshremain < model.MSHUsed)
                {
                    ModelState.AddModelError("","The company you have selected does not have enough remaining MSH, Please contact your Boss immediately regarding this issue.");
                    return View(model);

                }
                //questionable
                //if(!(model.TimeStart.CompareTo(model.TimeEnd) < 0))
                //{
                //    ModelState.AddModelError("", "your start time should be before your end time");
                //    return View(model);
                //}
                context.AddServiceReport(model);
                return RedirectToAction("ViewReports","ServiceReport");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewReports()
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["Pending"] = context.getPendingReports();
            ViewData["Confirmed"] = context.getConfirmedReports();

            return View();
        }

        [HttpPost]
        public IActionResult ViewReports(int page)
        {
            return RedirectToAction("Report",page);
        }

        [HttpGet]
        public IActionResult Report(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("ViewReports");
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
ServiceReport model = context.getServiceReport(id);
            
            return View(model);
        }

        

        [HttpGet]
        public IActionResult EditReport(int id)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReport(ServiceReport report)
        {
            //update database
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            
            double totalmshremain = context.GetRemainingMSHByCompany(report);
            if (totalmshremain < report.MSHUsed)
            {
                ModelState.AddModelError("", "The company you are trying to edit does not have enough remaining MSH, Please contact your Boss immediately regarding this issue.");
                return View(report);

            }
            context.ReportEdit(report);
            return RedirectToAction("Report", report.SerialNumber);
        }

        
        public IActionResult ReportConfirm(int id)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ServiceReport meh = context.getServiceReport(id);
            double totalmshremain = context.GetRemainingMSHByCompany(meh);
            if (totalmshremain < meh.MSHUsed)
            {
                TempData["error"] = "The company you are trying to confirm currently does not have enough remaining MSH.";

                return RedirectToAction("Report",new { id });

            }

            context.ReportConfirm(id);
            //if (tf == false)
            //{
            //    ViewBag.Error = "Error with Confirming Report. Please try again.";
            //    return View();
            //}
            
            double remains = context.SubtractMSHUsingSR(context.getServiceReport(id));

            if(remains > 0)
            {
                context.SubtractRemains(remains, context.getServiceReport(id));
                //wait step context copy code to create method
            }
            Debug.WriteLine("hi id = " + id);
            return  RedirectToAction("ViewReports");
        }

        [HttpPost]
        public IActionResult ReportDelete(int id, string confirm)
        {
            if(confirm == id + "Confirm".ToUpper())
            {
                //DB set status to remove/ remove from db
                return RedirectToAction("ViewReports");
            }

            return RedirectToAction("Report", id);
        }


        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult PrintReport(int id)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ServiceReport model = context.getServiceReport(id);
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
            HttpContext.JsReportFeature().Recipe(Recipe.PhantomPdf);
            
            return View(model);
        }

        
        [HttpGet]
        public IActionResult AddBilling(int id)
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            
            ServiceReport model = context.getServiceReport(id);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBilling(ServiceReport model)
        {
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