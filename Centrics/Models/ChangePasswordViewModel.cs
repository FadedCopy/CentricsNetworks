﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class ChangePasswordViewModel
    {

        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "Current Password"), Required, StringLength(30), DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password"), Required, StringLength(30), DataType(DataType.Password),
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z])\S{8,30}$", ErrorMessage = "Password must contain: Minimum 8 characters at least 1 uppercase alphabet, 1 lowercase alphabet, 1 numerical value and 1 symbol.")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm New Password"), Required, StringLength(30), Compare("NewPassword", ErrorMessage = "Confirm password and password do not match"), DataType(DataType.Password)]
        public string CfmNewPassword { get; set; } 
    }
}
