using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;

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
            //Generate this from DB not static like now
            int SRN = 1;
            ViewData["SRN"] = SRN;

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

            return View();
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
            #endregion
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (ModelState.IsValid)
            {
                for (int i = 0; i < model.PurposeOfVisit.Length; i++) {
                    Debug.WriteLine(model.PurposeOfVisit[i].ToString());
                }

                Debug.WriteLine(model.Description);

                context.AddServiceReport(model);
                return RedirectToAction("ViewReports","ServiceReport");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ViewReports()
        {
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            ViewData["Pending"] = context.getPendingReports();
            ViewData["Confirmed"] = context.getConfirmedReports();

            return View();
        }


    }
}