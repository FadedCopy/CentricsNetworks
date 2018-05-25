using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Centrics.Models
{
    public class ServiceReport
    {

        [Display(Name = "SRN")]
        public int SerialNumber { get; set; }

        #region client-related
        //To render the companies from contact from db
        public List<SelectListItem> Companies { get; set;  }

        //Client Details
        [Display(Name = "Company Name"), Required(ErrorMessage = "Please enter a company name", AllowEmptyStrings = false)]
        public string ClientCompanyName { get; set; }

        [Display(Name = "Address"), StringLength(100, ErrorMessage = "Maximum character space for address is 100 only"), Required(ErrorMessage = "Enter the address of the company")]
        public string ClientAddress { get; set; }
        
        [Display(Name = "Tel / HP")] 
        public int ClientTel { get; set; }

        [Display(Name = "Contact Person")]
        public string ClientContactPerson { get; set; }
        #endregion

        #region service-related
        
        [Display(Name = "Purpose of Visit")]
        [Required]
        public string[] PurposeOfVisit { get; set; }

        [Display(Name = "Description"), Required, StringLength(3000, ErrorMessage = "Maximum word limit (3000) exceeded ")]
        public string Description { get; set; }

        [Display(Name = "Remarks"), StringLength(1000, ErrorMessage = "Maximum word limit (1000) exceeded")]
        public string Remarks { get; set; }

        //Service Time info
        [Display(Name = "Time Start"), DataType(DataType.DateTime),Required, DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime TimeStart { get; set; }
        
        //DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true),
        [Display(Name = "Time End") ,DataType(DataType.DateTime),Required]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy hh:mm}",ApplyFormatInEditMode = true)]
        public DateTime TimeEnd { get; set; }

        [Display(Name = "MSH Used"), Required(ErrorMessage = "Please enter how much MSH is used")] // ???
        public double MSHUsed { get; set; }
        #endregion

        [Display(Name = "Attended by Staff/s"),Required(ErrorMessage = "Please enter the name of the attending ")]
        public string AttendedByStaffName { get; set; }

        [Display(Name = "Attended on Date"), DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:d}",ApplyFormatInEditMode = true)] 
        public DateTime AttendedOnDate { get; set; }

        #region Billing Seperated?
        [Display(Name = "Labour")]
        public double Labour { get; set; }

        [Display(Name = "Transport")]
        public double Transport { get; set; }

        [Display(Name = "Parts")]
        public double Parts { get; set; }

        [Display(Name = "Others")]
        public double Others { get; set; }

        //Invoice Details
        [Display(Name = "Invoice No")]
        public int InvoiceNo { get; set; }
        
        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InvoiceDate { get; set; }


       
        #endregion

        //JobStatus
        [Display(Name = "Job Status"), Required]
        public string[] JobStatus { get; set; }

        public string JobStat { get; set; }

        public string ReportFrom { get; set; }

        public string DateRecorded { get; set; }

        public string Purpose { get; set; }

        public string ReportStatus { get; set; }

        [Display(Name = "Address Searcher")]
        public string PostalCode { get; set; }
    }


    public class SRCalculation
    {
        [Display(Name = "Before 9 Multiplier")]
        [Required(ErrorMessage ="Please fill in Before 9 Multiplier")]
        public double From12amto9am { get; set; }

        [Display(Name ="After 6 Multiplier")]
        [Required(ErrorMessage = "Please fill in After 6 Multilier ")]
        public double After6pmto12am { get; set; }

        [Display(Name = "Saturday Multiplier")]
        [Required(ErrorMessage = "Please fill in Saturday Multiplier")]
        public double SaturdayMultiplier { get; set; }

        [Display(Name = "Sunday Multiplier")]
        [Required(ErrorMessage = "Please fill in SundayMultiplier")]
        public double SundayMultiplier { get; set; }
    }
}

//public class Billing{

//    ////Billing Details
//    [Display(Name = "Labour")]
//    public double Labour { get; set; }

//    [Display(Name = "Transport")]
//    public double Transport { get; set; }

//    [Display(Name = "Parts")]
//    public double Parts { get; set; }

//    [Display(Name = "Others")]
//    public double Others { get; set; }

//    //TBC
//    //Invoice Details
//    [Display(Name = "Invoice No"),MaxLength(20,ErrorMessage = "Exceeded 20 character")]
//    public int InvoiceNo { get; set; }

//    [Display(Name = "Invoice Date")]
//    public DateTime InvoiceDate { get; set; }
//}