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

        //[Display(Name = "Address 2"),StringLength(100,ErrorMessage = "Maximum character space for address is 100 only")]
        //public string ClientAddress2 { get; set; }

        //not required? (condition for Req: proper report)
        [Display(Name = "Tel / HP")] //Regular Expression? Max Length 8?
        public int ClientTel { get; set; }

        //not required? (condition for Req: proper report)
        [Display(Name = "Contact Person")]
        public string ClientContactPerson { get; set; }
        #endregion

        #region service-related
        //Service Details
        //
        [Display(Name = "Purpose of Visit")]
        [Required]
        public string[] PurposeOfVisit { get; set; } //select or option?

        [Display(Name = "Description"), Required, StringLength(3000, ErrorMessage = "Maximum word limit (3000) exceeded ")]
        public string Description { get; set; }

        [Display(Name = "Remarks"), StringLength(1000, ErrorMessage = "Maximum word limit (1000) exceeded")]
        public string Remarks { get; set; }

        //Service Time info

        //[Display(Name = "Date"), Required, DataType(DataType.Date)]
        //public DateTime Date { get; set; }

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

        #region Acknowledgement Questionable here commented out
        //TBC
        //AttendedBy and when


        ////TBC
        ////Client Acknowledgement
        //[Display(Name = "Name")]
        //public string ClientName { get; set; }

        //[Display(Name = "Date")]
        //public DateTime ClientSignedDate { get; set; }


        #endregion

        #region Billing Seperated?
        [Display(Name = "Labour")]
        public double Labour { get; set; }

        [Display(Name = "Transport")]
        public double Transport { get; set; }

        [Display(Name = "Parts")]
        public double Parts { get; set; }

        [Display(Name = "Others")]
        public double Others { get; set; }

        //TBC
        //Invoice Details
        [Display(Name = "Invoice No")]
        public int InvoiceNo { get; set; }


        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InvoiceDate { get; set; }


        //servicerating
        //[Display(Name = "please rate our service")]
        //public bool Servicerating { get; set; }
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

        public string[] Address1Array { get; set; }
        public string[] Address2Array { get; set; }
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