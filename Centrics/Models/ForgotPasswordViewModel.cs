using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class ForgotPasswordViewModel
    {
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }
    }
}
