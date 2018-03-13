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

        [Display(Name = "Password"), Required, StringLength(30), DataType(DataType.Password),
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z])\S{8,15}$", ErrorMessage = "Password must contain: Minimum 8 characters at least 1 uppercase alphabet, 1 lowercase alphabet and 1 numerical value.")]
        public string UserPassword { get; set; }
    }
}
