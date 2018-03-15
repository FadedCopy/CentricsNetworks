using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class EditUserViewModel
    {
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Display(Name = "First Name"), Required, StringLength(20)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name"), Required, StringLength(20)]
        public string LastName { get; set; }

        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }

        [Display(Name = "Role"), Required, StringLength(40)]
        public string UserRole { get; set; }

        public List<SelectListItem> Roles { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "User", Text = "User" },
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Super Admin", Text = "Super Admin"},
        };
    }
}
