﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class User
    {
        //User Details for Login & Registration
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "First Name"), Required, StringLength(20)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name"), Required, StringLength(20)]
        public string LastName { get; set; }

        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail{ get; set; }

        [Display(Name = "Password"), Required, StringLength(30), DataType(DataType.Password), 
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z])\S{8,30}$", ErrorMessage = "Password must contain: Minimum 8 characters at least 1 uppercase and 1 lowercase alphabet, 1 numerical value and 1 symbol.")]
        public string UserPassword { get; set; }

        [Display(Name = "Confirm Password"), Required, StringLength(30), Compare("UserPassword", ErrorMessage = "Confirm password and password do not match"), DataType(DataType.Password)]
        public string UserCfmPassword { get; set; }

        [Display(Name = "Role"), StringLength(40)]
        public string UserRole { get; set; }

        public Boolean Authenticated { get; set; }

        public List<SelectListItem> Roles { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "User", Text = "User" },
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Super Admin", Text = "Super Admin"},
        };

        public List<SelectListItem> RolesNoSuperAdmin { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "User", Text = "User" },
            new SelectListItem { Value = "Admin", Text = "Admin" },
        };
    }
}
