using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class ServiceReport
    {

        [Display(Name = "SRN"), Required]
        public int SerialNumber { get; set; }

        #region client-related
        //Client Details
        [Display(Name = "Company Name"), Required]
        public string ClientCompanyName { get; set; }

        [Display(Name = "Address"), StringLength(200, ErrorMessage = "Maximum character space for address is 200 only"), Required]
        public string ClientAddress { get; set; }

        //not required?
        [Display(Name = "Tel / HP"), MaxLength(8)] //Regular Expression? Max Length 8?
        public int ClientTel { get; set; }

        //not required?
        [Display(Name = "Contact Person")]
        public string ClientContactPerson { get; set; }
        #endregion

        #region service-related
        //Service Details
        [Display(Name = "Purpose of Visit"), Required]
        public string[] PurposeOfVisit { get; set; } //select or option?

        [Display(Name = "Description"), Required, StringLength(3000, ErrorMessage = "Maximum word limit (3000) exceeded ")]
        public string Description { get; set; }

        [Display(Name = "Remarks"), StringLength(1000, ErrorMessage = "Maximum word limit (1000) exceeded")]
        public string Remarks { get; set; }

        //Service Time info
        [Display(Name = "Date"), Required, DataType(DataType.Date),DisplayFormat(DataFormatString ="{0:dd/MM/yyyy}"),RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$")]
        public DateTime Date { get; set; }

        [Display(Name = "Time Start"), Required, DataType(DataType.Time),RegularExpression(@"([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Please enter a valid 24 Hour format time")]
        public DateTime TimeStart { get; set; }

        [Display(Name = "Time End"), Required, DataType(DataType.Time), RegularExpression(@"([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Please enter a valid 24 Hour format time")]
        public DateTime TimeEnd { get; set; }

        [Display(Name = "MSH Used"), Required] // ???
        public double MSHUsed { get; set; }
        #endregion

        [Display(Name = "Name")]
        public string AttendedByStaffName { get; set; }

        [Display(Name = "Date"),Required, DataType(DataType.Date)]
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

        #region Billing Seperated? commented out
        ////Billing Details
        //[Display(Name = "Labour")]
        //public double Labour { get; set; }

        //[Display(Name = "Transport")]
        //public double Transport { get; set; }

        //[Display(Name = "Parts")]
        //public double Parts { get; set; }

        //[Display(Name = "Others")]
        //public double Others { get; set; }

        ////TBC
        ////Invoice Details
        //[Display(Name = "Invoice No")]
        //public int InvoiceNo { get; set; }

        //[Display(Name = "Invoice Date")]
        //public DateTime InvoiceDate {get;set;}



        ////ServiceRating
        //[Display(Name = "Please rate our service")]
        //public string ServiceRating { get; set; }
        #endregion

        //JobStatus
        [Display(Name = "Job Status"), Required]
        public string[] JobStatus { get; set; }
    }
}

