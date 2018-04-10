using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class Log
    {
        [Display(Name = "Log ID")]
        public int LogID { get; set; }

        [Display(Name = "Type")]
        public string Type { get; set; }

        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "Email"), DataType(DataType.EmailAddress)]
        public string UserEmail{ get; set; }

        [Display(Name = "Action Performed")]
        public string ActionPerformed { get; set; }

        [Display(Name = "Date & Time of Action")]
        public DateTime DateTimePerformed { get; set; }
    }

}
