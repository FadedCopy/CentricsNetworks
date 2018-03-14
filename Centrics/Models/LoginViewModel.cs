using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }

        [Display(Name = "Password"), Required, StringLength(30), DataType(DataType.Password)]
        public string UserPassword { get; set; }
    }
}
