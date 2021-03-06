﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class LoginViewModel
    {
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }

        [Display(Name = "Password"), Required, StringLength(30), DataType(DataType.Password)]
        public string UserPassword { get; set; }
        
        public Boolean SuccessfulLogin { get; set; }

    }

    public class TwoFactorAuth
    {
        [Required]
        public int CodeDigit { get; set; }
    }
}
